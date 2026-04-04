using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_ArcherElf : Enemy
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_ArcherElfBattleState archerBattleState { get; private set;}

    [Header("Arrow details")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private float arrowSpeed = 8f;

    protected override void Awake()
    {
        base.Awake();

        idleState = new EnemyIdleState(this, stateMachine, "Idle");
        moveState = new EnemyMoveState(this, stateMachine, "Move");
        archerBattleState = new Enemy_ArcherElfBattleState(this, stateMachine, "Move");
        attackState = new EnemyAttackState(this, stateMachine, "Attack");
        stunnedState = new EnemyStunnedState(this, stateMachine, "Stun");
        deadState = new EnemyDeadState(this, stateMachine, "Idle"); ;

        battleState = archerBattleState;
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }
    public override void SpecialAttack()
    {
       GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        arrow.GetComponent<Enemy_ArcherElfArrow>().SetupArrow(arrowSpeed * facingDir, combat);



    }


    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }
}
