using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum StageState
{
    Loading,
    Playing,
    BossSpawned,
    Completed,
    Failed
}

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }
    
    [Header("Current Stage")]
    [SerializeField] private StageData currentStage;
    [SerializeField] private StageState currentState = StageState.Loading;
    
    [Header("Enemy Spawn")]
    [SerializeField] private float spawnInterval;           // 2초마다 스폰
    [SerializeField] private int maxEnemiesAtOnce;          // 동시 최대 적 수
    [SerializeField] private int enemiesPerSpawn;            // 한 번에 스폰할 적 수
    [SerializeField] private float spawnDistanceAhead;     // 플레이어 앞쪽에서 스폰
    [SerializeField] private float spawnRangeWidth;         // 좌우 스폰 범위
    [SerializeField] private LayerMask obstacleLayerMask = -1;   // 장애물 레이어 마스크
    
    [Header("Boss Spawn")]
    [SerializeField] private float bossSpawnDelay;
    [SerializeField] private float bossSpawnDistance;
    
    [Header("Stage Progress")]
    private int currentKillCount = 0;
    private int totalSpawnedCount = 0;
    private List<Enemy> activeEnemies = new();
    private Enemy currentBoss = null;
    
    // Events
    public static event Action<StageData> OnStageStart;
    public static event Action<StageData> OnStageComplete;
    public static event Action<StageData> OnStageFailed;
    public static event Action<int, int> OnKillCountChanged; // current, required
    public static event Action<Enemy> OnBossSpawned;
    public static event Action<Enemy> OnBossDefeated;
    
    private Coroutine spawnCoroutine;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #region Stage LifeCycle
    
    /// <summary>
    /// 새로운 스테이지 로드
    /// </summary>
    public void LoadStage(StageData stageData)
    {
        if (currentState == StageState.Playing)
        {
            Debug.LogWarning("스테이지가 이미 진행 중입니다!");
            return;
        }
        
        currentStage = stageData;
        ResetStageData();
        StartStage();
    }
    
    /// <summary>
    /// 현재 스테이지 시작
    /// </summary>
    private void StartStage()
    {
        if (currentStage == null)
        {
            Debug.LogError("스테이지 데이터 없음 !!!");
            return;
        }
        
        currentState = StageState.Playing;
        
        // Pool에 현재 스테이지 전달, 풀 초기화
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.InitializeFromStageData(currentStage);
        }
        
        // 스테이지 맵 생성
        CreateStageMap();
        
        // 적 스폰 시작
        spawnCoroutine = StartCoroutine(SpawnNormalEnemies());
        
        OnStageStart?.Invoke(currentStage);
        Debug.Log($"스테이지 시작: {currentStage.stageName}");
    }
    
    /// <summary>
    /// 스테이지 맵 생성
    /// </summary>
    private void CreateStageMap()
    {
        if (currentStage.stagePrefab == null) return;
        
        // 기존 맵 제거
        GameObject existingMap = GameObject.FindGameObjectWithTag("StageMap");
        if (existingMap != null)
        {
            Destroy(existingMap);
        }
        
        // 새 맵 생성
        GameObject stageMap = Instantiate(currentStage.stagePrefab);
        stageMap.tag = "StageMap";
        
        Debug.Log($"스테이지 맵 생성: {stageMap.name}");
    }
    
    /// <summary>
    /// 스테이지 완료 처리
    /// </summary>
    private void CompleteStage()
    {
        currentState = StageState.Completed;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        
        // 스테이지 완료 보상
        GiveStageRewards();
        
        OnStageComplete?.Invoke(currentStage);
        Debug.Log($"스테이지 완료: {currentStage.stageName}");
    }
    
    /// <summary>
    /// 스테이지 데이터 리셋
    /// </summary>
    private void ResetStageData()
    {
        currentKillCount = 0;
        totalSpawnedCount = 0;
        currentBoss = null;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        
        ClearAllActiveEnemies();
    }

    #endregion
    
    #region MonsterSpawn
    
    /// <summary>
    /// 일반 몬스터 스폰 코루틴
    /// </summary>
    private IEnumerator SpawnNormalEnemies()
    {
        while (currentState == StageState.Playing)
        {
            if (activeEnemies.Count < maxEnemiesAtOnce)
            {
                int spawnCount = Mathf.Min(enemiesPerSpawn, maxEnemiesAtOnce - activeEnemies.Count);

                for (int i = 0; i < spawnCount; i++)
                {
                    SpawnNormalEnemy();
                    totalSpawnedCount++;
                    yield return new WaitForSeconds(0.2f);
                }
            }
            
            yield return new WaitForSeconds(spawnInterval);
        }

        if (currentState == StageState.Playing)
        {
            yield return StartCoroutine(WaitAndSpawnBoss());
        }
    }
    
    /// <summary>
    /// 일반 몬스터 스폰
    /// </summary>
    private void SpawnNormalEnemy()
    {
        Player player = CharacterManager.Player;
        if (player == null) return;

        Vector3 spawnPosition = GetValidSpawnPosition(player.transform);
        if (spawnPosition == Vector3.zero) return;
        
        EnemyData normalEnemyData = GetRandomNormalEnemyData();
        if (normalEnemyData == null) return;
        
        // EnemyPoolManager에서 적 가져오기
        Enemy enemy = PoolManager.Instance.GetEnemy(normalEnemyData, spawnPosition, Quaternion.identity);
        
        if (enemy != null)
        {
            activeEnemies.Add(enemy);
            StartCoroutine(WatchEnemyDeath(enemy));
        }
    }

    /// <summary>
    /// 보스 스폰 대기 및 실행
    /// </summary>
    private IEnumerator WaitAndSpawnBoss()
    {
        // 남은 일반 몬스터 처치될 때까지 대기
        
        yield return new WaitForSeconds(bossSpawnDelay);

        if (currentState == StageState.Playing)
        {
            ClearRemainingEnemies();
            
            SpawnBoss();
        }
    }

    /// <summary>
    /// 보스 몬스터 스폰
    /// </summary>
    private void SpawnBoss()
    {
        Player player = CharacterManager.Player;
        
        if (!player) return;
        
        // 플레이어 앞쪽 멀리에 보스 스폰
        Vector3 spawnPosition = GetBossSpawnPosition(player.transform);
        if (spawnPosition == Vector3.zero) return;
        
        // 해당 Stage 보스 데이터
        EnemyData bossData = GetBossEnemyData();
        if (bossData == null)
        {
            Debug.LogError("보스 데이터 찾을 수 없음 !!!");
            return;
        }
        
        Enemy boss = PoolManager.Instance.GetEnemy(bossData, spawnPosition, Quaternion.identity);

        if (boss == null) return;
        
        currentBoss = boss;
        activeEnemies.Add(boss);
        currentState = StageState.BossSpawned;
        
        StartCoroutine(WatchBossDeath(boss));
        
        OnBossSpawned?.Invoke(boss);
        Debug.Log($"보스 등장: {bossData.enemyName}");
    }
    
    #endregion

    /// <summary>
    /// 랜덤한 일반 몬스터 데이터 가져오기
    /// </summary>
    private EnemyData GetRandomNormalEnemyData()
    {
        if (currentStage?.enemyPool == null) return null;

        List<EnemyData> normalEnemies = new List<EnemyData>();
        
        foreach (var enemyData in currentStage.enemyPool)
        {
            if (enemyData && enemyData.type == EnemyType.Normal)
            {
                normalEnemies.Add(enemyData);
            }
        }

        if (normalEnemies.Count > 0)
        {
            return normalEnemies[Random.Range(0, normalEnemies.Count)];
        }
        
        // 일반 몬스터가 없으면 첫 번째 적 사용
        Debug.LogWarning("⚠일반 몬스터 데이터가 없음 !!!");
        return currentStage.enemyPool.Length > 0 ? currentStage.enemyPool[0] : null;
    }
    
    /// <summary>
    /// 플레이어 앞쪽에서 유효한 스폰 위치 찾기
    /// </summary>
    private Vector3 GetValidSpawnPosition(Transform player)
    {
        Vector3 playerPos = player.position;
        Vector3 playerForward = player.forward;
        
        // 최대 10번 시도
        for (int i = 0; i < 10; i++)
        {
            // 플레이어 앞쪽 거리에서 좌우 랜덤 위치 계산
            float randomOffset = Random.Range(-spawnRangeWidth, spawnRangeWidth);
            Vector3 rightVector = Vector3.Cross(playerForward, Vector3.up).normalized;
            
            Vector3 candidatePos = playerPos + 
                                   playerForward * spawnDistanceAhead + 
                                   rightVector * randomOffset;
                                 
            // 땅 높이로 조정
            if (Physics.Raycast(candidatePos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            {
                candidatePos.y = hit.point.y;
            }
            
            // 장애물과 겹치는지 체크
            if (!IsPositionBlocked(candidatePos))
            {
                return candidatePos;
            }
        }
        
        Debug.LogWarning("유효한 스폰 위치를 찾을 수 없습니다!");
        return Vector3.zero;
    }
    
    /// <summary>
    /// 해당 위치에 장애물이 있는지 체크
    /// </summary>
    private bool IsPositionBlocked(Vector3 position)
    {
        // 적의 크기만큼 체크 (반지름 0.5f, 높이 2f 가정)
        return Physics.CheckCapsule(
            position + Vector3.up * 0.5f,      // 하단
            position + Vector3.up * 1.5f,      // 상단
            0.5f,                       // 반지름
            obstacleLayerMask               // 장애물 레이어
        );
    }
    
    /// <summary>
    /// StageData에서 보스 타입 적 찾기
    /// </summary>
    /// <returns>EnemyData 보스 데이터</returns>
    private EnemyData GetBossEnemyData()
    {
        if (currentStage?.enemyPool == null) return null;

        foreach (EnemyData enemyData in currentStage.enemyPool)
        {
            if (enemyData != null && enemyData.type == EnemyType.Boss)
            {
                return enemyData;
            }
        }
        
        // 보스가 없으면
        Debug.LogError("보스 데이터 없음 !!!");
        return currentStage.enemyPool.Length > 0 ? currentStage.enemyPool[0] : null;
    }
    
    /// <summary>
    /// 플레이어 기준 보스 스폰 위치 계산
    /// </summary>
    /// <param name="player"></param>
    /// <returns>Vector3 보스 스폰 위치</returns>
    private Vector3 GetBossSpawnPosition(Transform player)
    {
        Vector3 playerPos = player.position;
        Vector3 playerForward = player.forward;
        
        Vector3 bossSpawnPos = playerPos + playerForward * bossSpawnDistance;

        if (Physics.Raycast(bossSpawnPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
        {
            bossSpawnPos.y = hit.point.y;
        }
        
        return bossSpawnPos;
    }
    
    /// <summary>
    /// 적 사망 감지
    /// </summary>
    private IEnumerator WatchEnemyDeath(Enemy enemy)
    {
        while (enemy && !enemy.IsDead)
        {
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 적 사망 처리
    /// </summary>
    public void OnEnemyKilled(Enemy enemy)
    {
        if (enemy == null) return;
        
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
        
        if (enemy == currentBoss)
        {
            // 보스 처치 처리
            OnBossKilled(enemy);
        }
        else
        {
            // 일반 몬스터 처치 처리
            currentKillCount++;
            OnKillCountChanged?.Invoke(currentKillCount, currentStage.requiredKillCount);
            
            Debug.Log($" 적 처치! 게이지: ({currentKillCount}/{currentStage.requiredKillCount})");
            
            // 킬 카운트가 목표에 도달하면 보스 스폰
            if (currentKillCount >= currentStage.requiredKillCount && currentState == StageState.Playing)
            {
                Debug.Log("목표 처치 수 달성! 보스 스폰 준비");
                StartCoroutine(WaitAndSpawnBoss());
            }
        }
        
        if (PoolManager.Instance)
        {
            PoolManager.Instance.ReturnEnemy(enemy);
        }
    }

    private void ClearRemainingEnemies()
    {
        // 복사본 생성 후 처리
        List<Enemy> enemiesToClear = new List<Enemy>();

        foreach (var enemy in activeEnemies)
        {
            if (enemy && enemy != currentBoss)
            {
                enemiesToClear.Add(enemy);
            }
        }
        
        foreach (var enemy in enemiesToClear)
        {
            enemy.ForceKill();
        }
    }
    
    /// <summary>
    /// 모든 활성 적들 정리 (스테이지 리셋용)
    /// </summary>
    private void ClearAllActiveEnemies()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnAllActiveEnemies();
        }
        
        activeEnemies.Clear();
    }

    
    /// <summary>
    /// 보스 사망 감지
    /// </summary>
    private IEnumerator WatchBossDeath(Enemy boss)
    {
        while (boss && !boss.IsDead)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (boss)
        {
            OnBossKilled(boss);
        }
    }
    
    /// <summary>
    /// 보스 사망 처리
    /// </summary>
    /// <param name="boss"></param>
    private void OnBossKilled(Enemy boss)
    {
        currentBoss = null;
        OnBossDefeated?.Invoke(boss);
        
        CompleteStage();
        Debug.Log("보스 처치 완료");
    }
    
    /// <summary>
    /// 스테이지 완료 보상
    /// </summary>
    private void GiveStageRewards()
    {
        Player player = CharacterManager.Player;
        if (player == null) return;
        
        // TODO: 보상 아이템 지급
        // player.StatHandler.ModifyStat(StatType.Exp, currentStage.rewardExp);
        // player.StatHandler.ModifyStat(StatType.Gold, currentStage.rewardGold);
    }
    
    // Public Getters
    public StageData CurrentStage => currentStage;
    public StageState CurrentState => currentState;
    public int CurrentKillCount => currentKillCount;
    public int RequiredKillCount => currentStage?.requiredKillCount ?? 0;
    public List<Enemy> ActiveEnemies => activeEnemies;
    public Enemy CurrentBoss => currentBoss;
    public bool IsBossStage => currentState == StageState.BossSpawned;
    
    public bool IsPlaying()
    {
        return currentState == StageState.Playing || currentState == StageState.BossSpawned;
    }
    
    // 디버깅용 기즈모
    private void OnDrawGizmos()
    {
        if (CharacterManager.Player == null) return;
        
        Transform player = CharacterManager.Player.transform;
        Vector3 playerPos = player.position;
        Vector3 playerForward = player.forward;
        Vector3 rightVector = Vector3.Cross(playerForward, Vector3.up).normalized;
        
        // 스폰 영역 표시
        Gizmos.color = Color.yellow;
        Vector3 spawnCenter = playerPos + playerForward * spawnDistanceAhead;
        Vector3 leftPoint = spawnCenter - rightVector * spawnRangeWidth;
        Vector3 rightPoint = spawnCenter + rightVector * spawnRangeWidth;
        
        Gizmos.DrawLine(leftPoint, rightPoint);
        Gizmos.DrawWireSphere(spawnCenter, 1f);
        
        // 플레이어 전진 방향 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(playerPos, playerForward * spawnDistanceAhead);
    }
}