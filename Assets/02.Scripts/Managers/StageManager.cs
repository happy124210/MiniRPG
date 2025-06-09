using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum StageState
{
    Loading,
    Playing,
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
    [SerializeField] private float spawnInterval = 2f;           // 2초마다 스폰 (더 빠르게)
    [SerializeField] private int maxEnemiesAtOnce = 12;          // 동시 최대 적 수 증가
    [SerializeField] private int enemiesPerSpawn = 2;            // 한 번에 스폰할 적 수
    [SerializeField] private float spawnDistanceAhead = 15f;     // 플레이어 앞 몇 미터에서 스폰
    [SerializeField] private float spawnRangeWidth = 4f;         // 좌우 스폰 범위 (좀 더 넓게)
    [SerializeField] private LayerMask obstacleLayerMask = -1;   // 장애물 레이어 마스크
    
    [Header("Stage Progress")]
    private int currentKillCount = 0;
    private int totalSpawnedCount = 0;
    private List<Enemy> activeEnemies = new();
    
    // Events
    public static event Action<StageData> OnStageStart;
    public static event Action<StageData> OnStageComplete;
    public static event Action<StageData> OnStageFailed;
    public static event Action<int, int> OnKillCountChanged; // current, required
    
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
    
    private void Start()
    {
        // 플레이어 기반 스폰이므로 기본 스폰 포인트 생성 불필요
    }
    
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
            Debug.LogError("스테이지 데이터가 없습니다!");
            return;
        }
        
        currentState = StageState.Playing;
        
        // 스테이지 프리팹 생성 (맵)
        if (currentStage.stagePrefab != null)
        {
            // 기존 맵 제거 (있다면)
            GameObject existingMap = GameObject.FindGameObjectWithTag("StageMap");
            if (existingMap != null)
            {
                Destroy(existingMap);
            }
            
            GameObject stageMap = Instantiate(currentStage.stagePrefab);
            stageMap.tag = "StageMap";
        }
        
        // 적 스폰 시작
        spawnCoroutine = StartCoroutine(SpawnEnemies());
        
        OnStageStart?.Invoke(currentStage);
        Debug.Log($"스테이지 시작: {currentStage.stageName}");
    }
    
    /// <summary>
    /// 적 스폰 코루틴
    /// </summary>
    private IEnumerator SpawnEnemies()
    {
        while (currentState == StageState.Playing && 
               totalSpawnedCount < currentStage.requiredKillCount)
        {
            // 현재 활성 적 수가 최대치보다 적을 때만 스폰
            if (activeEnemies.Count < maxEnemiesAtOnce)
            {
                // 한 번에 여러 마리 스폰
                int spawnCount = Mathf.Min(enemiesPerSpawn, 
                                          maxEnemiesAtOnce - activeEnemies.Count,
                                          currentStage.requiredKillCount - totalSpawnedCount);
                
                for (int i = 0; i < spawnCount; i++)
                {
                    SpawnRandomEnemy();
                    totalSpawnedCount++;
                    
                    // 스폰 간 약간의 딜레이 (동시 스폰 방지)
                    yield return new WaitForSeconds(0.2f);
                }
            }
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    /// <summary>
    /// 플레이어 앞쪽에 적 스폰
    /// </summary>
    private void SpawnRandomEnemy()
    {
        Player player = CharacterManager.Player;
        if (player == null) return;
        
        Vector3 spawnPosition = GetValidSpawnPosition(player.transform);
        if (spawnPosition == Vector3.zero) return; // 유효한 위치를 찾지 못함
        
        // TODO: 나중에 EnemyData 배열에서 랜덤하게 선택하도록 개선
        // 지금은 기본 적 프리팹 사용 (임시)
        GameObject enemyPrefab = Resources.Load<GameObject>("Enemies/Slime");
        if (enemyPrefab == null)
        {
            Debug.LogError("적 프리팹을 찾을 수 없습니다!");
            return;
        }
        
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        
        if (enemy != null)
        {
            activeEnemies.Add(enemy);
            // 적 사망 이벤트 구독
            StartCoroutine(WatchEnemyDeath(enemy));
        }
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
                                 
            // 땅 높이로 조정 (레이캐스트로 바닥 찾기)
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
            0.5f,                              // 반지름
            obstacleLayerMask                  // 장애물 레이어
        );
    }
    
    /// <summary>
    /// 적 사망 감지
    /// </summary>
    private IEnumerator WatchEnemyDeath(Enemy enemy)
    {
        while (enemy != null && !enemy.IsDead)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        if (enemy != null)
        {
            OnEnemyKilled(enemy);
        }
    }
    
    /// <summary>
    /// 적 사망 처리
    /// </summary>
    public void OnEnemyKilled(Enemy enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
        
        currentKillCount++;
        OnKillCountChanged?.Invoke(currentKillCount, currentStage.requiredKillCount);
        
        // 보상 지급
        GiveRewards(enemy);
        
        // 스테이지 완료 체크
        CheckStageComplete();
        
        Debug.Log($"적 처치! ({currentKillCount}/{currentStage.requiredKillCount})");
    }
    
    /// <summary>
    /// 보상 지급
    /// </summary>
    private void GiveRewards(Enemy enemy)
    {
        Player player = CharacterManager.Player;
        if (player == null) return;
        
        // 경험치, 골드 지급 (EnemyData에서 가져와야 하는데 현재 구조상 임시로 고정값)
        player.StatHandler.ModifyStat(StatType.Exp, 10);
        player.StatHandler.ModifyStat(StatType.Gold, 5);
    }
    
    /// <summary>
    /// 스테이지 완료 체크
    /// </summary>
    private void CheckStageComplete()
    {
        if (currentKillCount >= currentStage.requiredKillCount)
        {
            CompleteStage();
        }
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
        }
        
        // 스테이지 완료 보상
        GiveStageRewards();
        
        OnStageComplete?.Invoke(currentStage);
        Debug.Log($"스테이지 완료: {currentStage.stageName}");
    }
    
    /// <summary>
    /// 스테이지 완료 보상
    /// </summary>
    private void GiveStageRewards()
    {
        Player player = CharacterManager.Player;
        if (player == null) return;
        
        player.StatHandler.ModifyStat(StatType.Exp, currentStage.rewardExp);
        player.StatHandler.ModifyStat(StatType.Gold, currentStage.rewardGold);
        
        // 보상 아이템 생성 (나중에 구현)
        // SpawnRewardItems();
    }
    
    /// <summary>
    /// 스테이지 데이터 리셋
    /// </summary>
    private void ResetStageData()
    {
        currentKillCount = 0;
        totalSpawnedCount = 0;
        activeEnemies.Clear();
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }
    
    // Public Getters
    public StageData CurrentStage => currentStage;
    public StageState CurrentState => currentState;
    public int CurrentKillCount => currentKillCount;
    public int RequiredKillCount => currentStage?.requiredKillCount ?? 0;
    public List<Enemy> ActiveEnemies => activeEnemies;
    
    // 디버깅용 기즈모 (Scene 뷰에서 스폰 범위 표시)
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