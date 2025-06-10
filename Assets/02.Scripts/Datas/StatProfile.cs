using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    Hp,
    Mp,
    AttackPower,
    SkillPower,
    Level,
    Gold,
    Gem,
    Key,
    MaxKey,
    Exp,
    PowerLevel
}

[System.Serializable]
public class StatData
{
    public StatType type;
    public int baseValue;
}

[CreateAssetMenu(fileName = "NewStatProfile", menuName = "Stats/Stat Profile")]
public class StatProfile : ScriptableObject
{
    public List<StatData> stats;

    public int GetBaseValue(StatType type)
    {
        StatData stat = stats.Find(s => s.type == type);
        return stat?.baseValue ?? 0;
    }
}