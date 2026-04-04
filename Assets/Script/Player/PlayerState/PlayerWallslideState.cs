using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallslideState : PlayerState
{
    public PlayerWallslideState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

    }

    public override void Update()
    {
        base.Update();
        // 墙滑时按跳跃键，基于输入快照触发墙跳。
        if(player.input.jumpPressed) 
        {
            stateMachine.ChangeState(player.wallJumpState);
            return;
        }
        if (!player.isWallDetected())
            stateMachine.ChangeState(player.airState);
        if (yInput<0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y * .7f);
        }
        if (xInput!=0&& player.facingDir != xInput)
        {
            stateMachine.ChangeState(player.idleState);
        }
        if (player.isGroundDetected())
            stateMachine.ChangeState(player.idleState);
    }
}
