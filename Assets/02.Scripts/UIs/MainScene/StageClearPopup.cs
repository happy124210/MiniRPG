using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageClearPopup : MonoBehaviour, IGUI
{
    [Header("Main Panels")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private GameObject rewardsPanel;
    
    [Header("Reward Panel")]
    [SerializeField] private Button rewardBox;
    [SerializeField] private TextMeshProUGUI rewardRarityText;
    [SerializeField] private Image rewardIconImage;
    [SerializeField] private TextMeshProUGUI rewardNameText;
    [SerializeField] private TextMeshProUGUI rewardPowerLevelText;
    
    [Header("Rewards Panel")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI gainedExpText;
    [SerializeField] private TextMeshProUGUI rewardGoldText;
    [SerializeField] private TextMeshProUGUI rewardGemText;
    [SerializeField] private Image rewardItemImage;
    [SerializeField] private Button stageSelectButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float boxAnimationDuration = 1f;
    [SerializeField] private float rewardShowDelay = 0.5f;
    [SerializeField] private float rewardAnimationDuration = 0.8f;
    
    private StageData currentStageData;
    private bool isBoxOpened = false;
    private bool waitingForTap = false;
    
    public GameObject GUIObject => gameObject;

    private void Reset()
    {
        rewardPanel = transform.Find("Img_RewardPanel").gameObject;
        rewardsPanel = transform.Find("Img_RewardsPanel").gameObject;
        
        rewardBox = transform.FindChild<Button>("Btn_RewardBox");
        rewardRarityText = transform.FindChild<TextMeshProUGUI>("Tmp_RewardRarity");
        rewardIconImage = transform.FindChild<Image>("Img_RewardIcon");
        rewardNameText = transform.FindChild<TextMeshProUGUI>("Tmp_RewardName");
        rewardPowerLevelText = transform.FindChild<TextMeshProUGUI>("Tmp_RewardPowerLevel");
        
        levelText = transform.FindChild<TextMeshProUGUI>("Tmp_Level");
        gainedExpText = transform.FindChild<TextMeshProUGUI>("Tmp_GainedExp");
        rewardGoldText = transform.FindChild<TextMeshProUGUI>("Tmp_RewardGoldAmount");
        rewardGemText = transform.FindChild<TextMeshProUGUI>("Tmp_RewardGemAmount");
        rewardItemImage = transform.FindChild<Image>("Img_RewardItem");
        stageSelectButton = transform.FindChild<Button>("Btn_StageSelect");
    }
    
    
    private void Awake()
    {
        rewardPanel = transform.Find("Img_RewardPanel").gameObject;
        rewardsPanel = transform.Find("Img_RewardsPanel").gameObject;
        
        rewardBox = transform.FindChild<Button>("Btn_RewardBox");
        rewardRarityText = transform.FindChild<TextMeshProUGUI>("Tmp_RewardRarity");
        rewardIconImage = transform.FindChild<Image>("Img_RewardIcon");
        rewardNameText = transform.FindChild<TextMeshProUGUI>("Tmp_RewardName");
        rewardPowerLevelText = transform.FindChild<TextMeshProUGUI>("Tmp_RewardPowerLevel");
        
        levelText = transform.FindChild<TextMeshProUGUI>("Tmp_Level");
        gainedExpText = transform.FindChild<TextMeshProUGUI>("Tmp_GainedExp");
        rewardGoldText = transform.FindChild<TextMeshProUGUI>("Tmp_RewardGoldAmount");
        rewardGemText = transform.FindChild<TextMeshProUGUI>("Tmp_RewardGemAmount");
        rewardItemImage = transform.FindChild<Image>("Img_RewardItem");
        stageSelectButton = transform.FindChild<Button>("Btn_StageSelect");
    }
    
    
    private void Update()
    {
        if (!waitingForTap || !Input.GetMouseButtonDown(0)) return;
        
        waitingForTap = false;
        ShowRewards();
    }
    

    private void OnDestroy()
    {
        StageManager.OnStageComplete -= OnStageComplete;
        DOTween.Kill(transform);
    }
    
    public void Initialization()
    {
        StageManager.OnStageComplete += OnStageComplete;

        ResetUI();
        Close();
    }
    

    public void Open()
    {
        gameObject.SetActive(true);
        
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    
    public void Close()
    {
        gameObject.SetActive(false);
        
        rewardPanel.SetActive(false);
        rewardsPanel.SetActive(false);
    }
    
    
    private void OnStageComplete(StageData stageData)
    {
        currentStageData = stageData;
        
        // UI 데이터 설정
        SetupRewardData();
        
        // UI 표시
        Open();
    }
    
    
    public void OnRewardBoxClicked()
    {
        if (isBoxOpened) return;
        
        isBoxOpened = true;
        rewardBox.interactable = false;

        PlayBoxOpenAnimation();
    }
    
    
    private void SetupRewardData()
    {
        if (currentStageData == null) return;
        
        Player player = CharacterManager.Player;
        if (player == null) return;
        
        rewardGoldText.text = currentStageData.rewardGold.ToString();
        rewardGemText.text = currentStageData.rewardGem.ToString();
        gainedExpText.text = $"+{currentStageData.rewardExp} EXP";
        
        int currentLevel = player.StatHandler.GetStat(StatType.Level);
        levelText.text = $"LEVEL {currentLevel}";
        
        SetupRewardItem();
    }
    
    
    private void SetupRewardItem()
    {
        ItemData rewardItem = currentStageData.rewardItem;
        
        rewardRarityText.text = rewardItem.itemRarity.ToString();
        rewardIconImage.sprite = rewardItem.icon;
        rewardItemImage.sprite = rewardItem.icon;
        rewardNameText.text = rewardItem.itemName;
        rewardPowerLevelText.text = rewardItem.powerLevel.ToString();
    }
    
    // 외부 코드 참고
    private void PlayBoxOpenAnimation()
    {
        rewardPanel.SetActive(true);
        
        Sequence boxSequence = DOTween.Sequence();
        boxSequence.SetUpdate(true);
        
        boxSequence.Append(rewardPanel.transform.DOScale(Vector3.one * 0.6f, 0.2f))
            .Append(rewardPanel.transform.DOScale(Vector3.one * 0.4f, 0.1f))
            .Append(rewardPanel.transform.DOScale(Vector3.one * 0.5f, 0.1f));
        
        // 딜레이 후 보상 표시
        boxSequence.AppendInterval(rewardShowDelay);
        boxSequence.AppendCallback(() => WaitForScreenTap());
        rewardBox.gameObject.SetActive(false);
    }
    
    private void WaitForScreenTap()
    {
        waitingForTap = true;
    }
    
    private void ShowRewards()
    {

        rewardsPanel.SetActive(true);
        rewardsPanel.transform.localScale = Vector3.zero;
    
        Sequence rewardSequence = DOTween.Sequence();
        rewardSequence.SetUpdate(true);
        
        // 보상 패널 등장
        rewardSequence.Append(rewardsPanel.transform.DOScale(Vector3.one * 0.5f, rewardAnimationDuration)
            .SetEase(Ease.OutBounce));
        
        // 실제 보상 지급
        rewardSequence.AppendCallback(() => GiveRewardsToPlayer());
    }
    
    
    private void GiveRewardsToPlayer()
    {
        if (!currentStageData) return;
        
        Player player = CharacterManager.Player;
        if (!player?.StatHandler) return;

        player.StatHandler.ModifyStat(StatType.Gold, currentStageData.rewardGold);
        player.StatHandler.ModifyStat(StatType.Exp, currentStageData.rewardExp);
        player.StatHandler.ModifyStat(StatType.Gem, currentStageData.rewardGem);
        
        Debug.Log("GIVE REWARDS");
        
        // TODO: 아이템 보상 지급
        // GiveItemRewards();
    }
    
    
    private void ResetUI()
    {
        isBoxOpened = false;
        
        rewardBox.interactable = true;
        rewardsPanel.SetActive(false);

        rewardPanel.transform.localScale = Vector3.one;
        rewardPanel.transform.rotation = Quaternion.identity;
    }
}
