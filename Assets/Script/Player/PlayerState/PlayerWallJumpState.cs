using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerState
{
    private bool shouldPlayLandingSfx;

    public PlayerWallJumpState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = .4f;
        shouldPlayLandingSfx = false;
        player.SetVelocity(player.wallJumpForce.x * -player.facingDir, player.wallJumpForce.y);

        if (player.sfx != null)
            player.sfx.PlayJump();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (rb.velocity.y < -.1f)
            shouldPlayLandingSfx = true;

        if(stateTimer < 0) 
        {
            stateMachine.ChangeState(player.airState);
        }
        if (player.isGroundDetected())
        {
            if (shouldPlayLandingSfx && player.sfx != null)
                player.sfx.PlayLand();

            stateMachine.ChangeState(player.idleState);
        }
    }
}
