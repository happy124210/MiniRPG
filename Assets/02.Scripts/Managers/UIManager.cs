using System;
using System.Collections.Generic;
using UnityEngine;

// UI 계층 타입
public enum UILayerType
{
    Screen,    // 전체 화면 UI
    Popup,     // 팝업 UI  
    Modal      // 모달 UI (확인창 등)
}

// 스크린 타입 (전체 화면을 차지하는 UI)
public enum ScreenType
{
    Start,
    Main,
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
    
    [Header("Popup UIs")]
    [SerializeField] private List<MonoBehaviour> popupUIList;
    private Dictionary<PopupType, IGUI> popupUIs = new();
    
    [Header("Modal UIs")]
    [SerializeField] private List<MonoBehaviour> modalUIList;
    private Dictionary<ModalType, IGUI> modalUIs = new();
    
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
        
        InitializeUIs();
    }
    
    private void InitializeUIs()
    {
        // Screen UIs 초기화
        foreach (var uiMono in screenUIList)
        {
            if (uiMono is IGUI gui)
            {
                ScreenType screenType = GetScreenTypeFromName(uiMono.name);
                screenUIs[screenType] = gui;
                gui.Initialization();
            }
        }
        
        // Popup UIs 초기화  
        foreach (var uiMono in popupUIList)
        {
            if (uiMono is IGUI gui)
            {
                PopupType popupType = GetPopupTypeFromName(uiMono.name);
                popupUIs[popupType] = gui;
                gui.Initialization();
            }
        }
        
        // Modal UIs 초기화
        foreach (var uiMono in modalUIList)
        {
            if (uiMono is IGUI gui)
            {
                ModalType modalType = GetModalTypeFromName(uiMono.name);
                modalUIs[modalType] = gui;
                gui.Initialization();
            }
        }
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
    /// 팝업 열기 (기존 화면 위에 올리기)
    /// </summary>
    public void ShowPopup(PopupType popupType)
    {
        if (popupUIs.ContainsKey(popupType))
        {
            popupUIs[popupType].Open();
            activePopups.Push(popupType);
            Debug.Log($"팝업 열기: {popupType}");
        }
        else
        {
            Debug.LogError($"Popup UI not found: {popupType}");
        }
    }
    
    /// <summary>
    /// 특정 팝업 닫기
    /// </summary>
    public void ClosePopup(PopupType popupType)
    {
        if (popupUIs.ContainsKey(popupType))
        {
            popupUIs[popupType].Close();
            
            // 스택에서 제거
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
            
            Debug.Log($"팝업 닫기: {popupType}");
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
            popupUIs[topPopup].Close();
            Debug.Log($"최상위 팝업 닫기: {topPopup}");
        }
    }
    
    /// <summary>
    /// 모든 팝업 닫기
    /// </summary>
    public void CloseAllPopups()
    {
        while (activePopups.Count > 0)
        {
            PopupType popup = activePopups.Pop();
            popupUIs[popup].Close();
        }
        Debug.Log("모든 팝업 닫기");
    }
    
    public bool IsPopupOpen(PopupType popupType)
    {
        return activePopups.Contains(popupType);
    }
    
    #endregion
    
    #region Modal Management
    
    /// <summary>
    /// 모달 열기 (최상위)
    /// </summary>
    public void ShowModal(ModalType modalType)
    {
        if (modalUIs.ContainsKey(modalType))
        {
            modalUIs[modalType].Open();
            activeModals.Push(modalType);
            Debug.Log($"모달 열기: {modalType}");
        }
        else
        {
            Debug.LogError($"Modal UI not found: {modalType}");
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
            modalUIs[topModal].Close();
            Debug.Log($"모달 닫기: {topModal}");
        }
    }
    
    /// <summary>
    /// 모든 모달 닫기
    /// </summary>
    public void CloseAllModals()
    {
        while (activeModals.Count > 0)
        {
            ModalType modal = activeModals.Pop();
            modalUIs[modal].Close();
        }
        Debug.Log("모든 모달 닫기");
    }
    
    #endregion
    
    #region Helper Methods
    
    private ScreenType GetScreenTypeFromName(string name)
    {
        if (name.Contains("Start")) return ScreenType.Start;
        if (name.Contains("Main")) return ScreenType.Main;
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