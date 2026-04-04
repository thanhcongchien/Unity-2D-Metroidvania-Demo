using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerAiredState
{
    public PlayerJumpState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        rb.velocity=new Vector2(rb.velocity.x,player.jumpForce);

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
        if(rb.velocity.y<0&&stateMachine.currentState!=player.jumpAttack)
        {
            stateMachine.ChangeState(player.airState);
        }
    }
}
