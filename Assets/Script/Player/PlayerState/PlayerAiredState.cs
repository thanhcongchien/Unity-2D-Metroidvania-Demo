using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAiredState : PlayerState
{
    public PlayerAiredState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
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

        // 空中横向移动仍然使用输入快照中的 x 轴。
        if (xInput != 0)
        {
            player.SetVelocity(player.moveSpeed * .8f * xInput, rb.velocity.y);
        }
        // 空中左键按下切换到空中攻击。
        if(player.input.primaryAttackPressed)
        {
            stateMachine.ChangeState(player.jumpAttack);
        }
    }
}
