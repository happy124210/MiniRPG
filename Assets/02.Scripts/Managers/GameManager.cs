using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
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
}
