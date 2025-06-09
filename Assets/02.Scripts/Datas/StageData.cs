using UnityEngine;

[CreateAssetMenu(fileName = "NewStageData", menuName = "Stage/StageData")]
public class StageData : ScriptableObject
{
    [Header("맵 정보")]
    public string stageName;
    public int stageID;
    public GameObject stagePrefab;
    
    [Header("스폰 몬스터 정보")]
    public EnemyData[] enemyPool; // 스폰할 적 데이터들
    public int[] enemySpawnCounts; // 각 적별 스폰 수
    public float enemySpawnInterval; // 스폰 간격
    public int requiredKillCount; // 보스 소환까지 필요한 처치 수
    
    [Header("보상")]
    public GameObject[] rewardPrefabs; 
    public int rewardExp;
    public int rewardGold;
}
