using UnityEngine;

public class UICanvasStageSelect : MonoBehaviour, IGUI
{
    public GameObject GUIObject => gameObject;
    public void Initialization()
    {
        Close();
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