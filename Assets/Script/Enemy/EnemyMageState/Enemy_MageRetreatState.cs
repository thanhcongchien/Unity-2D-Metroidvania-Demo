using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_MageRetreatState : EnemyState
{
    private EnemyMage enemyMage;
    private Vector3 startPosition;


    public Enemy_MageRetreatState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyMage = enemy as EnemyMage;
    }
    public override void Enter()
    {
        base.Enter();

        enemy.TryFindTarget();

        int targetDirection = enemy.DirectionToCurrentTarget();
        if (targetDirection == 0)
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        startPosition = enemy.transform.position;

        int retreatDirection = -targetDirection;
        if (enemy.IsGroundDetectedInDirection(retreatDirection) == false)
        {
            stateMachine.ChangeState(enemyMage.mageBattleState);
            return;
        }

        enemy.FlipController(targetDirection);
        rb.velocity = new Vector2(enemyMage.retreatSpeed * enemy.activeSlowMultiplier * retreatDirection, enemy.retreatVelocity.y);

        enemy.gameObject.layer = LayerMask.NameToLayer("Untargetable");

        enemy.vfx.DoImageEchoEffect(1f);

    }
    public override void Update()
    {
        base.Update();

        if (enemy.HasValidTarget() == false && enemy.TryFindTarget() == false)
        {
            stateMachine.ChangeState(enemy.idleState);
            return;
        }

        int targetDirection = enemy.DirectionToCurrentTarget();
        if (targetDirection == 0)
        {
            stateMachine.ChangeState(enemyMage.mageBattleState);
            return;
        }

        int retreatDirection = -targetDirection;
        bool reachedMaxDistance = Vector2.Distance(startPosition, enemy.transform.position) > enemyMage.retreatMaxDistance;
        bool canKeepRetreating = enemy.IsGroundDetectedInDirection(retreatDirection);

        if (reachedMaxDistance || canKeepRetreating == false)
        {
            stateMachine.ChangeState(enemyMage.spellCastState);
           
            return;
        }

        enemy.FlipController(targetDirection);
        rb.velocity = new Vector2(enemyMage.retreatSpeed * enemy.activeSlowMultiplier * retreatDirection, rb.velocity.y);
    }

    public override void Exit()
    {
        base.Exit();
        enemy.vfx.StopImageEchoEffect();
       
        enemy.gameObject.layer = LayerMask.NameToLayer("Enemy");
    }
}
