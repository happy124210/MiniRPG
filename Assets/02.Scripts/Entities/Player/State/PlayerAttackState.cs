using System.Collections;
using UnityEngine;

public class PlayerAttackState : IState
{
    private PlayerController player;
    private Coroutine attackRoutine;
    private bool animationEnded;
    
    public PlayerAttackState(PlayerController player)
    {
        this.player = player;
    }
    
    
    public void Enter()
    {
        Debug.Log("Enter AttackState");
        
        player.AnimationHandler.OnAttackHitCallback = OnAttackHit;
        player.AnimationHandler.OnAttackEndCallback = OnAttackEnd;
        
        attackRoutine = player.StartCoroutine(AttackLoop());
        player.AnimationHandler.SetAnimationState(AnimationState.Attack);
    }
    
    
    public void Update()
    {
        // 죽음 처리
        if (player.StatHandler.IsDead())
        {
            player.StateMachine.ChangeState(new PlayerDeadState(player));
        }
    }

    
    public void Exit()
    {
        player.AnimationHandler.OnAttackHitCallback = null;
        player.AnimationHandler.OnAttackEndCallback = null;
        
        if (attackRoutine != null)
        {
            player.StopCoroutine(attackRoutine);
        }
        
        Debug.Log("Exit AttackState");
    }

    
    private IEnumerator AttackLoop()
    {
        while (player.IsEnemyInRange())
        {
            animationEnded = false;
            player.AnimationHandler.SetAnimationState(AnimationState.Attack);

            yield return new WaitUntil(() => animationEnded);
            
            if (!player.IsEnemyInRange()) break;
            yield return new WaitForSeconds(player.AttackInterval);
        }
        
        player.StateMachine.ChangeState(new PlayerMoveState(player));
    }
    
    private void OnAttackHit()
    {
        player.PerformAttack(); // 실제 대미지 처리
    }

    
    private void OnAttackEnd()
    {
        animationEnded = true;
    }
}