using UnityEngine;

public enum ItemType
{
    Weapon,     // 무기
    Armor,      // 방어구
}

public enum ItemRarity
{
    Common,     // 일반 (회색)
    Uncommon,   // 고급 (초록)
    Rare,       // 희귀 (파랑)
    Epic,       // 전설 (보라)
    Legendary   // 신화 (주황)
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public string description;
    public ItemType itemType;
    public ItemRarity itemRarity;
    public Sprite icon;
    
    [Header("Stats")]
    public StatModifier[] statModifiers;
    
    [Header("Values")]
    public int sellPrice;
    public int buyPrice;
}