using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ElementalEffectData 
{
   public float chillDuration;
    public float chillSlowEffect;

    public float burnDuration;
    public float totalBurnDamage;

    public float lightningDuration;
    public float lightningDamage;
    public float lightningCharge;

    public ElementalEffectData()
    {
    }

    public ElementalEffectData(EntityStats entityStats,DamageScaleData damageScale)
    {
        chillDuration = damageScale.chillDuration;
        chillSlowEffect = damageScale.chillSlowMultiplier;


        burnDuration = damageScale.burnDuration;
        totalBurnDamage = entityStats.offense.fireDamage.GetValue() * damageScale.burnDamageScale;

        lightningDuration = damageScale.lightningDuration;
        lightningDamage = entityStats.offense.lightningDamage.GetValue() * damageScale.lightningDamageScale;
        lightningCharge = damageScale.lightningCharge;
        
    }

    public ElementalEffectData(ElementType element, float elementalDamage, DamageScaleData damageScale)
    {
        chillDuration = damageScale.chillDuration;
        chillSlowEffect = damageScale.chillSlowMultiplier;

        burnDuration = damageScale.burnDuration;
        totalBurnDamage = element == ElementType.Fire
            ? elementalDamage * damageScale.burnDamageScale
            : 0f;

        lightningDuration = damageScale.lightningDuration;
        lightningDamage = element == ElementType.Lightning
            ? elementalDamage * damageScale.lightningDamageScale
            : 0f;
        lightningCharge = element == ElementType.Lightning
            ? damageScale.lightningCharge
            : 0f;
    }
   
}

