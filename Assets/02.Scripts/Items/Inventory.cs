using System;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 30;
    
    private InventorySlot[] slots;
    
    // 초기 아이템
    [SerializeField] private ItemData[] testItems;
    
    
    // Events
    public static event Action<int, InventorySlot> OnSlotChanged; // 슬롯 변경 시
    public static event Action<ItemData> OnItemAdded;
    public static event Action<ItemData> OnItemRemoved;
    public static event Action OnInventoryChanged;

    private void Awake()
    {
        InitializeInventory();
    }
    
    private void InitializeInventory()
    {
        slots = new InventorySlot[inventorySize];
        for (int i = 0; i < inventorySize; i++)
        {
            slots[i] = new InventorySlot();
        }

        foreach (var testItem in testItems)
        {
            AddItem(testItem);
        }
    }

    /// <summary>
    /// 아이템 추가
    /// </summary>
    /// <param name="item"> 추가하려는 ItemData </param>
    /// <returns> 성공하면 true 반환 </returns>
    public bool AddItem(ItemData item)
    {
        if (item == null) return false;
        
        // 빈 슬롯 찾아서 추가
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].AddItem(item);
                OnSlotChanged?.Invoke(i, slots[i]);
                OnItemAdded?.Invoke(item);
                return true;
            }
        }
        
        Debug.LogError($"Inventory is full");
        return false;
    }

    /// <summary>
    /// 아이템 제거
    /// </summary>
    /// <param name="item"> 제거하려는 ItemData </param>
    /// <returns> 성공하면 true 반환 </returns>
    public bool RemoveItem(ItemData item)
    {
        if (item == null) return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].itemData == item)
            {
                slots[i].RemoveItem();
                OnSlotChanged?.Invoke(i, slots[i]);
                OnItemRemoved?.Invoke(item);
                return true;
            }
        }
        
        return false;
    }


    public bool UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length || slots[slotIndex].IsEmpty) return false;
        
        ItemData item = slots[slotIndex].itemData;

        if (item.itemType == ItemType.Weapon || item.itemType == ItemType.Armor)
        {
            // TODO: 장비 시스템 연동
            ToggleItemEquip(item, slotIndex);
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// 장비 착용 처리
    /// </summary>
    /// <param name="item"></param>
    /// <param name="slotIndex"></param>
    private void ToggleItemEquip(ItemData item, int slotIndex)
    {
        Player player = CharacterManager.Player;
        if (player == null) return;
        
        // 이미 장착중이라면 해제
        if (slots[slotIndex].isEquipped)
        {
            UnequipItem(item, slotIndex);
            return;
        }

        UnequipSameTypeItems(item.itemType);
        slots[slotIndex].SetEquipped(true);
        
        // 스탯 효과 적용
        foreach (var modifier in item.statModifiers)
        {
            player.StatHandler.AddModifier(modifier);
        }
        
        OnSlotChanged?.Invoke(slotIndex, slots[slotIndex]);
        Debug.Log($"{item.itemName} 장착!");
    }

    private void UnequipItem(ItemData item, int slotIndex)
    {
        Player player = CharacterManager.Player;
        if (player == null) return;

        slots[slotIndex].SetEquipped(false);
        
        // 스탯 효과 제거
        foreach (var modifier in item.statModifiers)
        {
            player.StatHandler.RemoveModifier(modifier);
        }
        
        OnSlotChanged?.Invoke(slotIndex, slots[slotIndex]);
        Debug.Log($"{item.itemName} 해제!");
    }

    /// <summary>
    /// 같은 타입의 장착 중인 아이템 해제
    /// </summary>
    /// <param name="itemType"></param>
    private void UnequipSameTypeItems(ItemType itemType)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].isEquipped && slots[i].itemData != null && slots[i].itemData.itemType == itemType)
            {
                UnequipItem(slots[i].itemData, i);
            }
        }
    }

    /// <summary>
    /// 장착 중인 특정 타입의 아이템 가져오기
    /// </summary>
    /// <param name="itemType">확인하려는 아이템 타입</param>
    /// <returns>장착중인 아이템 데이터, 없으면 null</returns>
    public ItemData GetEquippedItem(ItemType itemType)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].isEquipped && slots[i].itemData != null && slots[i].itemData.itemType == itemType)
            {
                return slots[i].itemData;
            }
        }
        
        return null;
    }
    
    
    public int GetEmptySlotCount()
    {
        int count = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty) count++;
        }
        return count;
    }
    
    // Public Getters
    public InventorySlot[] GetAllSlots() => slots;
    public InventorySlot GetSlot(int index) => (index >= 0 && index < slots.Length) ? slots[index] : null;
    public int InventorySize => inventorySize;
}
