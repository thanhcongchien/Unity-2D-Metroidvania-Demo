using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_SkillTree : MonoBehaviour,ISaveable
{
    public int skillPoints;
    [SerializeField] private TextMeshProUGUI skillpointsText;
    public UI_TreeConnectHandler[] parentNodes;
    public PlayerSkillManager skillManager;
    private UI_TreeNode[] allTreeNodes;

    [ContextMenu("RefundAllSkills")]
    public void RefundAllSkills()
    {
        EnsureInitialized();
        UI_TreeNode[] nodes = GetComponentsInChildren<UI_TreeNode>();
        foreach (var node in nodes)
        {
            node.Refund();
        }
    }

    

    public void UnlockDefaultSkills()
    {
        EnsureInitialized();
        foreach (var node in allTreeNodes)
        {
            node.UnlockDefaultSkill();
        }
    }




    public bool EnoughSkillPoints(int cost)=>skillPoints>=cost;


    public void RemoveSkillPoints(int cost)
    {
        skillPoints -= cost;
        UpdateSkillPointsUI();
    }
    public void AddSkillPoints(int cost)
    {
        skillPoints += cost;
        UpdateSkillPointsUI();
    }




    private void Start()
    {
        EnsureInitialized();
        UpdateAllConnections();
        RefreshAllTreeNodeVisuals();
        UpdateSkillPointsUI();
    }

    private void EnsureInitialized()
    {
        if (skillManager == null)
            skillManager = FindAnyObjectByType<PlayerSkillManager>();

        if (allTreeNodes == null || allTreeNodes.Length == 0)
            allTreeNodes = GetComponentsInChildren<UI_TreeNode>(true);
    }

    private void RefreshAllTreeNodeVisuals()
    {
        if (allTreeNodes == null)
            return;

        foreach (var node in allTreeNodes)
        {
            node.RefreshVisualState();
        }
    }

    private void UpdateSkillPointsUI()
    {
        skillpointsText.text = skillPoints.ToString();
    }

    [ContextMenu("UpdateAllConnections")]
    public void UpdateAllConnections()
    {
        foreach (var node in parentNodes)
        {
            node.UpdateAllConnections();
        }

    }

    public void LoadData(GameData data)
    {
        EnsureInitialized();
       skillPoints = data.skillPoints;

        foreach(var node in allTreeNodes)
        {
            if(data.skillTreeUI.TryGetValue(node.skillData.skillName,out bool unlocked)&&unlocked)
            {
                node.UnlockWithSavedData();
            }
        }

        foreach(var skill in skillManager.allSkills)
        {
            if(data.skillUpgrades.TryGetValue(skill.GetSkillType(),out SkillUpgradeType upgradeType))
            {
                foreach(var node in allTreeNodes)
                {
                    if (node.skillData.UpgradeData.upgradeType == upgradeType)
                        skill.SetSkillUpgrade(node.skillData);
                }
            }
        }

        UpdateAllConnections();
        RefreshAllTreeNodeVisuals();
        UpdateSkillPointsUI();
    }

    public void SaveData(ref GameData data)
    {
        EnsureInitialized();
        data.skillPoints = skillPoints;
        data.skillTreeUI.Clear();
        data.skillUpgrades.Clear();

        foreach(var node in allTreeNodes)
        {
            string skillName = node.skillData.skillName;
            data.skillTreeUI[skillName] = node.isUnlocked;
        }

        foreach(var skill in skillManager.allSkills)
        {
            data.skillUpgrades[skill.GetSkillType()] = skill.GetUpgradeType();
        }
    }
}
