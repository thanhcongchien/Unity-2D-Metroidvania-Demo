using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationTrigger : EntityAnimationTrigger
{
    private Enemy enemy;
    private EnemyVFX enemyVFX;
    protected override void Awake()
    {
        base.Awake();
        enemy = GetComponentInParent<Enemy>();
        enemyVFX = GetComponentInParent<EnemyVFX>();
    }

    protected void SpecialAttackTrigger()
    {
        enemy.SpecialAttack();
    }
    protected void EnableCounterWindow()
    {
        enemyVFX.EnableAttackAlert(true);
        enemy.EnableCounterWindow(true);

    }

    protected void DisableCounterWindow()
    {
        enemyVFX.EnableAttackAlert(false);
        enemy.EnableCounterWindow(false);
    } 

    //private void OpenCounterAttackWindow() => enemy.OpenCounterAttackWindow();
    //private void CloseCounterAttackWindow() => enemy.CloseCounterAttackWindow();



}
