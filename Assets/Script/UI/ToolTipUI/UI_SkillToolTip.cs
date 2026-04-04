using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class UI_SkillToolTip : UI_ToolTip
{
    private UI_SkillTree skillTree;
    private UI ui;
    protected override void Awake()
    {
        base.Awake();
       
        ui= GetComponentInParent<UI>();
        skillTree=ui.GetComponentInChildren<UI_SkillTree>(true);
    }

    [SerializeField] private TextMeshProUGUI skillname;
    [SerializeField] private TextMeshProUGUI skilldescription;
    [SerializeField] private TextMeshProUGUI skillrequirements;

    [Space]
    [SerializeField] private string metConditionHex;
    [SerializeField] private string unmetConditionHex;
    [SerializeField] private string ImportantInfoHex;
    [SerializeField] private Color exampleColor;
    [SerializeField] private string lockedSkillText = "You've taken a diffrent path - this skill is now locked.";


    private Coroutine textEffectCo;
    public override void ShowToolTip(bool show, RectTransform targetRect)
    {
        base.ShowToolTip(show, targetRect);
    }
    public void ShowToolTip(bool show, RectTransform targetRect,SkillDataSO skillData,UI_TreeNode node)
    {
        base.ShowToolTip(show, targetRect);

        if (show == false)
            return;

        // 传入技能数据为空时直接清空文本, 避免空引用.
        if (skillData == null)
        {
            if (skillname != null) skillname.text = "";
            if (skilldescription != null) skilldescription.text = "";
            if (skillrequirements != null) skillrequirements.text = "";
            return;
        }

        // 文本组件未绑定时不继续写入, 避免空引用.
        if (skillname == null || skilldescription == null || skillrequirements == null)
            return;

        skillname.text = skillData.skillName;
        skilldescription.text = skillData.description;

        if(node==null)
        {
            skillrequirements.text = "";
            return;
        }

        string SkillLockedText = $"<color={ImportantInfoHex}>{lockedSkillText} </color>";

        //  节点技能数据异常时退化显示锁定文本, 避免空引用.
        string requirements = (node.isLocked || node.skillData == null)
            ? SkillLockedText
            : getRequirements(node.skillData.cost, node.neededNodes, node.conflictNodes);

        skillrequirements.text = requirements;
    }
    private string getRequirements(int skillcost, UI_TreeNode[] neededNodes,UI_TreeNode[] conflictNodes)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Requirements:");

        //  skillTree可能为空, 为空时按不满足条件处理.
        bool enoughPoints = skillTree != null && skillTree.EnoughSkillPoints(skillcost);
        string costColor = enoughPoints ? metConditionHex : unmetConditionHex;

        sb.AppendLine($"<color={costColor}>-{skillcost} skill point(s) </color>");

        //  依赖数组未配置时按空数组处理.
        if (neededNodes == null)
            neededNodes = System.Array.Empty<UI_TreeNode>();

        foreach (var node in neededNodes)
        {
            if(node==null || node.skillData == null)
                continue;

            string nodeColor = node.isUnlocked ? metConditionHex : unmetConditionHex;
            sb.AppendLine($"<color={nodeColor}>-{node.skillData.skillName} skill point(s) </color>");
        }

        if (conflictNodes == null || conflictNodes.Length<=0)
            return sb.ToString();

        sb.AppendLine();
        sb.AppendLine($"<color={ImportantInfoHex}> Locks out: </color>");

        foreach (var node in conflictNodes)
        {
            if (node == null || node.skillData == null)
                continue;

            sb.AppendLine($"<color={ImportantInfoHex}>- {node.skillData.skillName} </color>");
        }
        return sb.ToString();
    }

  
    public void LockedSkillEffect()
    {
        StopLockedSkillEffect();

        textEffectCo=StartCoroutine(TextBlinkEffectCo(skillrequirements,0.15f,3));
    }
    public void StopLockedSkillEffect()
    {
        if (textEffectCo != null)
            StopCoroutine(textEffectCo);
    }
    private IEnumerator TextBlinkEffectCo(TextMeshProUGUI text,float BlinkInterval,int BlinkCount)
    {
        for(int i = 0; i < BlinkCount; i++)
        {
            text.text = GetTextColored(unmetConditionHex, lockedSkillText);
            yield return new WaitForSeconds(BlinkInterval);

            text.text=GetTextColored(ImportantInfoHex, lockedSkillText);
            yield return new WaitForSeconds(BlinkInterval);

        }
    }
}
