using UnityEngine;

public class BattleStagePanel : MonoBehaviour, IGUI
{
    public GameObject GUIObject => gameObject;
    
    [Header("Stage Info")]
    
    public void Initialization()
    {
        
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
