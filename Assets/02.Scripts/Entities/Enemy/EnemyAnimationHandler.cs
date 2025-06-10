using System;
using System.Collections;
using UnityEngine;

public class EnemyAnimationHandler : MonoBehaviour
{
    private Animator animator;

    // 애니메이션 파라미터 해시 (성능 최적화)
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private static readonly int IsAttackingHash = Animator.StringToHash("isAttacking");
    private static readonly int OnHitHash = Animator.StringToHash("onHit");
    private static readonly int OnDieHash = Animator.StringToHash("onDie");

    // 콜백 이벤트들
    public Action OnDieEndCallback;
    public Action OnAttackHitCallback;   // 공격이 실제로 적중하는 시점
    public Action OnAttackEndCallback;   // 공격 애니메이션 종료 시점
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"{gameObject.name}: Animator 컴포넌트를 찾을 수 없습니다!");
        }
    }
    
    public void SetOverrideController(AnimatorOverrideController overrideController)
    {
        if (animator != null && overrideController != null)
        {
            animator.runtimeAnimatorController = overrideController;
        }
    }

    #region 애니메이션 상태 제어

    /// <summary>
    /// Idle 애니메이션 재생 (모든 다른 상태 OFF)
    /// </summary>
    public void PlayIdle()
    {
        if (animator == null) return;
        
        animator.SetBool(IsMovingHash, false);
        animator.SetBool(IsAttackingHash, false);
    }

    /// <summary>
    /// Move 애니메이션 재생
    /// </summary>
    public void PlayMove()
    {
        if (animator == null) return;
        
        animator.SetBool(IsMovingHash, true);
        animator.SetBool(IsAttackingHash, false);
    }

    /// <summary>
    /// Attack 애니메이션 재생
    /// </summary>
    public void PlayAttack()
    {
        if (animator == null) return;
        
        animator.SetBool(IsMovingHash, false);
        animator.SetBool(IsAttackingHash, true);
    }

    /// <summary>
    /// Hit 애니메이션 트리거 (일회성)
    /// </summary>
    public void PlayHit()
    {
        if (animator == null) return;
        
        animator.SetTrigger(OnHitHash);
    }

    /// <summary>
    /// Die 애니메이션 트리거 및 종료 대기
    /// </summary>
    public void PlayDie()
    {
        if (animator == null) return;
        
        // 다른 모든 상태 정지
        animator.SetBool(IsMovingHash, false);
        animator.SetBool(IsAttackingHash, false);
        
        // 사망 애니메이션 트리거
        animator.SetTrigger(OnDieHash);
        StartCoroutine(WaitForDieAnimationEnd());
    }

    #endregion

    #region 애니메이션 이벤트 (Animation Event에서 호출)

    /// <summary>
    /// 공격 애니메이션에서 실제 데미지를 주는 시점에 호출
    /// </summary>
    public void OnAttackHit()
    {
        OnAttackHitCallback?.Invoke();
    }

    /// <summary>
    /// 공격 애니메이션이 완전히 끝났을 때 호출
    /// </summary>
    public void OnAttackAnimationEnd()
    {
        OnAttackEndCallback?.Invoke();
    }

    #endregion

    #region 코루틴

    /// <summary>
    /// 사망 애니메이션이 끝날 때까지 대기
    /// </summary>
    private IEnumerator WaitForDieAnimationEnd()
    {
        yield return null; // 애니메이터 상태 갱신 대기

        // 현재 애니메이션 정보 가져오기
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // Die 애니메이션이 재생될 때까지 대기
        while (!stateInfo.IsName("Die") && !stateInfo.IsName("Death"))
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        // 애니메이션 길이만큼 대기
        float animationLength = stateInfo.length;
        yield return new WaitForSeconds(animationLength);

        // 사망 처리 완료 콜백 호출
        OnDieEndCallback?.Invoke();
    }

    #endregion
}