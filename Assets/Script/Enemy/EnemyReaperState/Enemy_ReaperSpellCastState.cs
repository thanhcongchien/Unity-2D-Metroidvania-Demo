using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_ReaperSpellCastState : EnemyState
{
    private Enemy_Reaper enemyReaper;
    private float performedExitTimer;
    private const float PerformedExitDelay = .35f;

    public Enemy_ReaperSpellCastState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyReaper = enemy as Enemy_Reaper;
    }

    public override void Enter()
    {
        base.Enter();

        enemyReaper.SetVelocity(0, 0);
        enemyReaper.SetSpellCastPerformed(false);
        enemyReaper.SetSpellOnCooldown();
        performedExitTimer = -1f;
    }

    public override void Update()
    {
        base.Update();
        if(enemyReaper.spellCastPerformed)
        {
            anim.SetBool("SpellCast_Performed", true);
            if (performedExitTimer < 0f)
                performedExitTimer = PerformedExitDelay;
        }

        if (performedExitTimer >= 0f)
        {
            performedExitTimer -= Time.deltaTime;
        }

        if(triggerCalled || (enemyReaper.spellCastPerformed && performedExitTimer <= 0f))
        {
            bool shouldTeleport = enemyReaper.ShouldTeleport();
            stateMachine.ChangeState(shouldTeleport ? enemyReaper.reaperTeleportState : enemyReaper.reaperBattleState);
        }
    }
    public override void Exit()
    {
        base.Exit();
        anim.SetBool("SpellCast_Performed", false);
    }
}
