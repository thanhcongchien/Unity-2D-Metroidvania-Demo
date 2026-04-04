using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpAttackState : PlayerState
{
    private bool touchedGround;

    public PlayerJumpAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        touchedGround = false;

        player.SetVelocity(player.jumpAttackMovement.x * player.facingDir, player.jumpAttackMovement.y);

    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if(player.isGroundDetected()&&touchedGround==false)
        {
            touchedGround = true;
            player.anim.SetTrigger("JumpAttackTrigger");
            player.SetVelocity(0, rb.velocity.y);


        }
        if(triggerCalled&&player.isGroundDetected())
        {
            stateMachine.ChangeState(player.idleState);
        }

    }
}
