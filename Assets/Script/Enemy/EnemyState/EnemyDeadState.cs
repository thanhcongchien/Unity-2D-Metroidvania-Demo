using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeadState : EnemyState
{
    public EnemyDeadState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        anim.enabled = false;
        rb.gravityScale = 12f;
        rb.velocity = new Vector2(rb.velocity.x, 15);
        enemy.GetComponent<Collider2D>().enabled = false;
        stateMachine.SwitchOffStateMachine();
        enemy.DestroyGameObjectWithDelay();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

    }
}
