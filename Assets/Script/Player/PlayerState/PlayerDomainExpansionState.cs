using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDomainExpansionState : PlayerState
{
    private const float RiseStopVelocityThreshold = 0.05f;
    private const float CeilingClearance = 0.05f;

    private Vector2 originalPosition;
    private float orginalGravity;
    private float finalRiseDistance;

    private bool isLeviating;
    private bool createdDomain;


    public PlayerDomainExpansionState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }
    public override void Enter()
    {
        base.Enter();
        player.health.SetCanTakeDamage(false);
        originalPosition = player.transform.position;
        orginalGravity = rb.gravityScale;
        isLeviating = false;
        finalRiseDistance = getAvaliableRiseDistance();

        if (finalRiseDistance <= 0f)
        {
            Leviate();
            return;
        }

        player.SetVelocity(0, player.riseSpeed);
    }
    public override void Update()
    {
        base.Update();

        if (isLeviating == false && ShouldStartLeviating())
            Leviate();
        if(isLeviating)
        {
            skillManager.domainExpansion.DoSpellCasting();

            if (stateTimer <= 0)
            {
                isLeviating = false;
                rb.gravityScale = orginalGravity;
                stateMachine.ChangeState(player.idleState);
            }
        }
    }
    public override void Exit()
    {
        base.Exit();
        player.health.SetCanTakeDamage(true);
        rb.gravityScale = orginalGravity;
        isLeviating = false;
        createdDomain = false;
    }
    private void Leviate()
    {
        isLeviating = true;
        player.SetVelocity(0, 0);
        rb.gravityScale = 0;
        stateTimer = skillManager.domainExpansion.getDuration();
        if (createdDomain == false)
        {
            createdDomain = true;
            skillManager.domainExpansion.CreateDomain();
        }
    }
    private bool ShouldStartLeviating()
    {
        if (Vector2.Distance(originalPosition, player.transform.position) >= finalRiseDistance)
            return true;

        // If the player stops moving upward before reaching the target height,
        // a ceiling collision or other obstruction has interrupted the rise.
        return rb.velocity.y <= RiseStopVelocityThreshold;
    }
    private float getAvaliableRiseDistance()
    { 
        Bounds bounds = player.col.bounds;
        float castDistance = player.riseMaxDistance + bounds.extents.y + CeilingClearance;
        RaycastHit2D hit = Physics2D.Raycast(bounds.center, Vector2.up, castDistance, player.WhatIsGround);

        if (hit.collider == null)
            return player.riseMaxDistance;

        float availableRiseDistance = hit.distance - bounds.extents.y - CeilingClearance;
        return Mathf.Clamp(availableRiseDistance, 0f, player.riseMaxDistance);

    }
}
