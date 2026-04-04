using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState:EntityState 
{
    
    protected Player player;
   
    protected float xInput;
    protected float yInput;

    protected PlayerSkillManager skillManager;
    public PlayerState(Player player, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.player = player;

        anim = player.anim;
        rb = player.rb;
        stats = player.stats;
        skillManager = player.skillManager;
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

        // 状态层只读 Player 的输入快照，不再直接读取 Unity Input。
        xInput = player.input.moveX;
        yInput = player.input.moveY;
       
    }
    
    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();
        player.anim.SetFloat("yVelocity", rb.velocity.y);
    }
}
