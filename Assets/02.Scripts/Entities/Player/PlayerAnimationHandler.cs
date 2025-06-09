using System;
using UnityEngine;

public enum AnimationState
{
    Idle,
    Move,
    Attack,
    Dead
}

public class PlayerAnimationHandler : MonoBehaviour
{
    private Animator animator;
    
    public Action OnAttackHitCallback;
    public Action OnAttackEndCallback;

    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private static readonly int IsAttackingHash = Animator.StringToHash("isAttacking");
    private static readonly int IsDeadHash = Animator.StringToHash("isDead");

    private AnimationState currentState;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("No animator found");
    }

    /// <summary>
    /// 전달받은 상태의 Animation만 켜주기
    /// </summary>
    /// <param name="state"></param>
    public void SetAnimationState(AnimationState state)
    {
        currentState = state;
        
        // 모든 상태 OFF
        animator.SetBool(IsMovingHash, false);
        animator.SetBool(IsAttackingHash, false);
        animator.SetBool(IsDeadHash, false);

        // 전달받은 상태 ON
        switch (state)
        {
            case AnimationState.Move:
                animator.SetBool(IsMovingHash, true);
                break;
            
            case AnimationState.Attack:
                animator.SetBool(IsAttackingHash, true);
                break;
            
            case AnimationState.Dead:
                animator.SetBool(IsDeadHash, true);
                break;
            
            case AnimationState.Idle:
                
            default:
                // 아무 것도 안 켜면 Idle 상태 유지
                break;
        }
    }
    
    
    public void OnAttackHit()
    {
        OnAttackHitCallback?.Invoke();
    }
    
    
    public void OnAttackAnimationEnd()
    {
        OnAttackEndCallback?.Invoke();
    }
}