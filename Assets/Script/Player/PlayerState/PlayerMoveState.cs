using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerGroundedState
{
    public PlayerMoveState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        if (player.sfx != null)
            player.sfx.StartFootstepLoop();
    }

    public override void Exit()
    {
        if (player.sfx != null)
            player.sfx.StopFootstepLoop();
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        player.SetVelocity(xInput*player.moveSpeed, rb.velocity.y);
        if(xInput==0)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }
}
