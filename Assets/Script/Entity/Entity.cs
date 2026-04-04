using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Animator anim { get; private set; }
    public Rigidbody2D rb { get; private set; }

    public Collider2D col { get; private set;}
    public EntitySFX sfx {  get; private set; }
   
    protected StateMachine stateMachine;
   
    [Header("Collision")]
    //public Transform attackCheck;
    //public float attackCheckRadius;
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckDistance;
     public LayerMask WhatIsGround;

    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected float wallCheckDistance;


    protected bool isKnocked;
    private Coroutine knockbackCo;
    private Coroutine slowDownCo;

    public int facingDir { get; private set; } = 1;
    protected bool facingRight = true;
    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sfx = GetComponent<EntitySFX>();
        col = GetComponent<Collider2D>();

        stateMachine = new StateMachine();
    }
    protected virtual void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
       
    }
    protected virtual void Update()
    {
        stateMachine.UpdateActiveState();
    }
    public void ReceiveKnockback(Vector2 knockback, float duration)
    {
        if (knockbackCo != null)
            StopCoroutine(knockbackCo);

        knockbackCo = StartCoroutine(KnockbackCo(knockback, duration));
    }

    private IEnumerator KnockbackCo(Vector2 knockback,float duration)
    {
        isKnocked = true;
        rb.velocity = knockback;
        yield return new WaitForSeconds(duration);
        rb.velocity = new Vector2(0, 0);
        isKnocked=false;
    }


    public void AnimationTrigger() => stateMachine.currentState.AnimationTrigger();



    public virtual bool isGroundDetected() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, WhatIsGround);
    public virtual bool IsGroundDetectedInDirection(int moveDirection)
    {
        if (groundCheck == null)
            return false;

        Vector3 checkPosition = groundCheck.position;
        if (moveDirection != 0 && Mathf.Sign(moveDirection) != Mathf.Sign(facingDir))
        {
            Vector3 localCheckPosition = transform.InverseTransformPoint(groundCheck.position);
            localCheckPosition.x *= -1f;
            checkPosition = transform.TransformPoint(localCheckPosition);
        }

        return Physics2D.Raycast(checkPosition, Vector2.down, groundCheckDistance, WhatIsGround);
    }
    public virtual bool IsWallDetectedInDirection(int moveDirection)
    {
        if (wallCheck == null)
            return false;

        Vector3 checkPosition = wallCheck.position;
        if (moveDirection != 0 && Mathf.Sign(moveDirection) != Mathf.Sign(facingDir))
        {
            Vector3 localCheckPosition = transform.InverseTransformPoint(wallCheck.position);
            localCheckPosition.x *= -1f;
            checkPosition = transform.TransformPoint(localCheckPosition);
        }

        return Physics2D.Raycast(checkPosition, Vector2.right * moveDirection, wallCheckDistance, WhatIsGround);
    }
    public virtual bool isWallDetected() => IsWallDetectedInDirection(facingDir);
    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));
       
    }
    public virtual void Flip()
    {
        facingDir *= -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }
    public virtual void FlipController(float _x)
    {
        if (_x > 0 && !facingRight)
        {
            Flip();
        }
        else if (_x < 0 && facingRight)
            Flip();
    }
    public  void ZeroVelocity()
    {
        if (isKnocked)
            return;

        rb.velocity = new Vector2(0, 0);
    }
    public void SetVelocity(float _xVelocity, float _yVelocity)
    {
        if (isKnocked)
            return;

        rb.velocity = new Vector2(_xVelocity, _yVelocity);
        FlipController(_xVelocity);
    }
   
    public virtual void EntityDeath()
    {



    }

    public virtual void SlowDownEntity(float duration,float slowMultiplier,bool canOverride = false)
    {
        if (slowDownCo != null)
        {
            if (canOverride)
                StopCoroutine(slowDownCo);
            else
                return;
        }
        slowDownCo=StartCoroutine(SlowDownEntityCo(duration, slowMultiplier));
    }

    protected virtual IEnumerator SlowDownEntityCo(float duration, float slowMultiplier)
    {
        yield return null;
    }
    public virtual void StopSlowdown()
    {
        slowDownCo = null;
    }

}
