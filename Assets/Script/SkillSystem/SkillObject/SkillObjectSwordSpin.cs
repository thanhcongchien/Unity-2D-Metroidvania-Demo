using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillObjectSwordSpin : SkillObjectSword
{
    private int maxDistance;
    private float attacksPerSecond;
    private float attackTimer;

    protected override void Update()
    {
        HandleAttack();
        HandleStopping();
        HandleComeback();
    }
    public override void SetUpSword(SkillSwordThrow swordManager, Vector2 dir)
    {
        base.SetUpSword(swordManager, dir);

        anim?.SetTrigger("Spin");

        maxDistance = swordManager.maxDistance;
        attacksPerSecond = swordManager.attacksPerSecond;

        Invoke(nameof(GetSwordBack), swordManager.maxSpinDuration);
    }
    private void HandleStopping()
    {
        float distanceToPLayer = Vector2.Distance(transform.position, playerTransform.position);

        if(distanceToPLayer>maxDistance&&rb.simulated==true)
        {
            rb.simulated = false;
        }
    }

    private void HandleAttack()
    {
        attackTimer -= Time.deltaTime;
        if(attackTimer<=0)
        {
            DamageEnemiesInRadius(transform, 1);
            attackTimer = 1/attacksPerSecond;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        rb.simulated=false;
    }
}
