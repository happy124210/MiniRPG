using UnityEngine;

[CreateAssetMenu(fileName = "NewStageData", menuName = "Stage/StageData")]
public class StageData : ScriptableObject
{
    [Header("맵 정보")]
    public string stageName;
    public int stageID;
    public GameObject stagePrefab;
    
    [Header("스폰 몬스터 정보")]
    public int requiredKillCount;
    
    [Header("보상")]
    public GameObject[] rewardPrefabs;
    public int rewardExp;
    public int rewardGold;
}
