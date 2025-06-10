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
    public int requiredKillCount; // 보스 소환까지 필요한 처치 수
    
    [Header("보상")]
    public ItemData[] rewardItems; 
    public int rewardExp;
    public int rewardGold;
}
