using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[System.Serializable]
public class EnemyPoolData
{
    public EnemyData enemyData;
    public int defaultCapacity = 10;
    public int maxSize = 20;
    [HideInInspector] public ObjectPool<Enemy> pool;
    [HideInInspector] public Transform poolParent;
}

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }
    
    [Header("Pool Settings")]
    [SerializeField] private List<EnemyPoolData> enemyPools = new();
    [SerializeField] private Transform poolContainer;
    
    [Header("Runtime Info")]
    private Dictionary<EnemyData, EnemyPoolData> poolDictionary = new();
    private List<Enemy> activeEnemies = new();
    
    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializePools();
    }
    
    /// <summary>
    /// 풀 초기화
    /// </summary>
    private void InitializePools()
    {
        // 풀 컨테이너 생성
        if (poolContainer == null)
        {
            GameObject container = new GameObject("Enemy Pool Container");
            container.transform.SetParent(transform);
            poolContainer = container.transform;
        }
        
        // 각 적 타입별로 풀 생성
        foreach (var poolData in enemyPools)
        {
            if (poolData.enemyData?.enemyPrefab == null) continue;
            
            CreatePoolForEnemyType(poolData);
            poolDictionary[poolData.enemyData] = poolData;
        }
        
        Debug.Log($"[EnemyPoolManager] {poolDictionary.Count}개 적 타입 풀 초기화 완료");
    }
    
    /// <summary>
    /// 특정 적 타입에 대한 Unity Object Pool 생성
    /// </summary>
    private void CreatePoolForEnemyType(EnemyPoolData poolData)
    {
        // 각 적 타입별 부모 오브젝트 생성
        GameObject poolParent = new GameObject($"Pool_{poolData.enemyData.enemyName}");
        poolParent.transform.SetParent(poolContainer);
        poolData.poolParent = poolParent.transform;
        
        // Unity ObjectPool 생성
        poolData.pool = new ObjectPool<Enemy>(
            createFunc: () => CreateNewEnemy(poolData),
            actionOnGet: (enemy) => OnGetEnemy(enemy, poolData.enemyData),
            actionOnRelease: (enemy) => OnReleaseEnemy(enemy, poolData.poolParent),
            actionOnDestroy: (enemy) => DestroyEnemy(enemy),
            collectionCheck: true,  // 중복 반환 체크 활성화
            defaultCapacity: poolData.defaultCapacity,
            maxSize: poolData.maxSize
        );
        
        Debug.Log($"[EnemyPoolManager] {poolData.enemyData.enemyName} 풀 생성 완료 (용량: {poolData.defaultCapacity}, 최대: {poolData.maxSize})");
    }
    
    /// <summary>
    /// 새로운 Enemy 생성 (Pool의 createFunc)
    /// </summary>
    private Enemy CreateNewEnemy(EnemyPoolData poolData)
    {
        GameObject enemyObj = Instantiate(poolData.enemyData.enemyPrefab, poolData.poolParent);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        
        if (enemy == null)
        {
            Debug.LogError($"{poolData.enemyData.enemyName} Enemy 컴포넌트 없음 !!!");
            Destroy(enemyObj);
            return null;
        }
        
        return enemy;
    }
    
    /// <summary>
    /// 풀에서 Enemy를 가져올 때 (Pool의 actionOnGet)
    /// </summary>
    private void OnGetEnemy(Enemy enemy, EnemyData enemyData)
    {
        if (enemy == null) return;
        
        // 게임 오브젝트 활성화
        enemy.gameObject.SetActive(true);
        
        // Enemy 초기화 (overrideController 포함)
        enemy.InitializeFromPool(enemyData);
        
        // 활성 리스트에 추가
        activeEnemies.Add(enemy);
        
        Debug.Log($"[EnemyPoolManager] {enemyData.enemyName} 풀에서 가져옴 (활성: {activeEnemies.Count})");
    }
    
    /// <summary>
    /// Enemy를 풀로 반환할 때 (Pool의 actionOnRelease)
    /// </summary>
    private void OnReleaseEnemy(Enemy enemy, Transform poolParent)
    {
        if (enemy == null) return;
        
        // 활성 리스트에서 제거
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
        
        // 게임 오브젝트 비활성화 및 부모 설정
        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(poolParent);
        
        Debug.Log($"[EnemyPoolManager] {enemy.name} 풀로 반환됨 (활성: {activeEnemies.Count})");
    }
    
    /// <summary>
    /// Enemy 파괴 (Pool의 actionOnDestroy)
    /// </summary>
    private void DestroyEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            Destroy(enemy.gameObject);
        }
    }
    
    /// <summary>
    /// StageData로부터 풀 동적 생성
    /// </summary>
    public void InitializeFromStageData(StageData stageData)
    {
        if (stageData?.enemyPool == null) return;
        
        foreach (var enemyData in stageData.enemyPool)
        {
            if (enemyData == null) continue;
            
            // 이미 풀이 있으면 스킵
            if (poolDictionary.ContainsKey(enemyData)) continue;
            
            // 새로운 풀 데이터 생성
            EnemyPoolData newPoolData = new EnemyPoolData
            {
                enemyData = enemyData,
                defaultCapacity = enemyData.type == EnemyType.Boss ? 1 : 5,
                maxSize = enemyData.type == EnemyType.Boss ? 2 : 10
            };
            
            CreatePoolForEnemyType(newPoolData);
            poolDictionary[enemyData] = newPoolData;
            
            Debug.Log($"[EnemyPoolManager] 동적 풀 생성: {enemyData.enemyName}");
        }
    }
    
    /// <summary>
    /// 특정 EnemyData로 적 가져오기
    /// </summary>
    public Enemy GetEnemy(EnemyData enemyData, Vector3 position, Quaternion rotation)
    {
        if (enemyData == null)
        {
            Debug.LogError("[EnemyPoolManager] EnemyData 없음 !!!");
            return null;
        }
        
        // 풀이 없으면 동적 생성
        if (!poolDictionary.ContainsKey(enemyData))
        {
            CreateDynamicPool(enemyData);
        }
        
        EnemyPoolData poolData = poolDictionary[enemyData];
        
        // Unity ObjectPool에서 가져오기
        Enemy enemy = poolData.pool.Get();
        
        if (enemy != null)
        {
            // 위치/회전 설정
            enemy.transform.position = position;
            enemy.transform.rotation = rotation;
        }
        
        return enemy;
    }
    
    /// <summary>
    /// 적을 풀로 반환
    /// </summary>
    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        
        // 어떤 풀에 속하는지 찾기
        EnemyPoolData targetPool = FindPoolForEnemy(enemy);
        if (targetPool?.pool != null)
        {
            // Unity ObjectPool로 반환
            targetPool.pool.Release(enemy);
        }
        else
        {
            // 풀을 찾을 수 없으면 활성 리스트에서만 제거하고 파괴
            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
            }
            Debug.LogWarning($"[EnemyPoolManager] {enemy.name}의 풀 없음 !!!");
            Destroy(enemy.gameObject);
        }
    }
    
    /// <summary>
    /// 동적으로 풀 생성
    /// </summary>
    private void CreateDynamicPool(EnemyData enemyData)
    {
        EnemyPoolData newPoolData = new EnemyPoolData
        {
            enemyData = enemyData,
            defaultCapacity = enemyData.type == EnemyType.Boss ? 1 : 3,
            maxSize = enemyData.type == EnemyType.Boss ? 2 : 6
        };
        
        CreatePoolForEnemyType(newPoolData);
        poolDictionary[enemyData] = newPoolData;
    }
    
    /// <summary>
    /// 특정 적이 어떤 풀에 속하는지 찾기
    /// </summary>
    private EnemyPoolData FindPoolForEnemy(Enemy enemy)
    {
        Transform parent = enemy.transform.parent;
        if (parent == null) return null;
        
        foreach (var poolData in poolDictionary.Values)
        {
            if (poolData.poolParent == parent)
            {
                return poolData;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 특정 타입의 적 가져오기 (편의 메서드)
    /// </summary>
    public Enemy GetEnemyByType(EnemyType enemyType, Vector3 position, Quaternion rotation = default)
    {
        foreach (var kvp in poolDictionary)
        {
            if (kvp.Key.type == enemyType)
            {
                return GetEnemy(kvp.Key, position, rotation);
            }
        }
        
        Debug.LogError($"[EnemyPoolManager] {enemyType} 타입의 적을 찾을 수 없습니다!");
        return null;
    }
    
    /// <summary>
    /// 모든 활성 적 반환
    /// </summary>
    public void ReturnAllActiveEnemies()
    {
        // 리스트를 복사해서 순회 (ReturnEnemy에서 리스트 수정하므로)
        List<Enemy> enemiesToReturn = new List<Enemy>(activeEnemies);
        
        foreach (var enemy in enemiesToReturn)
        {
            ReturnEnemy(enemy);
        }
        
        Debug.Log("[EnemyPoolManager] 모든 활성 적을 풀로 반환했습니다.");
    }
    
    /// <summary>
    /// 풀 상태 정보 출력 (디버깅용)
    /// </summary>
    [ContextMenu("Print Pool Status")]
    public void PrintPoolStatus()
    {
        Debug.Log("=== Enemy Pool Status ===");
        foreach (var kvp in poolDictionary)
        {
            EnemyData enemyData = kvp.Key;
            EnemyPoolData poolData = kvp.Value;
            Debug.Log($"{enemyData.enemyName}: 풀 용량 {poolData.defaultCapacity}/{poolData.maxSize}, 활성 적 총 {activeEnemies.Count}개");
        }
    }
    
    /// <summary>
    /// 모든 풀 정리 (씬 전환할 경우)
    /// </summary>
    public void ClearAllPools()
    {
        ReturnAllActiveEnemies();
        
        foreach (var poolData in poolDictionary.Values)
        {
            poolData.pool?.Clear();
        }
        
        Debug.Log("[EnemyPoolManager] 모든 풀을 정리했습니다.");
    }
    
    // Public Getters
    public List<Enemy> ActiveEnemies => activeEnemies;
    public int ActiveEnemyCount => activeEnemies.Count;
    public Dictionary<EnemyData, EnemyPoolData> PoolDictionary => poolDictionary;
}