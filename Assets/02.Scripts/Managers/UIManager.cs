using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum UIType
{
    None,
    Start,
    Main,
    Option,
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [SerializeField] private List<MonoBehaviour> guiListRaw;
    private Dictionary<UIType, IGUI> guiDict = new();

    
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
        
        foreach (var mono in guiListRaw)
        {
            if (mono is not IGUI gui)
            {
                Debug.LogWarning($"{mono.name}은 IGUI를 구현하지 않음");
                continue;
            }
            guiDict.Add(GetUITypeFromName(mono.name), gui);
        }
    }


    public IGUI GetGUI(UIType type)
    {
        return guiDict.GetValueOrDefault(type);
    }

    
    private UIType GetUITypeFromName(string name)
    {
        // 예: Canvas_Main → UIType.Main
        return (UIType)Enum.Parse(typeof(UIType), name.Replace("Canvas_", ""));
    }

    
    public void ShowUI(UIType type)
    {
        foreach (var gui in guiDict.Values) gui.Close();
        GetGUI(type)?.Open();
    }
}
