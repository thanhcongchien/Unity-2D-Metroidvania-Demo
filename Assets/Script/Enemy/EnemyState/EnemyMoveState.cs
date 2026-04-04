using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveState : EnemyGroundedState
{
    private float lastXPosition;
    private float stuckTimer;
    private const float StuckFlipDelay = .2f;

    public EnemyMoveState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        lastXPosition = enemy.transform.position.x;
        stuckTimer = 0f;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (stateMachine.currentState != this)
            return;

        int moveDirection = enemy.facingDir;
        bool blockedAhead = enemy.IsWallDetectedInDirection(moveDirection) || enemy.IsGroundDetectedInDirection(moveDirection) == false;

        if (blockedAhead)
        {
            enemy.ZeroVelocity();
            enemy.Flip();
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        enemy.SetVelocity(enemy.getMoveSpeed() * moveDirection, rb.velocity.y);

        float movedDistance = Mathf.Abs(enemy.transform.position.x - lastXPosition);
        if (movedDistance < .005f)
            stuckTimer += Time.deltaTime;
        else
            stuckTimer = 0f;

        lastXPosition = enemy.transform.position.x;

        if (stuckTimer >= StuckFlipDelay)
        {
            enemy.ZeroVelocity();
            enemy.Flip();
            stateMachine.ChangeState(enemy.idleState);
        }
    }
}
