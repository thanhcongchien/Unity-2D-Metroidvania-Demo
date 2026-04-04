using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillObjectBase : MonoBehaviour
{
    [SerializeField] private GameObject onHitVfx;
    [Space]
    [SerializeField] protected LayerMask whatisEnemy;
    [SerializeField] protected Transform targetCheck;
    [SerializeField] protected float checkRadius = 1f;

    protected Animator anim;
    protected EntityStats playeStats;
    protected DamageScaleData damageScaleData;
    protected ElementType usedElement;
    protected bool targetGotHit;
    protected Rigidbody2D rb;
    protected Transform lastTarget;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected Collider2D[] getEnemiesAround(Transform t,float radius)
    {

        return Physics2D.OverlapCircleAll(t.position, radius, whatisEnemy);
    }
    protected void DamageEnemiesInRadius(Transform t,float radius)
    {
        foreach(var target in getEnemiesAround(t,radius))
        {
            IDamagable damagable=target.GetComponent<IDamagable>();

            if(damagable==null) continue;

            AttackData attackData = playeStats.getAttackData(damageScaleData);

          EntityStatusHandller entityStatusHandller=target.GetComponent<EntityStatusHandller>();

            float physicalDamage = attackData.phyiscalDamage;

            float elementalDamage = attackData.elementalDamage;

            ElementType element = attackData.element;
            targetGotHit =  damagable.takeDamage(physicalDamage, elementalDamage, element, transform); 

            if(element!=ElementType.None)
                entityStatusHandller.ApplyStatusEffect(element,attackData.effectData);

            if (targetGotHit)
            {
                lastTarget = target.transform;
                Instantiate(onHitVfx, target.transform.position, Quaternion.identity);
            }
            usedElement=element;
        }

    }
    protected Transform FindClosetTarget()
    {
        Transform target = null;
        float closestDistance = Mathf.Infinity;

        foreach(var enemy in getEnemiesAround(transform,8))
        {
            float distance=Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < closestDistance)
            {
                target=enemy.transform;
                closestDistance = distance;
            }
        }

        return target;
    }

    protected virtual void OnDrawGizmos()
    {
        if(targetCheck==null)
            targetCheck = transform;





        Gizmos.DrawWireSphere(targetCheck.position, checkRadius);
    }
}
