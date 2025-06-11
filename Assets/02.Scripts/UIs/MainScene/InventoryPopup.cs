using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPopup : MonoBehaviour, IGUI
{
    [Header("Equipment Slots")]
    [SerializeField] private Button weaponSlotButton;
    [SerializeField] private Button armorSlotButton;
    [SerializeField] private Image weaponSlotImage;
    [SerializeField] private Image armorSlotImage;
    [SerializeField] private Image weaponSlotFlag;
    [SerializeField] private Image armorSlotFlag;
    
    [Header("Player Info")]
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image playerProfileImage;
    
    [Header("PlayerStats")]
    [SerializeField] private TextMeshProUGUI attackPowerText;
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Header("Sub Popup")]
    [SerializeField] private EquipPopup equipPopup;
    
    public GameObject GUIObject => gameObject;
    
    private Inventory inventory;
    
    public void Initialization()
    {
        inventory = CharacterManager.Player.Inventory;
        
        weaponSlotButton.onClick.AddListener(() => OnEquipmentSlotClicked(ItemType.Weapon));
        armorSlotButton.onClick.AddListener(() => OnEquipmentSlotClicked(ItemType.Armor));

        equipPopup.Initialization();
        
        Close();
    }

    
    public void Open()
    {
        gameObject.SetActive(true);
        RefreshUI();
    }

    
    public void Close()
    {
        gameObject.SetActive(false);
        equipPopup.Close();
    }

    /// <summary>
    /// 장비 슬롯 클릭 시 해당 타입 아이템 선택 팝업 열기
    /// </summary>
    /// <param name="itemType">해당하는 장비 타입</param>
    private void OnEquipmentSlotClicked(ItemType itemType)
    {
        if (equipPopup != null)
        {
            equipPopup.ShowItemsOfType(itemType);
        }
    }


    public void RefreshUI()
    {
        RefreshPlayerInfo();
        RefreshEquippedItems();
        RefreshPlayerStats();
    }


    private void RefreshPlayerInfo()
    {
        Player player = CharacterManager.Player;
        if (player == null) return;

        int level = player.StatHandler.GetStat(StatType.Level);
        playerLevelText.text = $"LEVEL {level}";
        
        // TODO: 임시 플레이어 이름
        playerNameText.text = "PlayerA";
    }


    private void RefreshEquippedItems()
    {
        if (inventory == null) return;
        
        // 장착 무기 표시
        ItemData equippedWeapon = inventory.GetEquippedItem(ItemType.Weapon);
        UpdateEquipmentSlot(weaponSlotImage, weaponSlotFlag, equippedWeapon);
        
        ItemData equippedArmor = inventory.GetEquippedItem(ItemType.Armor);
        UpdateEquipmentSlot(armorSlotImage, armorSlotFlag, equippedArmor);
    }


    private void UpdateEquipmentSlot(Image slotImage, Image rarityFlag, ItemData item)
    {
        // 아이템 있으면
        if (item != null)
        {
            slotImage.sprite = item.icon;
            slotImage.color = Color.white;
            rarityFlag.color = GetRarityColor(item.itemRarity);
        }
        // 빈 슬롯
        else
        {
            slotImage.sprite = null;
            slotImage.color = Color.clear;
            rarityFlag.color = Color.clear;
        }
    }


    private void RefreshPlayerStats()
    {
        Player player = CharacterManager.Player;
        if (player == null)
        {
            Debug.LogError("Player is null");
            return;
        }

        int attackPower = player.StatHandler.GetStat(StatType.AttackPower);
        int health = player.StatHandler.GetStat(StatType.Hp);
        
        attackPowerText.text = attackPower.ToString();
        healthText.text = health.ToString();
        
        Debug.Log("Player stats updated");
    }


    private Color GetRarityColor(ItemRarity itemRarity)
    {
        return itemRarity switch
        {
            ItemRarity.Common => Color.gray,
            ItemRarity.Uncommon => Color.green,
            ItemRarity.Rare => Color.blue,
            ItemRarity.Epic => Color.magenta,
            ItemRarity.Legendary => Color.yellow,
            _ => Color.white,
        };
    }


    public void OnCloseButtonClicked()
    {
        Close();
    }
}
