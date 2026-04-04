using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillObjectSword : SkillObjectBase
{
   
    protected SkillSwordThrow swordManager;

    protected Transform playerTransform;
    protected bool shouldComeback;

    [Header("Throw Limits")]
    [SerializeField] protected float maxAllowedDistance = 25;

    [Header("Recall")]
    [SerializeField] protected float minRecallSpeed = 14;
    [SerializeField] protected float maxRecallSpeed = 36;
    [SerializeField] protected float catchDistance = .85f;
    [SerializeField] protected float recallArcStrength = 1f;
    [SerializeField] protected float recallArcFrequency = 6f;
    [SerializeField] protected float recallTurnSpeed = 28f;
    [SerializeField] protected float maxRecallSpinSpeed = 1620f;
    [SerializeField] protected float spinFadeDistance = 2.6f;
    [SerializeField] protected float suctionDistance = 2.2f;
    [SerializeField] protected float suctionSpeedMultiplier = 2f;

    private float recallElapsedTime;
    private float recallArcDirection;
    private float recallSpinAngle;
    private float recallSpinDirection;

    protected virtual void Update()
    {
        if (rb == null)
            return;

        if (shouldComeback)
        {
            HandleComeback();
            return;
        }

        if (rb.velocity.sqrMagnitude > .01f)
            transform.right = rb.velocity;

        HandleComeback();
    }

    public virtual void SetUpSword(SkillSwordThrow swordManager, Vector2 dir)
    { 
        
        rb.simulated = true;
        rb.velocity = dir;

        this.swordManager = swordManager;
        shouldComeback = false;
        recallElapsedTime = 0;
        recallSpinAngle = 0;

        playerTransform = swordManager.transform.root;
        playeStats = swordManager.player.stats;
        damageScaleData = swordManager.damageScaleData;
        transform.parent = null;
    }

    public void GetSwordBack()
    {
        if (shouldComeback)
            return;

        shouldComeback = true;
        recallElapsedTime = 0;
        recallSpinAngle = 0;
        transform.parent = null;

        Vector2 toPlayer = playerTransform.position - transform.position;
        float turnDirection = Vector3.Cross(transform.right, toPlayer).z;
        recallArcDirection = Mathf.Sign(turnDirection);
        recallSpinDirection = recallArcDirection;

        if (Mathf.Approximately(recallArcDirection, 0))
        {
            recallArcDirection = Random.value > .5f ? 1 : -1;
            recallSpinDirection = recallArcDirection;
        }

        rb.simulated = false;
        rb.velocity = Vector2.zero;
    }

    protected void HandleComeback()
    {
        if (playerTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        recallElapsedTime += Time.deltaTime;
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance >= maxAllowedDistance)
            GetSwordBack();

        if (!shouldComeback)
            return;

        if (distance <= catchDistance)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 toPlayer = playerTransform.position - transform.position;
        Vector2 moveDirection = toPlayer.normalized;
        Vector2 perpendicular = new Vector2(-moveDirection.y, moveDirection.x) * recallArcDirection;

        float distancePercent = Mathf.Clamp01(distance / maxAllowedDistance);
        float currentRecallSpeed = Mathf.Lerp(minRecallSpeed, maxRecallSpeed, Mathf.Sqrt(distancePercent));
        float suctionBlend = 1f - Mathf.Clamp01((distance - catchDistance) / Mathf.Max(suctionDistance - catchDistance, .01f));
        float arcFade = Mathf.InverseLerp(catchDistance * 2f, maxAllowedDistance, distance);
        float arcOffset = Mathf.Sin(recallElapsedTime * recallArcFrequency) * recallArcStrength * arcFade;
        float spinBlend = Mathf.Clamp01((distance - catchDistance) / Mathf.Max(spinFadeDistance - catchDistance, .01f));

        currentRecallSpeed *= Mathf.Lerp(1f, suctionSpeedMultiplier, suctionBlend);
        arcOffset *= 1f - suctionBlend * .9f;

        Vector2 recallTarget = (Vector2)playerTransform.position + perpendicular * arcOffset;
        Vector2 nextPosition = Vector2.MoveTowards(transform.position, recallTarget, currentRecallSpeed * Time.deltaTime);
        Vector2 frameVelocity = (nextPosition - (Vector2)transform.position) / Mathf.Max(Time.deltaTime, .0001f);

        transform.position = nextPosition;

        if (frameVelocity.sqrMagnitude > .01f)
        {
            recallSpinAngle = Mathf.Repeat(recallSpinAngle + maxRecallSpinSpeed * spinBlend * recallSpinDirection * Time.deltaTime, 360f);

            float aimAngle = Mathf.Atan2(frameVelocity.y, frameVelocity.x) * Mathf.Rad2Deg;
            float targetAngle = aimAngle + recallSpinAngle * spinBlend * (1f - suctionBlend * .75f);
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, recallTurnSpeed * Time.deltaTime);
        }
    }



    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (shouldComeback)
            return;

        StopSword(collision);
        DamageEnemiesInRadius(transform, 1);
    }

    protected void StopSword(Collider2D collision)
    {
        rb.simulated = false;
        rb.velocity = Vector2.zero;
        transform.parent = collision.transform;
    }

    protected virtual void OnDestroy()
    {
        if (swordManager != null)
            swordManager.ClearActiveSword(this);
    }
}
