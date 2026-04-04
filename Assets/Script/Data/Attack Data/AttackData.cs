using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AttackData
{

    public float phyiscalDamage;
    public float elementalDamage;
    public bool isCrit;
    public ElementType element;

    public ElementalEffectData effectData;


    public AttackData(EntityStats entityStats, DamageScaleData scaleData)
    {
        phyiscalDamage = entityStats.getPhysicalDamage(out isCrit, scaleData.physical);
        elementalDamage = entityStats.getElementalDamage(out element, scaleData.elemental);

        effectData = new ElementalEffectData(entityStats, scaleData);
    }
}
