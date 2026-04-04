using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class Enemy : Entity
{
    [Header("Quest Info")]
    public string questTargetId;

    public EntityCombat combat { get; private set;}
    public EnemyHealth health {  get; private set;}
    public EntityStats stats { get; private set; }

    public EntityVFX vfx { get; private set; }
    private const float CloseRangeDetectionRadius = 2f;
    private const float LoseTargetDistance = 15f;

    public EnemyIdleState idleState;
    public EnemyMoveState moveState;
    public EnemyAttackState attackState;
    public EnemyBattleState battleState;
    public EnemyStunnedState stunnedState;
    public EnemyDeadState deadState;



    public Transform player {  get; private set; }
    public LayerMask WhatIsPlayer;
    [Header("Stunned")]
    public float stunnedDuration;
    public Vector2 stunnedDir;
    public bool canBeStunned;
    [SerializeField] protected GameObject counterImage;

    [Header("Move")]
    public float moveSpeed;
    public float idleTime;
    public float battleTime;
  
    [Header("Attack")]
    public float attackDistance;
    public float attackCooldown;
    public float battleMoveSpeed;
    [HideInInspector] public float lastTimeAttack;
    public float minRetreatDistance;
    public Vector2 retreatVelocity;
    public bool canChase = true;

    public float activeSlowMultiplier { get; private set; } = 1f;

    protected override void Awake()
    {
        base.Awake();
        health = GetComponent<EnemyHealth>();
        stateMachine = new StateMachine();
        stats = GetComponent<EntityStats>();
        combat = GetComponent<EntityCombat>();
        vfx = GetComponent<EntityVFX>();
    }
    protected override void Update()
    {
        base.Update();
    }

    public virtual void SpecialAttack()
    {

    }


    public virtual RaycastHit2D IsPlayerDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * facingDir,20, WhatIsPlayer);

    // Modified: enemy now owns target acquisition instead of reading PlayerManager.
    public bool TryFindTarget()
    {
        if (CanAcquireCombatTarget() == false)
            return false;

        Transform detectedTarget = GetDetectedTarget();
        if (detectedTarget == null)
            return false;

        SetTarget(detectedTarget);
        return true;
    }

    public bool HasValidTarget() => IsTargetValid(player);

    public bool CanSenseCurrentTarget()
    {
        if (HasValidTarget() == false)
            return false;

        RaycastHit2D detectedTarget = IsPlayerDetected();
        if (detectedTarget.collider != null && detectedTarget.transform == player)
            return true;

        return Vector2.Distance(transform.position, player.position) <= CloseRangeDetectionRadius;
    }

    public float DistanceToCurrentTarget()
    {
        if (HasValidTarget() == false)
            return float.MaxValue;

        return Vector2.Distance(transform.position, player.position);
    }

    public float HorizontalDistanceToCurrentTarget()
    {
        if (HasValidTarget() == false)
            return float.MaxValue;

        return Mathf.Abs(player.position.x - transform.position.x);
    }

    public int DirectionToCurrentTarget()
    {
        if (HasValidTarget() == false)
            return 0;

        return player.position.x > transform.position.x ? 1 : -1;
    }

    public bool HasLostTarget() => CanSenseCurrentTarget() == false && DistanceToCurrentTarget() > LoseTargetDistance;

    public Transform GetDetectedTarget()
    {
        if (CanSenseCurrentTarget())
            return player;

        RaycastHit2D detectedTarget = IsPlayerDetected();
        if (detectedTarget.collider != null && IsTargetValid(detectedTarget.transform))
            return detectedTarget.transform;

        return GetNearestCloseTarget();
    }

    protected virtual bool CanAcquireCombatTarget() => true;

    private Transform GetNearestCloseTarget()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, CloseRangeDetectionRadius, WhatIsPlayer);
        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D target in targets)
        {
            if (IsTargetValid(target.transform) == false)
                continue;

            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target.transform;
            }
        }

        return closestTarget;
    }

    private bool IsTargetValid(Transform target)
    {
        if (target == null)
            return false;

        EntityHealth targetHealth = target.GetComponent<EntityHealth>();
        return targetHealth == null || targetHealth.isDead == false;
    }

    private void SetTarget(Transform target)
    {
        if (target == null)
            return;

        player = target;

        if (target.position.x > transform.position.x && !facingRight)
            Flip();
        else if (target.position.x < transform.position.x && facingRight)
            Flip();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + attackDistance * facingDir, transform.position.y));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + minRetreatDistance * facingDir, transform.position.y));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, CloseRangeDetectionRadius);
    }
    public virtual void AnimationFinishTrigger()=>stateMachine.currentState.AnimationTrigger();


    public void EnableCounterWindow(bool enable)=>canBeStunned=enable;


    public float getMoveSpeed() => moveSpeed * activeSlowMultiplier;
    public float getBattleMoveSpeed()
    {
        if (canChase)
            return battleMoveSpeed * activeSlowMultiplier;
        else
            return 0.0001f;
    }
    public override void SlowDownEntity(float duration, float slowMultiplier, bool canOverride = false)
    {
        base.SlowDownEntity(duration, slowMultiplier, canOverride);
    }

    protected override IEnumerator SlowDownEntityCo(float duration, float slowMultiplier)
    {
      
        
        activeSlowMultiplier = 1-slowMultiplier;

       
        anim.speed *= activeSlowMultiplier;
        
        yield return new WaitForSeconds(duration);
        StopSlowdown();
       
    }
    public override void StopSlowdown()
    {
        activeSlowMultiplier = 1;
        anim.speed = 1;

        base.StopSlowdown();
    }

    public virtual bool CanBeStunned()
    {

        if(canBeStunned)
        {
            //CloseCounterAttackWindow();
            return true;
        }
        return false;
    }
    public void TryEnterBattleState(Transform player)
    {
        // Modified: damage dealers can retarget enemies even while already in combat.
        SetTarget(player);

        if (stateMachine.currentState == battleState||stateMachine.currentState==attackState)
            return;

        stateMachine.ChangeState(battleState);
    }
    public override void EntityDeath()
    {
        base.EntityDeath();
        stateMachine.ChangeState(deadState);
    }

    public void StopCombat()
    {
        
        player = null;

        
        if (stateMachine != null)
        {
            stateMachine.ChangeState(idleState);
        }

        // 3. 停止所有移动，防止惯性导致的抽搐
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        
    }

    public void MakeUntargetable(bool untargetable)
    {
        gameObject.layer = LayerMask.NameToLayer(untargetable ? "Untargetable" : "Enemy");
    }

    public void DestroyGameObjectWithDelay(float delay = 10)
    {
        Destroy(gameObject, delay);
    }


    private void HandlePlayerDeath()
    {
        StopCombat();

    }
    private void OnEnable()
    {
        Player.OnPlayerDeath += HandlePlayerDeath;
    }
    private void OnDisable()
    {
        Player.OnPlayerDeath -= HandlePlayerDeath;
    }
}

