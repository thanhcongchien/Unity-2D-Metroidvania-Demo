using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_ReaperAnimationTrigger : EnemyAnimationTrigger
{
    private Enemy_Reaper enemyReaper;


    protected override void Awake()
    {
        base.Awake();
        enemyReaper = GetComponentInParent<Enemy_Reaper>();
    }

    private void AnimationTrigger()
    {
        base.AnimationTrigger();
    }

    private void AttackTrigger()
    {
        base.AttackTrigger();
    }

    private void SpecialAttackTrigger()
    {
        base.SpecialAttackTrigger();
    }

    private void EnableCounterWindow()
    {
        base.EnableCounterWindow();
    }

    private void DisableCounterWindow()
    {
        base.DisableCounterWindow();
    }

    private void TeleportTrigger()
    {
        enemyReaper.SetTeleportTrigger(true);

    }

}
