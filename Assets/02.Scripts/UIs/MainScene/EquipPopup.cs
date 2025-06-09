using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipPopup : MonoBehaviour, IGUI
{
    [Header("Selected Item Detail")]
    [SerializeField] private Image selectedItemImage;
    [SerializeField] private TextMeshProUGUI selectedItemName;
    [SerializeField] private TextMeshProUGUI selectedItemRarity;
    [SerializeField] private TextMeshProUGUI selectedItemStats;
    
    [Header("Item Grid")]
    [SerializeField] private Transform itemGridParent;
    [SerializeField] private GameObject itemSlotPrefab;
    
    [Header("Buttons")]
    [SerializeField] private Button equipButton;
    [SerializeField] private Button closeButton;
    
    [Header("Filter")]
    [SerializeField] private ItemType currentFilterType;
    [SerializeField] private int selectedSlotIndex = -1;
    
    public GameObject GUIObject => gameObject;
    
    private Inventory inventory;
    private InventoryPopup inventoryPopup;

    public void Initialization()
    {
        inventory = CharacterManager.Player.Inventory;
        inventoryPopup = GetComponentInParent<InventoryPopup>();
        
        // Button Event
        closeButton.onClick.AddListener(Close);
        equipButton.onClick.AddListener(OnEquipButtonClicked);
        
        Close();
    }

    
    public void Open()
    {
        gameObject.SetActive(true);
    }

    
    public void Close()
    {
        gameObject.SetActive(false);
        selectedSlotIndex = -1;
        ClearSelectedItemDetail();
    }


    public void ShowItemsOfType(ItemType type)
    {
        currentFilterType = type;
        
        // 기존 선택 해제
        selectedSlotIndex = -1;
        ClearSelectedItemDetail();
        ClearItemSlots();
        CreateItemSlots();
        Open();
    }


    public void ClearItemSlots()
    {
        foreach (Transform child in itemGridParent)
        {
            Destroy(child.gameObject);
        }
    }


    public void CreateItemSlots()
    {
        if (inventory == null || itemSlotPrefab == null) return;

        InventorySlot[] allSlots = inventory.GetAllSlots();

        for (int i = 0; i < allSlots.Length; i++)
        {
            InventorySlot slot = allSlots[i];

            if (slot.IsEmpty || slot.itemData.itemType != currentFilterType) continue;
            
            GameObject slotObj = Instantiate(itemSlotPrefab, itemGridParent);
            EquipSlot slotUI =  slotObj.GetComponent<EquipSlot>();
        
            if (slotUI != null)
                slotUI.Initialize(slot, i, OnItemSlotClicked);
        }
    }


    private void OnItemSlotClicked(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        ShowSelectedItemDetail(slotIndex);
        RefreshItemSlots();
    }
    
    
    private void ShowSelectedItemDetail(int slotIndex)
    {
        if (inventory == null) return;
        
        InventorySlot slot = inventory.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty) return;
        
        ItemData item = slot.itemData;
        
        gameObject.SetActive(true);

        // 아이템 정보 표시
        selectedItemImage.sprite = item.icon;
        selectedItemName.text = item.itemName;
        selectedItemRarity.text = GetRarityText(item.itemRarity);
        selectedItemRarity.color = GetRarityColor(item.itemRarity);

        // 스탯 정보 표시
        string statsText = "";
        foreach (var modifier in item.statModifiers)
        {
            statsText += $"{modifier.value} {GetStatDisplayName(modifier.type)}\n";
        }
        selectedItemStats.text = statsText.TrimEnd();
        
        // 장착/해제 버튼 텍스트 설정
        if (equipButton != null)
        {
            bool isEquipped = slot.isEquipped;
            equipButton.GetComponentInChildren<TextMeshProUGUI>().text = isEquipped ? "해제" : "파워업!";
        }
    }
    
    /// <summary>
    /// 선택된 아이템 상세정보 숨기기
    /// </summary>
    private void ClearSelectedItemDetail()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 파워업/해제 버튼 클릭
    /// </summary>
    private void OnEquipButtonClicked()
    {
        if (selectedSlotIndex == -1 || inventory == null) return;
        
        // 아이템 장착/해제
        inventory.UseItem(selectedSlotIndex);
        
        // UI 새로고침
        ShowSelectedItemDetail(selectedSlotIndex); // 버튼 텍스트 업데이트
        RefreshItemSlots(); // 장착 상태 표시 업데이트
        
        // 부모 팝업 UI 새로고침
        inventoryPopup?.RefreshUI();
    }
    
    /// <summary>
    /// 아이템 슬롯들 새로고침 (선택/장착 상태 업데이트)
    /// </summary>
    private void RefreshItemSlots()
    {
        EquipSlot[] slotUIs = itemGridParent.GetComponentsInChildren<EquipSlot>();
        foreach (var slotUI in slotUIs)
        {
            slotUI.RefreshUI(selectedSlotIndex);
        }
    }
    
    /// <summary>
    /// 희귀도 텍스트 반환
    /// </summary>
    private string GetRarityText(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "Common",
            ItemRarity.Uncommon => "Uncommon", 
            ItemRarity.Rare => "Rare",
            ItemRarity.Epic => "Epic",
            ItemRarity.Legendary => "Legendary",
            _ => "Unknown"
        };
    }
    
    /// <summary>
    /// 스탯 표시 이름 반환
    /// </summary>
    private string GetStatDisplayName(StatType statType)
    {
        return statType switch
        {
            StatType.AttackPower => "무기 피해량",
            StatType.Hp => "체력",
            StatType.Mp => "마나",
            StatType.SkillPower => "스킬 위력",
            _ => statType.ToString()
        };
    }
    
    /// <summary>
    /// 희귀도 색상 반환
    /// </summary>
    private Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => Color.gray,
            ItemRarity.Uncommon => Color.green,
            ItemRarity.Rare => Color.blue,
            ItemRarity.Epic => Color.magenta,
            ItemRarity.Legendary => Color.yellow,
            _ => Color.white
        };
    }
    
    /// <summary>
    /// 닫기 버튼 클릭
    /// </summary>
    public void OnCloseButtonClicked()
    {
        Close();
    }
}