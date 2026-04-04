using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EntityCombat : MonoBehaviour
{
    public event Action<float> OnDoingPhysicalDamage;
    
    private EntityVFX vfx;
    private EntityStats stats;
    private EntitySFX sfx;

    public DamageScaleData basicAttackScale;

    [Header("Target Detection")]
    [SerializeField] private Transform targetCheck;
    [SerializeField] private float targetRadius;
    [SerializeField] private LayerMask WhatIsTarget;

    

    private void Awake()
    {
        vfx = GetComponent<EntityVFX>();
        stats = GetComponent<EntityStats>();
        sfx = GetComponent<EntitySFX>();
    }

    public void PerformAttack()
    {
        bool targetGotHit = false;

        getDetectedColliders(WhatIsTarget);
        foreach(var target in getDetectedColliders(WhatIsTarget))
        {
           
            IDamagable damagable=target.GetComponent<IDamagable>();

            if (damagable == null)
            {
                continue;
            }

            AttackData attackData = stats.getAttackData(basicAttackScale);
            EntityStatusHandller entityStatusHandller=target.GetComponent<EntityStatusHandller>();
         
            float physicalDamage = attackData.phyiscalDamage;

            float elementalDamage = attackData.elementalDamage;

            ElementType element = attackData.element;

         targetGotHit=  damagable.takeDamage(physicalDamage,elementalDamage,element,transform);

            if(element!=ElementType.None)
                entityStatusHandller?.ApplyStatusEffect(element,attackData.effectData);

            
            if (targetGotHit)
            {
                OnDoingPhysicalDamage?.Invoke(physicalDamage);
              vfx.PlayHitVFX(target.transform,attackData.isCrit,element);
                sfx?.PlayAttackHit();
            }



        }


        if (!targetGotHit)
            sfx?.PlayAttackMiss();
    }

    public void PerformAttackOnTarget(Transform target,DamageScaleData damageScaleData = null)
    {
        bool targetGotHit = false;

        getDetectedColliders(WhatIsTarget);
        

            IDamagable damagable = target.GetComponent<IDamagable>();

            if (damagable == null)
            {
            return;
            }

        DamageScaleData damageScale = damageScaleData == null ? basicAttackScale : damageScaleData;

            AttackData attackData = stats.getAttackData(damageScale);
            EntityStatusHandller entityStatusHandller = target.GetComponent<EntityStatusHandller>();

            float physicalDamage = attackData.phyiscalDamage;

            float elementalDamage = attackData.elementalDamage;

            ElementType element = attackData.element;

            targetGotHit = damagable.takeDamage(physicalDamage, elementalDamage, element, transform);

            if (element != ElementType.None)
                entityStatusHandller?.ApplyStatusEffect(element, attackData.effectData);


            if (targetGotHit)
            {
                OnDoingPhysicalDamage?.Invoke(physicalDamage);
                vfx.PlayHitVFX(target.transform, attackData.isCrit, element);
                sfx?.PlayAttackHit();
            }



       


        if (!targetGotHit)
            sfx?.PlayAttackMiss();
    }

    public void PerformCustomAttackOnTarget(Transform target, float physicalDamage, float elementalDamage,
        ElementType element, ElementalEffectData effectData, bool isCrit = false)
    {
        bool targetGotHit = false;

        IDamagable damagable = target != null ? target.GetComponent<IDamagable>() : null;
        if (damagable == null)
            return;

        EntityStatusHandller entityStatusHandller = target.GetComponent<EntityStatusHandller>();

        targetGotHit = damagable.takeDamage(physicalDamage, elementalDamage, element, transform);

        if (element != ElementType.None && effectData != null)
            entityStatusHandller?.ApplyStatusEffect(element, effectData);

        if (targetGotHit)
        {
            OnDoingPhysicalDamage?.Invoke(physicalDamage);
            vfx.PlayHitVFX(target.transform, isCrit, element);
            sfx?.PlayAttackHit();
        }
        else
        {
            sfx?.PlayAttackMiss();
        }
    }


    //public void ApplyStatusEffect(Transform target,ElementType element,float scaleFactor=1)
    //{
    //    EntityStatusHandller StatusHandler=target.GetComponent<EntityStatusHandller>();
    //    if (StatusHandler == null)
    //        return;
    //    if(element==ElementType.Ice&&StatusHandler.CanbeApplied(ElementType.Ice))
    //        StatusHandler.ApplyChillEffect(defaultDuration, chillslowMutiplier);
    //    if (element == ElementType.Fire && StatusHandler.CanbeApplied(ElementType.Fire))
    //    { 
    //        scaleFactor=fireScale;
    //        float fireDamage=stats.offense.fireDamage.GetValue()*scaleFactor;

    //        StatusHandler.ApplyBurnEffect(defaultDuration,fireDamage);
    //    }
    //    if(element==ElementType.Lightning&&StatusHandler.CanbeApplied(ElementType.Lightning))
    //    {
    //        scaleFactor=lightningScale;
    //        float lightningDamage=stats.offense.lightningDamage.GetValue()*scaleFactor;
    //        StatusHandler.ApplyLightningEffect(defaultDuration, lightningDamage, electrifyChargeBuildUP); ;

    //    }
    //}



    protected Collider2D[] getDetectedColliders(LayerMask whatToDetect)
    {
       return Physics2D.OverlapCircleAll(targetCheck.position, targetRadius, whatToDetect);
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(targetCheck.position, targetRadius);
    }
}
