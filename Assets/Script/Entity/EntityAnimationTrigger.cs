using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityAnimationTrigger : MonoBehaviour
{
    private Entity entity;
    private EntityCombat entityCombat;


    protected virtual void Awake()
    {
        entity=GetComponentInParent<Entity>();
        entityCombat=GetComponentInParent<EntityCombat>();
    }
    protected void AnimationTrigger()
    {
        entity.AnimationTrigger();
    }
    protected void AttackTrigger()
    {
        entityCombat.PerformAttack();
    }
}
