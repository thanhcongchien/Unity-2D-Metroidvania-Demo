using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillObjectHealth : EntityHealth
{
    // Modified: Time Echo has no Entity/EntityVFX, so it handles damage locally.
    public override bool takeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        if (isDead)
            return false;
        currentHp -= damage + elementalDamage;

        lastDamageTaken=damage+elementalDamage;
        if (currentHp <= 0)
            Die();

        return true;
    }

    protected override void Die()
    {
        // Modified: SkillObject handles its own death instead of Entity.EntityDeath().
        isDead = true;

        SkillObjectTimeEcho skillObjectTimeEcho = GetComponent<SkillObjectTimeEcho>();
        if (skillObjectTimeEcho != null)
            skillObjectTimeEcho.HandleDeath();
        else
            Destroy(gameObject);
    }
}
