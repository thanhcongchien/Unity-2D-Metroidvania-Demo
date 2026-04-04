using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_ReaperAttackState : EnemyAttackState
{
    private Enemy_Reaper enemyReaper;


    public Enemy_ReaperAttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {

        enemyReaper = enemy as Enemy_Reaper;
    }

    public override void Update()
    {
        stateTimer -= Time.deltaTime;
        UpdateAnimationParameters();
        enemy.ZeroVelocity();

        if (triggerCalled == false)
            return;

        if (enemyReaper.ShouldTeleport())
            stateMachine.ChangeState(enemyReaper.reaperTeleportState);
        else
            stateMachine.ChangeState(enemyReaper.reaperBattleState);
    }
}
