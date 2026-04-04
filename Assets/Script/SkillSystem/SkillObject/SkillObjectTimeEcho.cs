using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SkillObjectTimeEcho : SkillObjectBase
{
    [SerializeField] private float wispMoveSpeed = 15;
    [SerializeField] private GameObject OnDeathVFX;
    [SerializeField] private LayerMask WhatIsGround;
    private bool shouldMoveToPlayer;


    private Transform playerTransform;
    private SkillTimeEcho echoManager;
    private bool deathHandled;
    private TrailRenderer wispTrail;
    private EntityHealth playerHealth;
    private SkillObjectHealth objectHealth;
    private PlayerSkillManager skillManager;
    private EntityStatusHandller statusHandler;

    public int maxAttacks { get; private  set;}

    public void SetUpEcho(SkillTimeEcho echoManager)
    {
        this.echoManager = echoManager;
        playeStats = echoManager.player.stats;
        damageScaleData = echoManager.damageScaleData;
        maxAttacks = echoManager.getMaxAttacks();
        playerTransform = echoManager.transform.root;
        playerHealth = echoManager.player.health;
        skillManager = echoManager.player.skillManager;
        statusHandler = echoManager.player.statusHandler;

        deathHandled = false;

        Invoke(nameof(HandleDeath), echoManager.getEchoDuration());
        FlipToTarget();
        objectHealth = GetComponent<SkillObjectHealth>();
        wispTrail = GetComponentInChildren<TrailRenderer>();
        wispTrail.gameObject.SetActive(false);

        anim.SetBool("CanAttack",maxAttacks > 0);   

    }
    private void Update()
    {
        if(shouldMoveToPlayer)
        {
            HandleWispMove();
        }
        else
        {
        anim.SetFloat("yVelocity", rb.velocity.y);
        StopHorizontalMove();

        }

    }
    private void HandleWispMove()
    {

        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, wispMoveSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, playerTransform.position) < .5f)
        {
            HandlePlayerTouch();
            Destroy(gameObject);
        }
    }
    private void HandlePlayerTouch()
    {
        float healAmount = objectHealth.lastDamageTaken * echoManager.getPercentOfDamageHealed();
        playerHealth.IncreaseHp(healAmount);

        float cooldownToReduce = echoManager.getCooldownReducedInSeconds();
        skillManager.ReduceAllSkillsCooldown(cooldownToReduce);

        if (echoManager.CanRemoveNegativeEffects())
        {
            statusHandler.RemoveAllNegativeEffects();
        }
    }
   private void FlipToTarget()
    {
        Transform target = FindClosetTarget();
        if (target !=null && target.position.x<transform.position.x)
            transform.Rotate(0,180,0);
    }


    public void PerformAttack()
    {
        Transform attackOrigin = targetCheck != null ? targetCheck : transform;
        DamageEnemiesInRadius(attackOrigin, checkRadius);

        if (targetGotHit == false)
            return;

        bool canClone = UnityEngine.Random.value < echoManager.getCloneChance();

        float xoffset = transform.position.x < lastTarget.position.x ? 1 : -1;
        if (canClone)
            echoManager.CreateEcho(lastTarget.position + new Vector3(xoffset, 0));

    }

   public void HandleDeath()
    {
        // Modified: avoid double death handling from timeout and damage in the same lifetime.
        if (deathHandled)
            return;

        deathHandled = true;
        CancelInvoke(nameof(HandleDeath));
        Instantiate(OnDeathVFX, transform.position, Quaternion.identity);

        if(echoManager.ShouldBeWisp())
        {
            ToIntoWisp();
        }
        else
            Destroy(gameObject);
    }

    private void ToIntoWisp()
    {
        shouldMoveToPlayer = true;
        anim.gameObject.SetActive(false);
        wispTrail.gameObject.SetActive(true);
        rb.simulated = false;
    }

    private void StopHorizontalMove()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.5f,WhatIsGround);

        if(hit.collider != null)
            rb.velocity = new Vector2(0, rb.velocity.y);
    }
}
