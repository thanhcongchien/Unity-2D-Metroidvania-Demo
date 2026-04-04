using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_ReaperBattleState : EnemyBattleState
{
    private Enemy_Reaper enemyReaper;
    private float battleIdleTimer;

    public Enemy_ReaperBattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyReaper = enemy as Enemy_Reaper;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = enemy.battleTime;

        ResetBattleIdleTimer();
    }


    public override void Update()
    {
        stateTimer -= Time.deltaTime;
        UpdateAnimationParameters();

        if (enemy.HasValidTarget() == false && enemy.TryFindTarget() == false)
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        enemy.TryFindTarget();

        int targetDirection = DirectionToTarget();
        bool canSenseTarget = enemy.CanSenseCurrentTarget();
        bool withinAttackRange = WithinAttackRange();
        bool shouldRetreat = ShouldRetreat();
        bool attackReady = Time.time >= enemy.lastTimeAttack + enemy.attackCooldown;
        bool movedThisFrame = false;

        if (targetDirection != 0)
            enemy.FlipController(targetDirection);

        if (canSenseTarget)
        {
            stateTimer = enemy.battleTime;

            if (shouldRetreat)
            {
                enemy.FlipController(targetDirection);
                movedThisFrame = TryMoveInDirection(-targetDirection, enemy.retreatVelocity.x * enemy.activeSlowMultiplier, enemy.retreatVelocity.y);
                UpdateBattleIdleTimer(movedThisFrame);
                return;
            }

            if (withinAttackRange && attackReady)
            {
                ResetBattleIdleTimer();

                if (CanAttack())
                    stateMachine.ChangeState(enemyReaper.reaperAttackState);

                return;
            }
        }
        else if (stateTimer < 0f || enemy.HasLostTarget())
        {
            StopBattleMovement();
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        if (targetDirection != 0)
        {
            enemy.FlipController(targetDirection);
            movedThisFrame = TryMoveInDirection(targetDirection, enemy.getBattleMoveSpeed(), rb.velocity.y);
        }
        else
        {
            StopBattleMovement();
        }

        UpdateBattleIdleTimer(movedThisFrame);
    }

    private void UpdateBattleIdleTimer(bool movedThisFrame)
    {
        if (movedThisFrame)
        {
            ResetBattleIdleTimer();
            return;
        }

        battleIdleTimer += Time.deltaTime;

        if (battleIdleTimer >= enemyReaper.maxBattleIdleTime)
        {
            if (enemyReaper.CanUseTeleport())
                stateMachine.ChangeState(enemyReaper.reaperTeleportState);
            else
                ResetBattleIdleTimer();
        }
    }

    private void ResetBattleIdleTimer()
    {
        battleIdleTimer = 0f;
    }
}
