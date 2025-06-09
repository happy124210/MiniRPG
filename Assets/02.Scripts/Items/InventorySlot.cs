using UnityEngine;

public class InventorySlot
{
    public ItemData itemData = null;
    public bool isEquipped;
    
    public bool IsEmpty => itemData == null;

    public bool CanAddItem(ItemData item)
    {
        return IsEmpty;
    }


    public void AddItem(ItemData item)
    {
        if (IsEmpty)
        {
            itemData = item;
        }
    }


    public void RemoveItem()
    {
        itemData = null;
    }


    public void SetEquipped(bool equipped)
    {
        isEquipped = equipped;
    }


    public void Clear()
    {
        itemData = null;
    }
}