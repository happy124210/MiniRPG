using System;
using TMPro;
using UnityEngine;

public class BattleStatPanel : MonoBehaviour, IGUI
{
    public GameObject GUIObject => gameObject;
    
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI keyText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI gemText;

    private StatHandler stat;

    private void Awake()
    {
        levelText = transform.FindChild<TextMeshProUGUI>("Tmp_LevelComment");
        keyText = transform.FindChild<TextMeshProUGUI>("Tmp_KeyComment");
        goldText = transform.FindChild<TextMeshProUGUI>("Tmp_GoldComment");
        gemText = transform.FindChild<TextMeshProUGUI>("Tmp_GemComment");
    }

    public void Initialization()
    {
        stat = CharacterManager.Player.StatHandler;
        stat.OnStatChanged += UpdateUI;
        UpdateUI();
        
        Close();
    }

    public void OnDisable()
    {
        if (stat != null)
        {
            stat.OnStatChanged -= UpdateUI;
        }
    }

    private void UpdateUI()
    {
        levelText.text = $"LEVEL {stat.GetStat(StatType.Level)}";
        keyText.text = $"{stat.GetStat(StatType.Key)} / {stat.GetStat(StatType.MaxKey)}";
        goldText.text = stat.GetStat(StatType.Gold).ToString();
        gemText.text = stat.GetStat(StatType.Gem).ToString();
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
