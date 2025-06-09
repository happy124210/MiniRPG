using UnityEngine;

public class UICanvasMain : MonoBehaviour, IGUI
{
    public GameObject GUIObject => gameObject;
    
    [Header("Main UI Panels")]
    [SerializeField] private BattleStatPanel battleStatPanel;
    [SerializeField] private BattleStagePanel battleStagePanel;
    
    [Header("Popups")]
    [SerializeField] private InventoryPopup inventoryPopup;
    [SerializeField] private ShopPopup shopPopup;
    
    [Header("Modals")]
    [SerializeField] private ConfirmModal confirmModal;

    public void Reset()
    {
        battleStatPanel = transform.FindChild<BattleStatPanel>("Group_Stat");
        battleStagePanel = transform.FindChild<BattleStagePanel>("Group_Stage");
        
        // 팝업들도 자동으로 찾기
        inventoryPopup = transform.FindChild<InventoryPopup>("InventoryPopup");
        shopPopup = transform.FindChild<ShopPopup>("ShopPopup");
        confirmModal = transform.FindChild<ConfirmModal>("ConfirmModal");
    }

    public void Initialization()
    {
        // 기존 패널들 초기화
        battleStatPanel?.Initialization();
        battleStagePanel?.Initialization();
        
        // 팝업들 초기화
        inventoryPopup?.Initialization();
        shopPopup?.Initialization();
        confirmModal?.Initialization();
        
        // 팝업들 시작 시 비활성화
        CloseAllPopups();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        
        battleStatPanel?.Open();
        battleStagePanel?.Open();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
    
    #region Popup Management
    
    /// <summary>
    /// 인벤토리 팝업 열기
    /// </summary>
    public void ShowInventory()
    {
        CloseAllPopups(); // 다른 팝업들 닫기
        inventoryPopup.Open();
        Debug.Log("인벤토리 팝업 열기");
    }
    
    /// <summary>
    /// 상점 팝업 열기
    /// </summary>
    public void ShowShop()
    {
        CloseAllPopups(); // 다른 팝업들 닫기
        shopPopup?.Open();
        Debug.Log("상점 팝업 열기");
    }
    
    /// <summary>
    /// 확인 모달 열기
    /// </summary>
    public void ShowConfirm()
    {
        confirmModal?.Open();
        Debug.Log("확인 모달 열기");
    }
    
    /// <summary>
    /// 인벤토리 팝업 닫기
    /// </summary>
    public void CloseInventory()
    {
        inventoryPopup?.Close();
    }
    
    /// <summary>
    /// 상점 팝업 닫기
    /// </summary>
    public void CloseShop()
    {
        shopPopup?.Close();
    }
    
    /// <summary>
    /// 확인 모달 닫기
    /// </summary>
    public void CloseConfirm()
    {
        confirmModal?.Close();
    }
    
    /// <summary>
    /// 모든 팝업 닫기
    /// </summary>
    public void CloseAllPopups()
    {
        inventoryPopup?.Close();
        shopPopup?.Close();
        confirmModal?.Close();
    }
    
    /// <summary>
    /// 특정 팝업이 열려있는지 확인
    /// </summary>
    public bool IsPopupOpen(PopupType popupType)
    {
        return popupType switch
        {
            PopupType.Inventory => inventoryPopup != null && inventoryPopup.GUIObject.activeInHierarchy,
            PopupType.Shop => shopPopup != null && shopPopup.GUIObject.activeInHierarchy,
            _ => false
        };
    }
    
    /// <summary>
    /// 특정 모달이 열려있는지 확인
    /// </summary>
    public bool IsModalOpen(ModalType modalType)
    {
        return modalType switch
        {
            ModalType.Confirm => confirmModal != null && confirmModal.GUIObject.activeInHierarchy,
            _ => false
        };
    }
    
    #endregion
    
    #region Button Event Handlers (UI에서 직접 호출)
    
    /// <summary>
    /// 인벤토리 버튼 클릭 이벤트
    /// </summary>
    public void OnInventoryButtonClicked()
    {
        if (IsPopupOpen(PopupType.Inventory))
        {
            CloseInventory();
        }
        else
        {
            ShowInventory();
        }
    }
    
    /// <summary>
    /// 상점 버튼 클릭 이벤트
    /// </summary>
    public void OnShopButtonClicked()
    {
        if (IsPopupOpen(PopupType.Shop))
        {
            CloseShop();
        }
        else
        {
            ShowShop();
        }
    }
    
    /// <summary>
    /// ESC 키나 뒤로가기 처리
    /// </summary>
    public void OnBackButtonPressed()
    {
        // 모달부터 확인
        if (IsModalOpen(ModalType.Confirm))
        {
            CloseConfirm();
            return;
        }
        
        // 팝업 확인
        if (IsPopupOpen(PopupType.Inventory))
        {
            CloseInventory();
            return;
        }
        
        if (IsPopupOpen(PopupType.Shop))
        {
            CloseShop();
            return;
        }
    }
    
    #endregion
}