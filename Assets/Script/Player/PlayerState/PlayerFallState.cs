using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : PlayerAiredState
{
    private bool shouldPlayLandingSfx;

    public PlayerFallState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        shouldPlayLandingSfx = false;
    }

    public override void Exit()
    {
        base.Exit();
        player.SetVelocity(0, rb.velocity.y);
    }

    public override void Update()
    {
        base.Update();

        if (rb.velocity.y < -.1f)
            shouldPlayLandingSfx = true;

        if(player.isWallDetected())
        {
            stateMachine.ChangeState(player.wallslideState);
        }
        if (player.isGroundDetected())
        {
            if (shouldPlayLandingSfx && player.sfx != null)
                player.sfx.PlayLand();

            stateMachine.ChangeState(player.idleState);
        }
      
      
        
    }
}
