using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerState
{
    public PlayerGroundedState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
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
       
        // 反击输入：由输入快照驱动，避免多处直接读 Input。
        if(player.input.counterPressed) 
        {
            stateMachine.ChangeState(player.counterAttack);
        }
     
        // 右键按下且技能可用时进入投剑状态。
        if (player.input.secondaryAttackPressed && skillManager.swordThrow.CanUseSkill())
             stateMachine.ChangeState(player.swordThrowState);

        // 左键按下进入地面普攻状态。
        if (player.input.primaryAttackPressed) 
        {

            
            stateMachine.ChangeState(player.primaryAttack);
        
        }
        if(player.isGroundDetected()==false )
        {
            stateMachine.ChangeState(player.airState);
        }
        // 跳跃输入：在地面检测成立时切换到跳跃状态。
        if(player.input.jumpPressed && player.isGroundDetected()) 
        {
            stateMachine.ChangeState(player.jumpState);
        }
    }
}
