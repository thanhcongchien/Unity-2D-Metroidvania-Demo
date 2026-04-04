using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item Effect/ Heal On Doing Damage", fileName = "Item effect data - Heal On Doing Damage")]
public class ItemEffect_HealOnDoingDamage : ItemEffect_DataSO
{
    [SerializeField] private float percentHealedOnAttack = .2f;

    public override void ExecuteEffect()
    {
        base.ExecuteEffect();
    }

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        player.combat.OnDoingPhysicalDamage += HealOnDoingDamage;
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        player.combat.OnDoingPhysicalDamage -= HealOnDoingDamage;
        player = null;
    }


    private void HealOnDoingDamage(float damage)
    {
        player.health.IncreaseHp(damage * percentHealedOnAttack);

    }


}
