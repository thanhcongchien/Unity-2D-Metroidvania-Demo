using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_MageBattleState : EnemyBattleState
{
    private EnemyMage enemyMage;
    private float lastTimeUsedRetreat = float.NegativeInfinity;


    public Enemy_MageBattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {

        enemyMage = enemy as EnemyMage;
    }

    public override void Enter()
    {
        base.Enter();
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

        if (canSenseTarget)
        {
            stateTimer = enemy.battleTime;

            if (targetDirection != 0)
                enemy.FlipController(targetDirection);

            if (shouldRetreat)
            {
                if (CanUseRetreatAbility())
                {
                    EnterRetreatState();
                }
                else
                {
                    BasicRetreat();
                }

                return;
            }

            if (withinAttackRange && attackReady)
            {
                if (CanAttack())
                    stateMachine.ChangeState(enemy.attackState);

                return;
            }
        }
        else if (stateTimer < 0 || enemy.HasLostTarget())
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        if (targetDirection != 0)
        {
            enemy.FlipController(targetDirection);
            TryMoveInDirection(targetDirection, enemy.getBattleMoveSpeed(), rb.velocity.y);
        }
        else
        {
            StopBattleMovement();
        }
    }
    private void EnterRetreatState()
    {
        lastTimeUsedRetreat = Time.time;
        stateMachine.ChangeState(enemyMage.retreatState);

    }
    private bool CanUseRetreatAbility() => Time.time > lastTimeUsedRetreat + enemyMage.retreatCooldown;
}
