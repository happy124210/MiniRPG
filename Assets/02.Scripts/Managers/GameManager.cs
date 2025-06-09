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
        UIManager.Instance.GetGUI(UIType.Main).Initialization();
        UIManager.Instance.ShowUI(UIType.Main);
        
        // TODO
        // 버튼 클릭 시 호출로 변경
        
        StageData firstStage = Resources.Load<StageData>("Stages/Stage01");
        if (firstStage != null)
        {
            StageManager.Instance.LoadStage(firstStage);
        }
    }
}
