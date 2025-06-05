using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatHandler : MonoBehaviour
{
    [SerializeField] private StatProfile profile;
    
    private Dictionary<StatType, int> baseStats = new();
    private List<StatModifier> modifiers = new();

    public event Action OnStatChanged;

    private void Awake()
    {
        foreach (var stat in profile.stats)
        {
            baseStats[stat.type] = stat.baseValue;
        }
    }

    public int GetStat(StatType type)
    {
        int baseVal = baseStats.GetValueOrDefault(type, 0);
        int modSum = modifiers
            .Where(m => m.type == type)
            .Sum(m => m.value);

        return baseVal + modSum;
    }


    public void AddModifier(StatModifier modifier)
    {
        modifiers.Add(modifier);
        OnStatChanged?.Invoke();
    }

    public void RemoveModifier(StatModifier modifier)
    {
        modifiers.Remove(modifier);
        OnStatChanged?.Invoke();
    }

    public void ModifyStat(StatType type, int value)
    {
        if (baseStats.ContainsKey(type))
        {
            baseStats[type] += value;
        }
        else
        {
            baseStats[type] = value;
        }
        OnStatChanged?.Invoke();
    }

    public void SetBaseStat(StatType type, int value)
    {
        baseStats[type] = value;
        OnStatChanged?.Invoke();
    }

    public bool IsDead()
    {
        return GetStat(StatType.Hp) <= 0;
    }
}