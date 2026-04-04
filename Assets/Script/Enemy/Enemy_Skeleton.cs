using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Skeleton : Enemy,ICounterable
{
    public bool CanBeCountered { get => canBeStunned;}

    protected override void Awake()
    {
        base.Awake();

        idleState = new EnemyIdleState(this, stateMachine,"Idle");
        moveState = new EnemyMoveState(this, stateMachine,"Move");
        battleState = new EnemyBattleState(this, stateMachine, "Move");
        attackState = new EnemyAttackState(this, stateMachine, "Attack");
        stunnedState = new EnemyStunnedState(this, stateMachine, "Stun");
        deadState = new EnemyDeadState(this, stateMachine, "Idle"); ;
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    

    public void HandleCounter()
    {
        if(CanBeCountered==false)
            return;

        stateMachine.ChangeState(stunnedState);
    }

   //public override bool CanBeStunned()
   // {
   //     if(base.CanBeStunned())
   //     {
   //         stateMachine.ChangeState(stunnedState);
   //          return true;
   //     }
   //     return false;
   // }
}
