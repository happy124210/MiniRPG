using UnityEngine;

public class PlayerDeadState : IState
{
    private PlayerController player;

    public PlayerDeadState(PlayerController player)
    {
        this.player = player;
    }
    
    public void Enter()
    {
        Debug.Log("Enter Dead State");
        player.AnimationHandler.SetAnimationState(AnimationState.Dead);
        player.OnDeath();
    }

    public void Update()
    {
        // 죽음
    }

    public void Exit()
    {
        // 부활 시?
    }
}
