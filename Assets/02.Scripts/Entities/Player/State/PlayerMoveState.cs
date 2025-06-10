using UnityEngine;

public class PlayerMoveState : IState
{
    private PlayerController player;

    public PlayerMoveState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        Debug.Log("Enter MoveState");
        player.AnimationHandler.SetAnimationState(AnimationState.Move);
    }

    public void Update()
    {
        player.DetectEnemy();

        if (player.IsEnemyInRange())
        {
            player.StateMachine.ChangeState(new PlayerAttackState(player));
            return;
        }
        
        if (!player.CanMove())
        {
            player.StateMachine.ChangeState(new PlayerIdleState(player));
            return;
        }
        
        player.MoveForward();
    }

    public void Exit()
    {
        Debug.Log("Exit MoveState");
    }
}