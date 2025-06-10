using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public enum SceneName
{
    MainMenu,
    StageSelect,
    GamePlay,
    Settings
}

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    
    [Header("Transition Settings")]
    [SerializeField] private GameObject fadePanel;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    
    [Header("Loading")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private bool showLoadingScreen = true;
    
    // 현재 로딩 중인 스테이지 데이터
    private static StageData pendingStageData;
    
    // Events
    public static event Action<SceneName> OnSceneLoadStart;
    public static event Action<SceneName> OnSceneLoadComplete;
    
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
        
        // 페이드 패널 초기 설정
        SetupFadePanel();
    }
    
    /// <summary>
    /// 페이드 패널 초기 설정
    /// </summary>
    private void SetupFadePanel()
    {
        if (fadePanel == null)
        {
            // 동적으로 페이드 패널 생성
            CreateFadePanel();
        }
        
        if (fadeCanvasGroup == null && fadePanel != null)
        {
            fadeCanvasGroup = fadePanel.GetComponent<CanvasGroup>();
            if (fadeCanvasGroup == null)
                fadeCanvasGroup = fadePanel.AddComponent<CanvasGroup>();
        }
        
        // 초기 상태: 투명
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
        
        // 로딩 패널 숨기기
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
    
    /// <summary>
    /// 동적으로 페이드 패널 생성
    /// </summary>
    private void CreateFadePanel()
    {
        // Canvas 생성
        GameObject canvasObj = new GameObject("TransitionCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // 최상위
        canvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObj);
        
        // 페이드 패널 생성
        fadePanel = new GameObject("FadePanel");
        fadePanel.transform.SetParent(canvasObj.transform, false);
        
        // RectTransform 설정 (전체 화면)
        RectTransform rectTransform = fadePanel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // 검은색 이미지 추가
        Image image = fadePanel.AddComponent<Image>();
        image.color = Color.black;
        
        // CanvasGroup 추가
        fadeCanvasGroup = fadePanel.AddComponent<CanvasGroup>();
    }
    
    #region Public Scene Transition Methods
    
    /// <summary>
    /// 메인 메뉴로 이동
    /// </summary>
    public static void LoadMainMenu()
    {
        LoadScene(SceneName.MainMenu);
    }
    
    /// <summary>
    /// 스테이지 선택 화면으로 이동
    /// </summary>
    public static void LoadStageSelect()
    {
        LoadScene(SceneName.StageSelect);
    }
    
    /// <summary>
    /// 게임플레이 씬으로 이동 (특정 스테이지)
    /// </summary>
    public static void LoadGamePlay(StageData stageData = null)
    {
        pendingStageData = stageData;
        LoadScene(SceneName.GamePlay);
    }
    
    /// <summary>
    /// 설정 화면으로 이동
    /// </summary>
    public static void LoadSettings()
    {
        LoadScene(SceneName.Settings);
    }
    
    /// <summary>
    /// 일반적인 씬 로드
    /// </summary>
    public static void LoadScene(SceneName sceneName)
    {
        if (Instance == null)
        {
            Debug.LogError("[SceneTransitionManager] Instance가 없습니다!");
            return;
        }
        
        Instance.StartCoroutine(Instance.LoadSceneCoroutine(sceneName));
    }
    
    /// <summary>
    /// 씬 이름으로 직접 로드 (외부 씬용)
    /// </summary>
    public static void LoadSceneByName(string sceneName)
    {
        if (Instance == null)
        {
            Debug.LogError("[SceneTransitionManager] Instance가 없습니다!");
            return;
        }
        
        Instance.StartCoroutine(Instance.LoadSceneByNameCoroutine(sceneName));
    }
    
    #endregion
    
    #region Scene Loading Implementation
    
    /// <summary>
    /// 씬 로딩 코루틴
    /// </summary>
    private IEnumerator LoadSceneCoroutine(SceneName sceneName)
    {
        string sceneNameString = GetSceneNameString(sceneName);
        
        Debug.Log($"[SceneTransitionManager] 씬 전환 시작: {sceneName}");
        OnSceneLoadStart?.Invoke(sceneName);
        
        // 1. 페이드 아웃 (검은색으로)
        yield return StartCoroutine(FadeOut());
        
        // 2. 로딩 화면 표시
        if (showLoadingScreen && loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        
        // 3. 실제 씬 로딩
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneNameString);
        asyncLoad.allowSceneActivation = false;
        
        // 로딩 진행률 체크
        while (asyncLoad.progress < 0.9f)
        {
            Debug.Log($"[SceneTransitionManager] 로딩 진행률: {asyncLoad.progress * 100:F1}%");
            yield return null;
        }
        
        // 4. 씬 활성화
        asyncLoad.allowSceneActivation = true;
        
        // 씬 로딩 완료 대기
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // 5. 씬 로딩 후 처리
        yield return StartCoroutine(OnSceneLoadedCoroutine(sceneName));
        
        // 6. 로딩 화면 숨기기
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
        
        // 7. 페이드 인
        yield return StartCoroutine(FadeIn());
        
        OnSceneLoadComplete?.Invoke(sceneName);
        Debug.Log($"[SceneTransitionManager] 씬 전환 완료: {sceneName}");
    }
    
    /// <summary>
    /// 씬 이름으로 로딩하는 코루틴
    /// </summary>
    private IEnumerator LoadSceneByNameCoroutine(string sceneName)
    {
        Debug.Log($"[SceneTransitionManager] 외부 씬 로딩: {sceneName}");
        
        yield return StartCoroutine(FadeOut());
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        yield return asyncLoad;
        
        yield return StartCoroutine(FadeIn());
        
        Debug.Log($"[SceneTransitionManager] 외부 씬 로딩 완료: {sceneName}");
    }
    
    /// <summary>
    /// 씬 로딩 완료 후 처리
    /// </summary>
    private IEnumerator OnSceneLoadedCoroutine(SceneName sceneName)
    {
        switch (sceneName)
        {
            case SceneName.GamePlay:
                // 게임플레이 씬에서 스테이지 데이터 설정
                if (pendingStageData != null)
                {
                    yield return new WaitForSeconds(0.1f); // 씬 초기화 대기
                    
                    if (StageManager.Instance != null)
                    {
                        StageManager.Instance.LoadStage(pendingStageData);
                        Debug.Log($"[SceneTransitionManager] 스테이지 로딩: {pendingStageData.stageName}");
                    }
                    
                    pendingStageData = null; // 사용 후 정리
                }
                
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.Initialize();
                    UIManager.Instance.ShowScreen(ScreenType.Main);
                }
                break;
            
            
                
            case SceneName.MainMenu:
                // 메인 메뉴 초기화
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.Initialize();
                    UIManager.Instance.ShowScreen(ScreenType.Start);
                }
                break;
                
            case SceneName.StageSelect:
                // 스테이지 선택 화면 초기화
                // TODO: StageSelectManager 초기화
                break;
        }
        
        yield return null;
    }
    
    #endregion
    
    #region Fade Effects
    
    /// <summary>
    /// 페이드 아웃 (어두워지기)
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (fadeCanvasGroup == null) yield break;
        
        fadeCanvasGroup.blocksRaycasts = true;
        
        Tween fadeTween = fadeCanvasGroup.DOFade(1f, fadeDuration).SetUpdate(true);
        yield return fadeTween.WaitForCompletion();
    }
    
    /// <summary>
    /// 페이드 인 (밝아지기)
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (fadeCanvasGroup == null) yield break;
        
        Tween fadeTween = fadeCanvasGroup.DOFade(0f, fadeDuration).SetUpdate(true);
        yield return fadeTween.WaitForCompletion();
        
        fadeCanvasGroup.blocksRaycasts = false;
    }
    
    #endregion
    
    #region Utility
    
    /// <summary>
    /// SceneName enum을 문자열로 변환
    /// </summary>
    private string GetSceneNameString(SceneName sceneName)
    {
        return sceneName switch
        {
            SceneName.MainMenu => "MainMenu",
            SceneName.StageSelect => "StageSelect", 
            SceneName.GamePlay => "GamePlay",
            SceneName.Settings => "Settings",
            _ => "MainMenu"
        };
    }
    
    /// <summary>
    /// 현재 씬 이름 가져오기
    /// </summary>
    public static string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
    
    /// <summary>
    /// 즉시 씬 전환 (페이드 없음) - 디버깅용
    /// </summary>
    [ContextMenu("Quick Load MainMenu")]
    public void QuickLoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    [ContextMenu("Quick Load StageSelect")]
    public void QuickLoadStageSelect()
    {
        SceneManager.LoadScene("StageSelect");
    }
    
    #endregion
}