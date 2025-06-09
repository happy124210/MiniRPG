using UnityEngine;

public class UICanvasMain : MonoBehaviour, IGUI
{
    public GameObject GUIObject => gameObject;
    
    [SerializeField] private BattleStatPanel battleStatPanel;
    [SerializeField] private BattleStagePanel battleStagePanel;

    public void Reset()
    {
        battleStatPanel = transform.FindChild<BattleStatPanel>("Group_Stat");
        battleStagePanel = transform.FindChild<BattleStagePanel>("Group_Stage");
    }

    public void Initialization()
    {
        battleStatPanel.Initialization();
        battleStagePanel.Initialization();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        
        battleStatPanel.Open();
        battleStagePanel.Open();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
