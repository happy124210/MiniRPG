using UnityEngine;

public class PlayerIdleState : IState
{
    private PlayerController player;

    public PlayerIdleState(PlayerController player)
    {
        this.player = player;
    }
    
    public void Enter()
    {
        Debug.Log("Enter Idle State");
        player.AnimationHandler.SetAnimationState(AnimationState.Idle);
    }

    public void Update()
    {
        if (player.CanMove())
        {
            player.StateMachine.ChangeState(new PlayerMoveState(player));
        }
    }

    public void Exit()
    {
        Debug.Log("Exit Idle State");
    }
}
