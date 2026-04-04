using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_SpellCastState : EnemyState
{
    private EnemyMage enemyMage;

    public Enemy_SpellCastState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyMage = enemy as EnemyMage;
    }

    public override void Enter()
    {
        base.Enter();

        enemyMage.ZeroVelocity();
        enemyMage.SetSpellCastPerformed(false);
    }

    public override void Update()
    {
        base.Update();

        if(enemyMage.spellCastPerformed)
            anim.SetBool("SpellCast_Performed", true);


        if(triggerCalled)
            stateMachine.ChangeState(enemy.battleState);
    }

    public override void Exit()
    {
        base.Exit();
        anim.SetBool("SpellCast_Performed", false);
    }
}
