using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTimeEcho : SkillBase
{
    
    [SerializeField] private GameObject TimeEchoPrefab;
    [SerializeField] private float EchoDuration = 2f;

    [Header("Attack Upgrades")]
    [SerializeField] private int maxAttacks = 3;
    [SerializeField] private float cloneChance = .3f;

    [Header("Heal Wisp Upgrades")]
    [SerializeField] private float damagePercentHealed = .3f;
    [SerializeField] private float cooldownReducedInSeconds;

    public float getPercentOfDamageHealed()
    {
        if (ShouldBeWisp() == false)
            return 0;
        return damagePercentHealed;

    }
    public float getCooldownReducedInSeconds()
    {
        if(upgradeType!=SkillUpgradeType.TimeEcho_CooldownWisp)
            return 0;
        return cooldownReducedInSeconds;
    }
    public bool CanRemoveNegativeEffects()
    {

        return upgradeType == SkillUpgradeType.TimeEcho_CleanseWisp;
    }

    public bool ShouldBeWisp()
    {
        return upgradeType == SkillUpgradeType.TimeEcho_HealWisp
        || upgradeType == SkillUpgradeType.TimeEcho_CleanseWisp
        || upgradeType == SkillUpgradeType.TimeEcho_CooldownWisp;

    }

    public float getCloneChance()
    {
        if (upgradeType != SkillUpgradeType.TimeEcho_ChanceToDuplicate)
            return 0;
        return cloneChance;
    }
    public int getMaxAttacks()
    {
        if (upgradeType == SkillUpgradeType.TimeEcho_SingleAttack || upgradeType == SkillUpgradeType.TimeEcho_ChanceToDuplicate)
            return 1;
        if(upgradeType == SkillUpgradeType.TimeEcho_MultiAttack)
            return maxAttacks;

        return 0;
    }

    public float getEchoDuration()
    {
        return EchoDuration;
    }
    public override void TryUseSkill()
    {
        if (CanUseSkill() == false)
            return;
        CreateEcho();

        SetSkillCooldown();
    }
   

    public void CreateEcho(Vector3? targetPosition=null)
    {
        Vector3 position = targetPosition ?? transform.position;


        GameObject timeEcho = Instantiate(TimeEchoPrefab, position, Quaternion.identity);
        timeEcho.GetComponent<SkillObjectTimeEcho>().SetUpEcho(this);
    }

   
}
