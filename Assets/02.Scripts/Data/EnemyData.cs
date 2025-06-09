using UnityEngine;

public enum EnemyType
{
    Normal,
    Boss
}

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy / Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Data")]
    public EnemyType type;
    public int maxHp;
    
    [Header("Movement")]
    public float moveSpeed;
    
    [Header("Combat")]
    public int attackPower;
    public int attackInterval;
    public int attackRange;
    
    [Header("Reward")]
    public int goldReward;
    public int expReward;
    
    [Header("Visual")]
    public string enemyName;
    public GameObject enemyPrefab;
    public AnimatorOverrideController overrideController;
}