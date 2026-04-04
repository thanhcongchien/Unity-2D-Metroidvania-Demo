using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState:EntityState
{
    
    protected Enemy enemy;

    

   

    public EnemyState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.enemy = enemy;

        rb = enemy.rb;
        anim = enemy.anim;
        stats=enemy.stats;
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
    }

    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();
    }
}
