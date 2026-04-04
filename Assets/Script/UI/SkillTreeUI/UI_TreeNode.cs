using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_TreeNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private RectTransform rect;
    private UI_SkillTree skillTree;
    private UI_TreeConnectHandler connectHandler;

    [Header("Unlock details")]
    public UI_TreeNode[] neededNodes;
    public UI_TreeNode[] conflictNodes;
    public bool isLocked;
    public bool isUnlocked;

    [Header("Skill details")]
    [SerializeField] public SkillDataSO skillData;
    [SerializeField] public string skillName;
    [SerializeField] public Image skillIcon;
    [SerializeField] public int skillCost;
    [SerializeField] public Color skillLockedColor;
    private string lockedColorHex = "#918989";
    private Color lastColor;



   
    private void GetNeededComponents()
    {
        ui = GetComponentInParent<UI>();
        rect = GetComponent<RectTransform>();
        skillTree = GetComponentInParent<UI_SkillTree>(true);
        connectHandler = GetComponent<UI_TreeConnectHandler>();
    }
    private void Start()
    {
        GetNeededComponents();
        UnlockDefaultSkill();
        RefreshVisualState();
    }
    public void UnlockDefaultSkill() 
    {
        GetNeededComponents();
       
        if (skillData.unlockByDefault)
            Unlock();
    }
    public void Refund()
    {
        if(isUnlocked==false||skillData.unlockByDefault)
            return;


        isUnlocked = false;
        isLocked = false;
        UpdateIconColor(getColorByHex(lockedColorHex));

        skillTree.AddSkillPoints(skillData.cost);
        connectHandler.ConnectionImageUnlock(false);
    }

    private void Unlock()
    {
        if(isUnlocked)
            return;

        isUnlocked = true;
        UpdateIconColor(Color.white);
        skillTree.RemoveSkillPoints(skillData.cost);
        LockConflictNodes();

        connectHandler.ConnectionImageUnlock(true);

        skillTree.skillManager.getSkillByType(skillData.skillType).SetSkillUpgrade(skillData);
    }
  
   public void UnlockWithSavedData()
    {
        isUnlocked = true;
        LockConflictNodes();
        RefreshVisualState();
    }

    private bool CanbeLocked()
    {
        if (isUnlocked || isLocked)
            return false;

        if (skillTree.EnoughSkillPoints(skillData.cost) == false)
            return false;

        foreach (var node in neededNodes)
        {
            if (node.isUnlocked == false)
                return false;
        }

        foreach (var node in conflictNodes)
        {
            if (node.isUnlocked)
                return false;
        }


        return true;
    }

    private void LockConflictNodes()
    {
        foreach (var node in conflictNodes)
        {
            node.isLocked = true;
            node.LockChildNodes();
        }
    }
    public void LockChildNodes()
    {
        isLocked = true;
        foreach (var node in connectHandler.getChildNodes())
        {
            node.LockChildNodes();
        }
    }

    public void RefreshVisualState()
    {
        if (isUnlocked)
        {
            UpdateIconColor(Color.white);
            connectHandler.ConnectionImageUnlock(true);
            return;
        }

        UpdateIconColor(getColorByHex(lockedColorHex));
        connectHandler.ConnectionImageUnlock(false);
    }




    private void UpdateIconColor(Color color)
    {
        if (skillIcon == null)
            return;

        lastColor = skillIcon.color;
        skillIcon.color = color;

    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (CanbeLocked())
            Unlock();
        else if(isLocked)
            ui.skillToolTip.LockedSkillEffect();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ui.skillToolTip.ShowToolTip(true, rect,skillData, this);

        if (isUnlocked || isLocked)
            return;
        
        Color color = Color.white * .9f;
        color.a = 1;
        UpdateIconColor(color);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.skillToolTip.ShowToolTip(false, rect);
        ui.skillToolTip.StopLockedSkillEffect();
        if(isUnlocked||isLocked)
            return;
            UpdateIconColor(lastColor);
    }

    private Color getColorByHex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;

    }
    private void OnValidate()
    {
        if (skillData == null)
        {
            return;
        }
        skillName = skillData.skillName;
        skillIcon.sprite = skillData.icon;
        skillCost = skillData.cost;
        gameObject.name = "UI_TreeNode-" + skillData.name;
    }
}
