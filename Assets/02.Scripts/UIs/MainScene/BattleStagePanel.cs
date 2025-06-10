using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleStagePanel : MonoBehaviour, IGUI
{
    public GameObject GUIObject => gameObject;
    
    [Header("Stage Info")]
    [SerializeField] private Image stageGaugeBar;
    
    [Header("Wave Check")]
    //[SerializeField] private WaveCheckBox[] waveCheckBoxes;
    
    [Header("Gauge Animation Settings")]
    [SerializeField] private float gaugeFillDuration = 0.8f;       // 게이지 채우는 시간
    [SerializeField] private Ease gaugeFillEase = Ease.OutCubic;   // 게이지 이징
    [SerializeField] private float punchScale = 0.15f;             // 펀치 효과 크기
    [SerializeField] private float punchDuration = 0.4f;           // 펀치 효과 시간
    
    private Sequence currentGaugeSequence;
    private float lastProgress = 0f;

    private void Awake()
    {
        stageGaugeBar = transform.FindChild<Image>("Img_StageGaugeBar");
        if (stageGaugeBar == null) Debug.LogError("StageGaugeBar not found");
    }
    
    
    private void OnDestroy()
    {
        currentGaugeSequence?.Kill();
        
        // 구독 해제
        StageManager.OnStageStart -= OnStageStart;
        StageManager.OnStageProgressChanged -= OnStageProgressChanged;
        StageManager.OnStageComplete -= OnStageComplete;
    }

    
    public void Initialization()
    {
        StageManager.OnStageStart += OnStageStart;
        StageManager.OnStageProgressChanged += OnStageProgressChanged;
        StageManager.OnStageComplete += OnStageComplete;

        ResetGauge();
    }

    
    public void Open()
    {
        gameObject.SetActive(true);
    }

    
    public void Close()
    {
        gameObject.SetActive(false);
    }


    private void OnStageStart(StageData stageData)
    {
        ResetGauge();
    }


    private void OnStageProgressChanged(float progress)
    {
        AnimateGaugeProgress(progress);
    }

    private void OnStageComplete(StageData stageData)
    {
        PlayCompleteEffect();
    }
    
    
    private void AnimateGaugeProgress(float targetProgress)
    {
        if (stageGaugeBar == null) return;
        
        // 기존 애니메이션 중단
        currentGaugeSequence?.Kill();
        
        float currentProgress = stageGaugeBar.fillAmount;
        float progressDelta = targetProgress - currentProgress;
        
        // 진행률이 감소하는 경우 (리셋 등) 즉시 적용
        if (progressDelta <= 0)
        {
            stageGaugeBar.fillAmount = targetProgress;
            lastProgress = targetProgress;
            return;
        }
        
        currentGaugeSequence = DOTween.Sequence();
        
        // 메인 게이지 채우기 애니메이션
        currentGaugeSequence.Append(
            stageGaugeBar.DOFillAmount(targetProgress, gaugeFillDuration)
                .SetEase(gaugeFillEase)
        );
        
        // 마지막 진행률 저장
        currentGaugeSequence.OnComplete(() => lastProgress = targetProgress);
    }
    
    /// <summary>
    /// 스테이지 완료 시 최종 축하 효과
    /// </summary>
    private void PlayCompleteEffect()
    {
        if (!stageGaugeBar) return;
        
        // 기존 애니메이션 중단
        currentGaugeSequence?.Kill();
        
        Sequence completeSequence = DOTween.Sequence();
        
        // 큰 펀치 효과
        completeSequence.Append(
            stageGaugeBar.transform.DOPunchScale(Vector3.one * 0.25f, 0.6f, 8, 1.0f)
        );
    }

    /// <summary>
    /// 게이지 리셋 (부드러운 애니메이션)
    /// </summary>
    private void ResetGauge()
    {
        if (stageGaugeBar == null) return;

        // 애니메이션 중단
        currentGaugeSequence?.Kill();

        Sequence resetSequence = DOTween.Sequence();

        // 부드럽게 0으로 감소
        resetSequence.Append(stageGaugeBar.DOFillAmount(0f, 0.4f).SetEase(Ease.InCubic))
            .Join(stageGaugeBar.transform.DOScale(0.95f, 0.2f).SetLoops(2, LoopType.Yoyo));
    }
}
