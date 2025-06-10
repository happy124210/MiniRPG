using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipSlot : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button slotButton;
    [SerializeField] private Image itemIcon;
    [SerializeField] private GameObject equippedIndicator; // 장착 아이콘
    [SerializeField] private GameObject selectedIndicator; // 선택 아이콘
    
    private InventorySlot slotData;
    private int slotIndex;
    private System.Action<int> onClickCallback;
    
    public void Initialize(InventorySlot slot, int index, System.Action<int> clickCallback)
    {
        slotData = slot;
        slotIndex = index;
        onClickCallback = clickCallback;
        
        // 버튼 이벤트 연결
        slotButton.onClick.AddListener(() => onClickCallback?.Invoke(slotIndex));
        
        // UI 업데이트
        UpdateUI();
    }
    
    /// <summary>
    /// UI 업데이트 (선택된 슬롯 인덱스 전달)
    /// </summary>
    public void RefreshUI(int selectedSlotIndex = -1)
    {
        UpdateUI();
        UpdateSelectionState(selectedSlotIndex);
    }
    
    /// <summary>
    /// 기본 UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (slotData == null || slotData.IsEmpty) return;
        
        ItemData item = slotData.itemData;
        
        // 아이템 아이콘
        itemIcon.sprite = item.icon;
        itemIcon.color = Color.white;
        
        // 장착 표시 아이콘 (체크 마크)
        if (equippedIndicator != null)
        {
            equippedIndicator.SetActive(slotData.isEquipped);
        }
    }
    
    /// <summary>
    /// 선택 상태 업데이트
    /// </summary>
    private void UpdateSelectionState(int selectedSlotIndex)
    {
        bool isSelected = (slotIndex == selectedSlotIndex);
        
        // 선택 표시 아이콘
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(isSelected);
        }
    }
}