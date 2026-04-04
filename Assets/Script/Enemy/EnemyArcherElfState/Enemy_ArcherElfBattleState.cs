using UnityEngine;

public class Enemy_ArcherElfBattleState : EnemyBattleState
{
    private const float KiteStartDistancePercent = 0.6f;

    public Enemy_ArcherElfBattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
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
        float targetDistance = DistanceToTarget();
        bool canSenseTarget = enemy.CanSenseCurrentTarget();
        bool withinAttackRange = WithinAttackRange();
        bool attackReady = Time.time >= enemy.lastTimeAttack + enemy.attackCooldown;
        bool shouldKite = withinAttackRange && targetDistance <= enemy.attackDistance * KiteStartDistancePercent;

        if (canSenseTarget)
        {
            stateTimer = enemy.battleTime;

            if (targetDirection != 0)
                enemy.FlipController(targetDirection);

            if (attackReady == false && shouldKite)
            {
                BackstepFromTarget(targetDirection);
                return;
            }

            if (withinAttackRange)
            {
                stateMachine.ChangeState(enemy.attackState);
                return;
            }
        }
        else if (stateTimer < 0 || enemy.HasLostTarget())
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        if (ShouldRetreat())
        {
            MoveAwayFromTarget(targetDirection, true);
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

    private void BackstepFromTarget(int targetDirection)
    {
        if (targetDirection == 0)
        {
            StopBattleMovement();
            return;
        }

        if (ShouldRetreat())
        {
            MoveAwayFromTarget(targetDirection, true);
            return;
        }

        int backstepDirection = -targetDirection;

        if (enemy.IsGroundDetectedInDirection(backstepDirection) == false)
        {
            StopBattleMovement();
            return;
        }

        enemy.FlipController(targetDirection);
        TryMoveInDirection(backstepDirection, GetKiteMoveSpeed(), rb.velocity.y);
    }

    private void MoveAwayFromTarget(int targetDirection, bool useRetreatVelocity = false)
    {
        int moveDirection = -targetDirection;

        if (targetDirection == 0 || enemy.IsGroundDetectedInDirection(moveDirection) == false)
        {
            StopBattleMovement();
            return;
        }

        enemy.FlipController(targetDirection);

        if (useRetreatVelocity)
        {
            TryMoveInDirection(moveDirection, enemy.retreatVelocity.x * enemy.activeSlowMultiplier, enemy.retreatVelocity.y);
            return;
        }

        TryMoveInDirection(moveDirection, GetKiteMoveSpeed(), rb.velocity.y);
    }

    private float GetKiteMoveSpeed()
    {
        return enemy.battleMoveSpeed * enemy.activeSlowMultiplier;
    }
}
