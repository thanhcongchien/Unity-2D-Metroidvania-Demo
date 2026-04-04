using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class EntityStats : MonoBehaviour
{

    public StatSetupSO defaultStatSetup;

    public ElementType elementType;
    public StatResourceGroup resources;
    public StatOffenseGroup offense;
    public StatDefenseGroup defense;
    public StatMajorGroup major;

    public AttackData getAttackData(DamageScaleData damageScale)
    {
        return new AttackData(this, damageScale);
    }

    public void AdjustStatSetup(StatResourceGroup resourceGroup, StatOffenseGroup offenseGroup, StatDefenseGroup defenseGroup, float penalty, float increase)
    {
        // INCREASE STATS
        offense.damage.SetBaseValue(offenseGroup.damage.GetValue() * increase);
        offense.attackSpeed.SetBaseValue(offenseGroup.attackSpeed.GetValue() * increase);
        offense.critChance.SetBaseValue(offenseGroup.critChance.GetValue() * increase);
        offense.critPower.SetBaseValue(offenseGroup.critPower.GetValue() * increase);
        offense.fireDamage.SetBaseValue(offenseGroup.fireDamage.GetValue() * increase);
        offense.iceDamage.SetBaseValue(offenseGroup.iceDamage.GetValue() * increase);
        offense.lightningDamage.SetBaseValue(offenseGroup.lightningDamage.GetValue() * increase);

        defense.evasion.SetBaseValue(defenseGroup.evasion.GetValue() * increase);

        // PENALTY STATS
        resources.maxHealth.SetBaseValue(resourceGroup.maxHealth.GetValue() * penalty);
        resources.healthRegen.SetBaseValue(resourceGroup.healthRegen.GetValue() * penalty);

        defense.armor.SetBaseValue(defenseGroup.armor.GetValue() * penalty);
        defense.lightningRes.SetBaseValue(defenseGroup.lightningRes.GetValue() * penalty);
        defense.fireRes.SetBaseValue(defenseGroup.fireRes.GetValue() * penalty);
        defense.iceRes.SetBaseValue(defenseGroup.iceRes.GetValue() * penalty);
    }


    public float getElementalDamage(out ElementType element,float scaleFactor=1)
    {

        float fireDamage = offense.fireDamage.GetValue();
        float iceDamage=offense.iceDamage.GetValue();
        float lightningDamage=offense.lightningDamage.GetValue();

        float bonusElementalDamage=major.Intelligence.GetValue();

        float highestDamage = fireDamage;
        element = ElementType.Fire;


        if (iceDamage > highestDamage)
        {
            highestDamage = iceDamage;
            element = ElementType.Ice;
        }
        if (lightningDamage > highestDamage)
        {
            highestDamage = lightningDamage;
            element = ElementType.Lightning;
        }
        if (highestDamage <= 0)
        {
            element = ElementType.None;
            return 0;
        }
        float bonusFire = (element==ElementType.Fire) ? 0 : fireDamage * 0.5f;
        float bonusIce = (element==ElementType.Ice) ? 0 : iceDamage * 0.5f;
        float bonusLightning = (element == ElementType.Lightning) ? 0 : lightningDamage * 0.5f;

        float weakerElementalDamage = bonusFire + bonusIce + bonusLightning;

        float finalDamage = highestDamage+weakerElementalDamage + bonusElementalDamage;

        return finalDamage*scaleFactor;
    }

    public float getElementalResistance(ElementType element)
    {
        float baseResistance = 0;
        float bonusResistance = major.Intelligence.GetValue() * 0.5f;

        if(element==ElementType.Fire)
        {
            baseResistance = defense.fireRes.GetValue();
        }
        if(element==ElementType.Ice)
        {
            baseResistance = defense.iceRes.GetValue();
        }
        if(element==ElementType.Lightning)
        {
            baseResistance = defense.lightningRes.GetValue();
        }
        float resistance = baseResistance + bonusResistance;
        float resistanceCap = 75f;
        float finalResistance = Mathf.Clamp(resistance, 0, resistanceCap) / 100;
        return finalResistance;
    }






    public float getPhysicalDamage(out bool isCrit,float scaleFactor=1)
    {

        float baseDamage = GetBaseDamage();

        
        float critChance=GetCritChance();

        
        float critPower = GetCritPower();

        isCrit = Random.Range(0, 100) < critChance;
        float finalDamage = isCrit ? baseDamage * (critPower / 100f) : baseDamage;

        return finalDamage*scaleFactor;
    }

    // Bonus damage from Strength: +1 per STR
    public float GetBaseDamage() => offense.damage.GetValue() + major.Strength.GetValue();
    //  Bonus crit chance from Agility: +0.3% per AGI 
    public float GetCritChance() => offense.critChance.GetValue() + (major.Agility.GetValue() * .3f);
    // Bonus crit chance from Strength: +0.5% per STR 
    public float GetCritPower() => offense.critPower.GetValue() + (major.Strength.GetValue() * .5f);

    public float getArmorMitigation(float armorReduction)
    {
        
        float totalArmor=GetBaseArmor();

        float reductionMutliplier =Mathf.Clamp(1 - armorReduction,0,1);
        float effectiveArmor = totalArmor * reductionMutliplier;


        float mitigation = effectiveArmor / (effectiveArmor + 100);
        float mitigationCap = .85f;

        float finalMitigation = Mathf.Clamp(mitigation, 0, mitigationCap);

        return finalMitigation;


    }

    public float getArmorReduction()
    {

        float finalReduction = offense.armorReduction.GetValue() / 100;

        return finalReduction;
    }








    public float getMaxHealth()
    {
        float baseHp = resources.maxHealth.GetValue();
        float bonusHp = major.Vitality.GetValue() * 5;
        return baseHp + bonusHp;

    }

    public float getEvasion()
    {
        float baseEvasion = defense.evasion.GetValue();
        float bonusEvasion = major.Agility.GetValue() * .5f;

       float totalEvasion = baseEvasion + bonusEvasion;
        float evasionCap = 85f;

        float finalEvasion = Mathf.Clamp(totalEvasion, 0f, evasionCap);


        return  finalEvasion;
    }

    public float GetBaseArmor() => defense.armor.GetValue() + major.Vitality.GetValue();

    public float GetArmorReduction()
    {
        // Total armor reduction as multiplier ( e.g 30 / 100 = 0.3f - multiplier)
        float finalReduction = offense.armorReduction.GetValue() / 100;

        return finalReduction;
    }

    public float GetEvasion()
    {
        float baseEvasion = defense.evasion.GetValue();
        float bonusEvasion = major.Agility.GetValue() * .5f; // Bonus evasion from Agility: +0.5% per AGI 

        float totalEvasion = baseEvasion + bonusEvasion;
        float evasionCap = 85f; // Max evasion will be capped at 85%

        float finalEvasion = Mathf.Clamp(totalEvasion, 0, evasionCap);

        return finalEvasion;
    }

    public float GetMaxHealth()
    {
        float baseMaxHealth = resources.maxHealth.GetValue();
        float bonusMaxHealth = major.Vitality.GetValue() * 5;
        float finalMaxHealth = baseMaxHealth + bonusMaxHealth;

        return finalMaxHealth;
    }
    public Stat getStatByType(StatType type)
    {
        switch (type)
        {
            case StatType.MaxHealth: return resources.maxHealth;
            case StatType.HealthRegen: return resources.healthRegen;

            case StatType.Strength: return major.Strength;
            case StatType.Agility: return major.Agility;
            case StatType.Intelligence: return major.Intelligence;
            case StatType.Vitality: return major.Vitality;

            case StatType.AttackSpeed: return offense.attackSpeed;
            case StatType.Damage: return offense.damage;
            case StatType.CritChance: return offense.critChance;
            case StatType.CritPower: return offense.critPower;
            case StatType.ArmorReduction: return offense.armorReduction;

            case StatType.FireDamage: return offense.fireDamage;
            case StatType.IceDamage: return offense.iceDamage;
            case StatType.LightningDamage: return offense.lightningDamage;
           

            case StatType.Armor: return defense.armor;
            case StatType.Evasion: return defense.evasion;

            case StatType.IceResistance: return defense.iceRes;
            case StatType.FireResistance: return defense.fireRes;
            case StatType.LightningResistance: return defense.lightningRes;

            default:
                //Debug.LogWarning($"StatType {type} not implemented yet.");
                return null;
        }



    }
    [ContextMenu("Update Default Stat")]
    public void ApplyDefaultStatSetup()
    {

        if (defaultStatSetup == null)
            return;
        resources.maxHealth.SetBaseValue(defaultStatSetup.maxHealth);
        resources.healthRegen.SetBaseValue(defaultStatSetup.healthRegen);

        major.Strength.SetBaseValue(defaultStatSetup.strength);
        major.Agility.SetBaseValue(defaultStatSetup.agility);
        major.Intelligence.SetBaseValue(defaultStatSetup.intelligence);
        major.Vitality.SetBaseValue(defaultStatSetup.vitality);

        offense.attackSpeed.SetBaseValue(defaultStatSetup.attackSpeed);
        offense.damage.SetBaseValue(defaultStatSetup.damage);
        offense.critChance.SetBaseValue(defaultStatSetup.critChance);
        offense.critPower.SetBaseValue(defaultStatSetup.critPower);
        offense.armorReduction.SetBaseValue(defaultStatSetup.armorReduction);
        offense.fireDamage.SetBaseValue(defaultStatSetup.fireDamage);
        offense.iceDamage.SetBaseValue(defaultStatSetup.iceDamage);
        offense.lightningDamage.SetBaseValue(defaultStatSetup.lightningDamage);

        defense.armor.SetBaseValue(defaultStatSetup.armor);
        defense.evasion.SetBaseValue(defaultStatSetup.evasion);

        defense.fireRes.SetBaseValue(defaultStatSetup.fireResistance);
        defense.iceRes.SetBaseValue(defaultStatSetup.iceResistance);
        defense.lightningRes.SetBaseValue(defaultStatSetup.lightningResistance);
       


    }





}
