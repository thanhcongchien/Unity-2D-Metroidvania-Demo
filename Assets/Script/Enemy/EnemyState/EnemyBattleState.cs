using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBattleState : EnemyState
{
    private int moveDir;

    public EnemyBattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // Modified: battle state keeps or refreshes the enemy-owned target.
        enemy.TryFindTarget();
        SetBattleAnimation(false);
    }

    public override void Exit()
    {
        ClearBattleAnimation();
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (enemy.HasValidTarget() == false && enemy.TryFindTarget() == false)
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        enemy.TryFindTarget();

        if (Time.time < enemy.lastTimeAttack + enemy.attackCooldown)
        {
            StopBattleMovement();
            return;
        }

        if (enemy.CanSenseCurrentTarget())
        {
            stateTimer = enemy.battleTime;
            if (WithinAttackRange() && enemy.CanSenseCurrentTarget())
            {
                if (CanAttack())
                {
                    stateMachine.ChangeState(enemy.attackState);
                    return;
                }
            }
        }
        else if (stateTimer < 0 || enemy.HasLostTarget())
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        moveDir = DirectionToTarget();
        if (moveDir != 0)
        {
            enemy.FlipController(moveDir);
            TryMoveInDirection(moveDir, enemy.getBattleMoveSpeed(), rb.velocity.y);
        }
        else
        {
            StopBattleMovement();
        }

        if (ShouldRetreat())
        {
            BasicRetreat();
        }
    }

    protected void BasicRetreat()
    {
        int retreatDirection = -DirectionToTarget();
        float moveSpeed = enemy.retreatVelocity.x * enemy.activeSlowMultiplier;
        float yVelocity = enemy.retreatVelocity.y;

        if (TryMoveInDirection(retreatDirection, moveSpeed, yVelocity) == false)
            return;

        enemy.FlipController(moveDir);
    }

    protected bool CanAttack()
    {
        if (Time.time >= enemy.lastTimeAttack + enemy.attackCooldown)
        {
            enemy.lastTimeAttack = Time.time;
            return true;
        }

        return false;
    }

    protected float DistanceToTarget() => enemy.HorizontalDistanceToCurrentTarget();

    protected int DirectionToTarget() => enemy.DirectionToCurrentTarget();

    protected bool WithinAttackRange() => DistanceToTarget() < enemy.attackDistance;
    protected bool ShouldRetreat() => DistanceToTarget() < enemy.minRetreatDistance;

    protected bool TryMoveInDirection(int moveDirection, float moveSpeed, float verticalVelocity)
    {
        if (moveDirection == 0 ||
            enemy.IsGroundDetectedInDirection(moveDirection) == false ||
            enemy.IsWallDetectedInDirection(moveDirection))
        {
            StopBattleMovement();
            return false;
        }

        rb.velocity = new Vector2(moveSpeed * moveDirection, verticalVelocity);
        SetBattleAnimation(true);
        return true;
    }

    protected void StopBattleMovement()
    {
        enemy.ZeroVelocity();
        SetBattleAnimation(false);
    }

    protected void SetBattleAnimation(bool isMoving)
    {
        anim.SetBool("Move", isMoving);
        anim.SetBool("Idle", !isMoving);
    }

    protected void ClearBattleAnimation()
    {
        anim.SetBool("Move", false);
        anim.SetBool("Idle", false);
    }
}
