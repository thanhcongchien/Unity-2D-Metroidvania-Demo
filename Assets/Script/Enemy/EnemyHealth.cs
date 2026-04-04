using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : EntityHealth
{
    private Enemy enemy;
    private EnemyState enemyState;
    private Player_QuestManager questManager;
    protected override void Start()
    {
        enemy = GetComponent<Enemy>();

        questManager = Player.instance.questManager;
    }


    public override bool takeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        
        if(canTakeDamage==false)
            return false;

        bool wasHit  =   base.takeDamage(damage,elementalDamage,element, damageDealer);

        if (wasHit==false)
            return false;


        if (damageDealer.CompareTag("Player"))
        {
          
          
            enemy.TryEnterBattleState(damageDealer);

            
        }
        return true;
    }

    protected override void Die()
    {
        base.Die();


        questManager.AddProgress(enemy.questTargetId);
    }
}

