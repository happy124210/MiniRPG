using System;
using UnityEngine;

public enum ItemType
{
    Weapon,     // 무기
    Armor,      // 방어구
}

public enum ItemRarity
{
    Common,     // 일반 (gray)
    Uncommon,   // 고급 (green)
    Rare,       // 희귀 (blue)
    Epic,       // 전설 (magenta)
    Legendary   // 신화 (yellow)
}

[Serializable] [CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public string description;
    public ItemType itemType;
    public ItemRarity itemRarity;
    public Sprite icon;
    public int powerLevel;
    
    [Header("Stats")]
    public StatModifier[] statModifiers;
    
    [Header("Values")]
    public int sellPrice;
    public int buyPrice;
}