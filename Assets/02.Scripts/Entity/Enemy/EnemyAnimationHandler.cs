using System;
using System.Collections;
using UnityEngine;

public class EnemyAnimationHandler : MonoBehaviour
{
    private Animator animator;

    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private static readonly int OnHitHash = Animator.StringToHash("onHit");
    private static readonly int OnDieHash = Animator.StringToHash("onDie");

    public Action OnDieEndCallback;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    public void SetOverrideController(AnimatorOverrideController overrideController)
    {
        animator.runtimeAnimatorController = overrideController;
    }

    public void PlayIdle()
    {
        animator.SetBool(IsMovingHash, false);
    }

    public void StopIdle()
    {
        animator.SetBool(IsMovingHash, true);
    }

    public void PlayHit()
    {
        animator.SetTrigger(OnHitHash);
    }

    public void PlayDie()
    {
        animator.SetTrigger(OnDieHash);
        StartCoroutine(WaitForDieAnimationEnd());
    }

    private IEnumerator WaitForDieAnimationEnd()
    {
        yield return null; // 애니메이터 상태 갱신 대기

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float length = state.length;

        yield return new WaitForSeconds(length);

        OnDieEndCallback?.Invoke();
    }
}