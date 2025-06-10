using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool IsGamePaused { get; private set; }
    
    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    
    private void Start()
    {
        UIManager.Instance.Initialize();
        UIManager.Instance.ShowScreen(ScreenType.Main);
        
        // TODO: 스테이지 선택 시 연결
        StageData firstStage = Resources.Load<StageData>("Stages/Stage01");
        if (firstStage != null)
        {
            StageManager.Instance.LoadStage(firstStage);
        }
    }
    
    
    public static void PauseGame()
    {
        Time.timeScale = 0f;
        IsGamePaused = true;
    }
    
    
    public static void ResumeGame()
    {
        Time.timeScale = 1f;
        IsGamePaused = false;
    }
}
