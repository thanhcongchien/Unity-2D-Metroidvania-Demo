using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item Effect/ IceBlast", fileName = "Item effect data - IceBlast")]
public class ItemEffect_IceBlastOnTakingDamage : ItemEffect_DataSO
{
    [SerializeField] private ElementalEffectData effectData;
    [SerializeField] private float iceDamage;
    [SerializeField] private LayerMask whatIsEnemy;

    [Space]
    [SerializeField] private float hpPercentTrigger = .25f;
    [SerializeField] private float cooldown;
    private float lastTimeUsed = -999;

    [SerializeField] private GameObject IceBlastVFX;
    [SerializeField] private GameObject onHitVFX;

    public override void ExecuteEffect()
    {
        if (player == null || player.health == null)
            return;

        bool noCooldown = Time.time - lastTimeUsed > cooldown;
        if(!noCooldown)
            Debug.Log("IceBlast: Cooldown");
        bool reachedTrigger = player.health.getHealthPercent() <= hpPercentTrigger;

        if(noCooldown && reachedTrigger)
        {
            if (IceBlastVFX != null)
                player.playerVFX.CreateEffectOf(IceBlastVFX, player.transform);

            lastTimeUsed = Time.time;
            damageEnemiesWithIce();
        }
    }
    private void damageEnemiesWithIce()
    {
        Collider2D[] enemies= Physics2D.OverlapCircleAll(player.transform.position, 1.5f, whatIsEnemy);

        foreach(var target in enemies)
        {
            IDamagable idamagable = target.GetComponent<IDamagable>();
            // 关键修复：敌人碰撞体可能在子物体，伤害组件在父物体。
            if (idamagable == null)
                idamagable = target.GetComponentInParent<IDamagable>();
            if(idamagable==null)
                continue;

            bool targetGotHit = idamagable.takeDamage(0,iceDamage,ElementType.Ice,player.transform);
            EntityStatusHandller entityStatusHandller = target.GetComponent<EntityStatusHandller>();
            if (entityStatusHandller == null)
                entityStatusHandller = target.GetComponentInParent<EntityStatusHandller>();
            if (effectData != null && entityStatusHandller != null)
                entityStatusHandller.ApplyStatusEffect(ElementType.Ice,effectData);
            
            if(targetGotHit && onHitVFX != null)
            {
                player.playerVFX.CreateEffectOf(onHitVFX,target.transform);
            }
        }
    }


    public override void Subscribe(Player player)
    {
        base.Subscribe(player);

        if (player == null || player.health == null)
            return;
        //lastTimeUsed = -999f;
        // 关键修复：先取消再订阅，避免重复订阅导致行为异常。
        player.health.OnTakingDamage -= ExecuteEffect;
        player.health.OnTakingDamage += ExecuteEffect;
        
    }
    public override void Unsubscribe()
    {
        if (player != null && player.health != null)
        {
            player.health.OnTakingDamage -= ExecuteEffect;
        }

        base.Unsubscribe();
        player = null;
    }
}
