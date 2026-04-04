using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBase : MonoBehaviour
{
    public PlayerSkillManager skillManager { get; private set;}
    public Player player {  get; private set; }

    public DamageScaleData damageScaleData {  get; private set; }

    [Header("General Details")]
    [SerializeField] protected SkillType skillType;
    [SerializeField] protected SkillUpgradeType upgradeType;
    [SerializeField] protected float cooldown;
    private float lastTimeUsed;


    public void SetSkillUpgrade(SkillDataSO skillData)
    {
        UpgradeData upgrade = skillData.UpgradeData;
        upgradeType = upgrade.upgradeType;
        cooldown = upgrade.cooldown;
        damageScaleData = upgrade.damageScale;


        player.ui.inGameUI.getSkillSlot(skillType).SetupSkillSlot(skillData);


        ResetCooldown();
    }



    protected virtual void Awake()
    {
        skillManager = GetComponentInParent<PlayerSkillManager>();
        player = GetComponentInParent<Player>();
        lastTimeUsed-=cooldown;
        damageScaleData = new DamageScaleData();
    }

    public virtual void TryUseSkill()
    {
        
    }

    public bool IsSkillUpgrade()
    {
        if (upgradeType == SkillUpgradeType.None)
            return false;
        return true;
    }
    public virtual bool CanUseSkill()
    {
        if (OnCooldown())
            return false;
        if (upgradeType == SkillUpgradeType.None)
            return false;

        return true;
    }

    protected bool Unlocked(SkillUpgradeType upgradeToCheck) => upgradeType == upgradeToCheck;
    public SkillUpgradeType GetUpgradeType() => upgradeType;
    public SkillType GetSkillType() => skillType;

    public bool OnCooldown()=> Time.time - lastTimeUsed < cooldown;
    public void SetSkillCooldown()
    {
        player.ui.inGameUI.getSkillSlot(skillType).StartCooldown(cooldown);
        lastTimeUsed = Time.time;
    }
    public void ReduceCooldownBy(float cooldownReduction)=>lastTimeUsed+=cooldownReduction;
    public void ResetCooldown()
    {
        lastTimeUsed = Time.time - cooldown;

        player.ui.inGameUI.getSkillSlot(skillType).ResetCooldown();
    }

}
