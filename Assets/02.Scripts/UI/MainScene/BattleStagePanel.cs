using UnityEngine;

public class BattleStagePanel : MonoBehaviour, IGUI
{
    public GameObject GUIObject => gameObject;
    
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
