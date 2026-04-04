using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "RPG Setup/Skill Data", fileName = "Skill data - ")]
public class SkillDataSO : ScriptableObject
{
    
    

    [Header("Skill description")]
    public string skillName;
    [TextArea]
    public string description;
    public Sprite icon;

    [Header("Unlock & Upgrade")]
    public int cost;
    public SkillType skillType;
    public UpgradeData UpgradeData;
    public bool unlockByDefault;
}
[Serializable]
public class UpgradeData
{
   public SkillUpgradeType upgradeType;
    public float cooldown;
    public DamageScaleData damageScale;
}


