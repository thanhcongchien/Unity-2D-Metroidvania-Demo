using UnityEngine;

public class Enemy_Slime : Enemy, ICounterable
{
    public bool CanBeCountered { get => canBeStunned; }

    public Enemy_SlimeDeadState slimeDeadState { get; private set;} 

    [Header("Slime specifics")]
    [SerializeField] private GameObject childSlimePrefab;
    [SerializeField] private int amountToSpawn = 2;
    [SerializeField] private Vector2 newSlimeVelocity;


    [SerializeField] private bool hasRecoveryAnimation = true;
    private bool hasSplitOnDeath;


    protected override void Awake()
    {
        base.Awake();

        idleState = new EnemyIdleState(this, stateMachine, "Idle");
        moveState = new EnemyMoveState(this, stateMachine, "Move");
        battleState = new EnemyBattleState(this, stateMachine, "Move");
        attackState = new EnemyAttackState(this, stateMachine, "Attack");
        stunnedState = new EnemyStunnedState(this, stateMachine, "Stun");
        slimeDeadState = new Enemy_SlimeDeadState(this, stateMachine, "Idle");
        deadState = slimeDeadState;

        anim.SetBool("hasStunRecovery", hasRecoveryAnimation);

    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public override void EntityDeath()
    {
        base.EntityDeath();
    }

    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }

    public void CreateSlimeOnDeath()
    {
        if (hasSplitOnDeath || childSlimePrefab == null || amountToSpawn <= 0)
            return;

        hasSplitOnDeath = true;

        for (int i = 0; i < amountToSpawn; i++)
        {
            GameObject newSlime = Instantiate(childSlimePrefab, transform.position, Quaternion.identity);
            Enemy_Slime slimeScript = newSlime.GetComponent<Enemy_Slime>();
            if (slimeScript == null)
                continue;

            slimeScript.stats.AdjustStatSetup(stats.resources, stats.offense, stats.defense, .6f, 1.2f);
            slimeScript.ApplyRespawnVelocity();
            if (player != null)
                slimeScript.StartBattleStateCheck(player);
        }
    }

    public void ApplyRespawnVelocity()
    {
        Vector2 baseVelocity = newSlimeVelocity;
        if (baseVelocity == Vector2.zero)
            baseVelocity = stunnedDir;

        if (baseVelocity == Vector2.zero)
            baseVelocity = new Vector2(2f, 4f);

        float xVelocity = Mathf.Abs(baseVelocity.x) * (Random.value < .5f ? -1f : 1f) * Random.Range(.8f, 1.2f);
        float yVelocity = Mathf.Abs(baseVelocity.y) * Random.Range(.8f, 1.2f);

        SetVelocity(xVelocity, yVelocity);
    }

    public void StartBattleStateCheck(Transform player)
    {
        if (player == null)
            return;

        TryEnterBattleState(player);
        InvokeRepeating(nameof(ReEnterBattleState), 0, .3f);
    }
    private void ReEnterBattleState()
    {
        if (HasValidTarget() == false)
        {
            CancelInvoke(nameof(ReEnterBattleState));
            return;
        }

        if (stateMachine.currentState == battleState || stateMachine.currentState == attackState)
        {
            CancelInvoke(nameof(ReEnterBattleState));
            return;
        }

        stateMachine.ChangeState(battleState);
    }
    public void SetupSlime(Vector2 velocity, EntityStats newStats)
    {
        float xVelocity = velocity.x * Random.Range(-2, 2);
        float yVelocity = velocity.y * Random.Range(1, 2);

        rb.velocity = new Vector2(xVelocity, yVelocity);

        stats.AdjustStatSetup(stats.resources, stats.offense, stats.defense, .6f, 1.2f);

    }

}
