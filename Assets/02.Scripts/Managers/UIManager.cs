using System.Collections.Generic;
using UnityEngine;

// 스크린 타입 (전체 화면을 차지하는 UI)
public enum ScreenType
{
    Start,
    Main,
    StageSelect,
    Option
}

// 팝업 타입 (기존 화면 위에 올라가는 UI)
public enum PopupType
{
    Inventory,
    Shop,
    Settings,
    ItemDetail,
    Pause
}

// 모달 타입 (최상위 확인창)
public enum ModalType
{
    Confirm,
    Loading,
    Error
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("Screen UIs")]
    [SerializeField] private List<MonoBehaviour> screenUIList;
    private Dictionary<ScreenType, IGUI> screenUIs = new();
    
    // 현재 상태
    private ScreenType currentScreen = ScreenType.Main;
    private Stack<PopupType> activePopups = new();
    private Stack<ModalType> activeModals = new();
    
    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// GameManager에서 호출하는 초기화 메서드
    /// </summary>
    public void Initialize()
    {
        InitializeUIs();
        Debug.Log("UIManager 초기화 완료");
    } 
    
    
    private void InitializeUIs()
    {
        // Screen UIs만 초기화 (팝업들은 각 Screen에서 관리)
        if (screenUIList != null && screenUIList.Count > 0)
        {
            foreach (var uiMono in screenUIList)
            {
                if (uiMono == null) 
                {
                    Debug.LogWarning("Screen UI List에 null 항목이 있습니다!");
                    continue;
                }
                
                if (uiMono is IGUI gui)
                {
                    ScreenType screenType = GetScreenTypeFromName(uiMono.name);
                    screenUIs[screenType] = gui;
                    
                    try
                    {
                        gui.Initialization();
                        Debug.Log($"Screen UI 초기화 완료: {uiMono.name}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Screen UI 초기화 실패: {uiMono.name}, Error: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"{uiMono.name}이 IGUI를 구현하지 않습니다!");
                }
            }
        }
        else
        {
            Debug.Log("Screen UI List가 비어있습니다.");
        }
        
        Debug.Log($"UIManager 초기화 완료 - Screen: {screenUIs.Count}");
    }
    
    #region Screen Management
    
    /// <summary>
    /// 스크린 전환 (전체 화면)
    /// </summary>
    public void ShowScreen(ScreenType screenType)
    {
        // 모든 팝업과 모달 닫기
        CloseAllPopups();
        CloseAllModals();
        
        // 기존 스크린 닫기
        if (screenUIs.ContainsKey(currentScreen))
        {
            screenUIs[currentScreen].Close();
        }
        
        // 새 스크린 열기
        if (screenUIs.ContainsKey(screenType))
        {
            screenUIs[screenType].Open();
            currentScreen = screenType;
            Debug.Log($"스크린 전환: {screenType}");
        }
        else
        {
            Debug.LogError($"Screen UI not found: {screenType}");
        }
    }
    
    public ScreenType GetCurrentScreen() => currentScreen;
    
    #endregion
    
    #region Popup Management
    
    /// <summary>
    /// 팝업 열기 (Main 화면에서만 가능)
    /// </summary>
    public void ShowPopup(PopupType popupType)
    {
        // Main 화면이 아니면 팝업 불가
        if (currentScreen != ScreenType.Main)
        {
            Debug.LogWarning($"팝업은 Main 화면에서만 열 수 있습니다. 현재: {currentScreen}");
            return;
        }
        
        // Main UI에 팝업 관리 위임
        if (screenUIs.ContainsKey(ScreenType.Main) && screenUIs[ScreenType.Main] is UICanvasMain mainUI)
        {
            switch (popupType)
            {
                case PopupType.Inventory:
                    mainUI.ShowInventory();
                    activePopups.Push(popupType);
                    break;
                case PopupType.Shop:
                    mainUI.ShowShop();
                    activePopups.Push(popupType);
                    break;
                default:
                    Debug.LogWarning($"지원하지 않는 팝업 타입: {popupType}");
                    break;
            }
        }
        else
        {
            Debug.LogError("Main UI를 찾을 수 없습니다!");
        }
    }
    
    /// <summary>
    /// 특정 팝업 닫기
    /// </summary>
    public void ClosePopup(PopupType popupType)
    {
        if (screenUIs.ContainsKey(ScreenType.Main) && screenUIs[ScreenType.Main] is UICanvasMain mainUI)
        {
            switch (popupType)
            {
                case PopupType.Inventory:
                    mainUI.CloseInventory();
                    break;
                case PopupType.Shop:
                    mainUI.CloseShop();
                    break;
            }
            
            // 스택에서 제거
            RemoveFromPopupStack(popupType);
        }
    }
    
    /// <summary>
    /// 모달 열기
    /// </summary>
    public void ShowModal(ModalType modalType)
    {
        if (screenUIs.ContainsKey(ScreenType.Main) && screenUIs[ScreenType.Main] is UICanvasMain mainUI)
        {
            switch (modalType)
            {
                case ModalType.Confirm:
                    mainUI.ShowConfirm();
                    activeModals.Push(modalType);
                    break;
                default:
                    Debug.LogWarning($"지원하지 않는 모달 타입: {modalType}");
                    break;
            }
        }
    }
    
    /// <summary>
    /// 모달 닫기
    /// </summary>
    public void CloseModal(ModalType modalType)
    {
        if (screenUIs.ContainsKey(ScreenType.Main) && screenUIs[ScreenType.Main] is UICanvasMain mainUI)
        {
            switch (modalType)
            {
                case ModalType.Confirm:
                    mainUI.CloseConfirm();
                    break;
            }
            
            // 스택에서 제거
            RemoveFromModalStack(modalType);
        }
    }
    
    /// <summary>
    /// 최상위 팝업 닫기 (뒤로가기 버튼용)
    /// </summary>
    public void CloseTopPopup()
    {
        if (activePopups.Count > 0)
        {
            PopupType topPopup = activePopups.Pop();
            ClosePopup(topPopup);
            Debug.Log($"최상위 팝업 닫기: {topPopup}");
        }
    }
    
    /// <summary>
    /// 모든 팝업 닫기
    /// </summary>
    public void CloseAllPopups()
    {
        if (screenUIs.ContainsKey(ScreenType.Main) && screenUIs[ScreenType.Main] is UICanvasMain mainUI)
        {
            mainUI.CloseAllPopups();
            activePopups.Clear();
            Debug.Log("모든 팝업 닫기");
        }
    }
    
    /// <summary>
    /// 최상위 모달 닫기
    /// </summary>
    public void CloseTopModal()
    {
        if (activeModals.Count > 0)
        {
            ModalType topModal = activeModals.Pop();
            CloseModal(topModal);
            Debug.Log($"모달 닫기: {topModal}");
        }
    }
    
    /// <summary>
    /// 모든 모달 닫기
    /// </summary>
    public void CloseAllModals()
    {
        if (screenUIs.ContainsKey(ScreenType.Main) && screenUIs[ScreenType.Main] is UICanvasMain mainUI)
        {
            mainUI.CloseAllPopups(); // 모달도 포함
            activeModals.Clear();
            Debug.Log("모든 모달 닫기");
        }
    }
    
    /// <summary>
    /// 팝업 스택에서 특정 팝업 제거
    /// </summary>
    private void RemoveFromPopupStack(PopupType popupType)
    {
        Stack<PopupType> tempStack = new();
        while (activePopups.Count > 0)
        {
            PopupType popup = activePopups.Pop();
            if (popup != popupType)
            {
                tempStack.Push(popup);
            }
        }
        while (tempStack.Count > 0)
        {
            activePopups.Push(tempStack.Pop());
        }
    }
    
    /// <summary>
    /// 모달 스택에서 특정 모달 제거
    /// </summary>
    private void RemoveFromModalStack(ModalType modalType)
    {
        Stack<ModalType> tempStack = new();
        while (activeModals.Count > 0)
        {
            ModalType modal = activeModals.Pop();
            if (modal != modalType)
            {
                tempStack.Push(modal);
            }
        }
        while (tempStack.Count > 0)
        {
            activeModals.Push(tempStack.Pop());
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    private ScreenType GetScreenTypeFromName(string name)
    {
        if (name.Contains("Start")) return ScreenType.Start;
        if (name.Contains("Main")) return ScreenType.Main;
        if (name.Contains("StageSelect")) return ScreenType.StageSelect;
        if (name.Contains("Option")) return ScreenType.Option;
        
        Debug.LogWarning($"Unknown screen name: {name}");
        return ScreenType.Main;
    }
    
    private PopupType GetPopupTypeFromName(string name)
    {
        if (name.Contains("Inventory")) return PopupType.Inventory;
        if (name.Contains("Shop")) return PopupType.Shop;
        if (name.Contains("Settings")) return PopupType.Settings;
        if (name.Contains("ItemDetail")) return PopupType.ItemDetail;
        if (name.Contains("Pause")) return PopupType.Pause;
        
        Debug.LogWarning($"Unknown popup name: {name}");
        return PopupType.Inventory;
    }
    
    private ModalType GetModalTypeFromName(string name)
    {
        if (name.Contains("Confirm")) return ModalType.Confirm;
        if (name.Contains("Loading")) return ModalType.Loading;
        if (name.Contains("Error")) return ModalType.Error;
        
        Debug.LogWarning($"Unknown modal name: {name}");
        return ModalType.Confirm;
    }
    
    #endregion
    
    #region Unity Input Handling
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackButton();
        }
    }
    
    private void HandleBackButton()
    {
        // 우선순위: Modal > Popup > Screen
        if (activeModals.Count > 0)
        {
            CloseTopModal();
        }
        else if (activePopups.Count > 0)
        {
            CloseTopPopup();
        }
        else if (currentScreen != ScreenType.Main)
        {
            ShowScreen(ScreenType.Main);
        }
    }
    
    #endregion
}