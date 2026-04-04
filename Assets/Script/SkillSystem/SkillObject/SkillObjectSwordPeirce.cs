using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillObjectSwordPeirce : SkillObjectSword
{
    private int amountToPeierce;

    public override void SetUpSword(SkillSwordThrow swordManager, Vector2 dir)
    {
        base.SetUpSword(swordManager, dir);
        amountToPeierce = swordManager.peirceCount;
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        bool groundHit = collision.gameObject.layer == LayerMask.NameToLayer("Ground");


        if (amountToPeierce <= 0 || groundHit)
        {
            DamageEnemiesInRadius(transform, .3f);
            StopSword(collision);
            return;
        }

        amountToPeierce--;
        DamageEnemiesInRadius(transform, .3f);
    }
}
