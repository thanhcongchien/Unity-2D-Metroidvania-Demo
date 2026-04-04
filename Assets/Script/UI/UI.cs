using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI instance;



    [SerializeField] private GameObject[] uiElements;
    public UI_SkillToolTip skillToolTip {  get; private set; }
    public UI_ItemToolTip itemToolTip { get; private set;}
    public UI_StatToolTip statToolTip { get; private set;}


    public UI_SkillTree skillTreeUI {  get; private set;}
    public UI_Inventory inventoryUI { get; private set; }

    public UI_Options optionsUI { get; private set; }
    public UI_Storage storageUI { get; private set; }

    public UI_Craft craftUI { get; private set; }
    
    public UI_Merchant merchantUI { get; private set; }

    public UI_InGame inGameUI { get; private set; }

    public UI_DeathScreen deathScreenUI { get; private set; }

    public UI_FadeScreen fadeScreenUI { get; private set; }

    public UI_Quest questUI { get; private set;}

    public UI_Dialogue dialogueUI { get; private set; }

    private bool skillTreeEnabled;
    private bool inventoryEnabled;

    private void Awake()
    {
        instance = this;


        statToolTip = GetComponentInChildren<UI_StatToolTip>();
        itemToolTip = GetComponentInChildren<UI_ItemToolTip>();
        skillToolTip = GetComponentInChildren<UI_SkillToolTip>();
        skillTreeUI = GetComponentInChildren<UI_SkillTree>(true);
        inventoryUI = GetComponentInChildren<UI_Inventory>(true);
        optionsUI = GetComponentInChildren<UI_Options>(true);
        storageUI = GetComponentInChildren<UI_Storage>(true);
        craftUI = GetComponentInChildren<UI_Craft>(true);   
        merchantUI = GetComponentInChildren<UI_Merchant>(true);
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        deathScreenUI = GetComponentInChildren<UI_DeathScreen>(true);
        fadeScreenUI = GetComponentInChildren<UI_FadeScreen>(true);
        questUI = GetComponentInChildren<UI_Quest>(true);
        dialogueUI = GetComponentInChildren<UI_Dialogue>(true);

        skillTreeEnabled = skillTreeUI.gameObject.activeSelf;
        inventoryEnabled = inventoryUI.gameObject.activeSelf;

    }
    private void Start()
    {
        skillTreeUI.UnlockDefaultSkills();
        RegisterButtonClickSFX();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            ToggleSkillTree();

        if (Input.GetKeyDown(KeyCode.C))
            ToggleInventory();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            foreach(var uiElement in uiElements)
            {
                if(uiElement.activeSelf)
                {
                    Time.timeScale = 1;
                    SwitchToIngameUI();
                    return;
                }
            }
            Time.timeScale = 0;
            ToggleOptions();
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            dialogueUI.NavigateChoice(-1);
        }
        // 按下S/下箭头 → 向上切换（direction=1）
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            dialogueUI.NavigateChoice(1);
        }
        HandleInteractInput();

    }
    private void HandleInteractInput()
    {
        if (!Input.GetKeyDown(KeyCode.F))
            return;

        if (dialogueUI != null && dialogueUI.gameObject.activeInHierarchy)
        {
            dialogueUI.DialogueInteraction();
            return;
        }

        // Non-dialogue panels open means gameplay interaction is blocked.
        if (IsGameplayInputBlocked())
            return;

        TryInteractClosest();
    }

    private void TryInteractClosest()
    {
        if (Player.instance == null)
            return;

        Transform playerTransform = Player.instance.transform;
        Transform closest = null;
        float closestDistance = Mathf.Infinity;
        Collider2D[] objectsAround = Physics2D.OverlapCircleAll(playerTransform.position, 1f);

        foreach (var target in objectsAround)
        {
            IInteractable interactable = target.GetComponent<IInteractable>();
            if (interactable == null)
                continue;

            float distance = Vector2.Distance(playerTransform.position, target.transform.position);
            if (distance < closestDistance)
            {
                closest = target.transform;
                closestDistance = distance;
            }
        }

        if (closest == null)
            return;

        closest.GetComponent<IInteractable>().Interact();
    }
    public void SwitchOffAllToolTips()
    {
        if(itemToolTip != null)
            itemToolTip.ShowToolTip(false, null);
        if(skillToolTip != null)
            skillToolTip.ShowToolTip(false, null);
        if(statToolTip != null)
            statToolTip.ShowToolTip(false, null);
    }
    public void SwitchDeathScreen()
    {
        SwitchTo(deathScreenUI.gameObject);
        
    }
    public void SwitchToIngameUI()
    {
        
        HideAllToolTips();
        SwitchTo(inGameUI.gameObject);

        skillTreeEnabled = false;
        inventoryEnabled = false;
    }
    private void SwitchTo(GameObject objectToSwitchOn)
    {
        foreach(var element in uiElements)
        {
            element.gameObject.SetActive(false);
        }

        objectToSwitchOn.SetActive(true);
    }

    public void ToggleSkillTree()
    {
        skillTreeUI.transform.SetAsLastSibling();
        SetToolTipAbove();
        fadeScreenUI.transform.SetAsLastSibling();

        skillTreeEnabled = !skillTreeEnabled;
        skillTreeUI.gameObject.SetActive(skillTreeEnabled);
        if(skillToolTip != null)
            skillToolTip.ShowToolTip(false, null);

    }
    public void ToggleInventory()
    {
        inventoryUI.transform.SetAsLastSibling();
        SetToolTipAbove();
        fadeScreenUI.transform.SetAsLastSibling();

        inventoryEnabled = !inventoryEnabled;
        inventoryUI.gameObject.SetActive(inventoryEnabled);
        HideAllToolTips() ;
    }
    public void ToggleOptions()
    {
        if (optionsUI == null)
            return;
        foreach(var uiElement in uiElements)
        {
           uiElement.gameObject.SetActive(false);
        }
        HideAllToolTips();
        bool next = !optionsUI.gameObject.activeSelf;
        optionsUI.gameObject.SetActive(next);
    }

    public void ToggleDialogueUI(DialogueLineSO firstLine,DialogueNPCData npcData)
    {
        HideAllToolTips();

        dialogueUI.gameObject.SetActive(true);
        dialogueUI.SetupNpcData(npcData);
        dialogueUI.PlayDialogueLine(firstLine);
    }

    public void ToggleCraftUI(bool openCraftUI)
    {
        craftUI.gameObject.SetActive(openCraftUI);

      
    }    

    public void ToggleStorage(bool openStorageUI)
    {
        storageUI.gameObject.SetActive(openStorageUI);
    }
    public void ToggleMerchant(bool openMerchantUI)
    {
        merchantUI.gameObject.SetActive(openMerchantUI);
    }
   public void ToggleQuestUI(QuestDataSO[] questsToShow)
    {
        HideAllToolTips();

        questUI.gameObject.SetActive(true);
        questUI.SetupQuestUI(questsToShow);
    }
    public void HideAllToolTips()
    {
        skillToolTip.ShowToolTip(false, null);
        itemToolTip.ShowToolTip(false, null);
        statToolTip.ShowToolTip(false, null);
    }
    
    public bool IsGameplayInputBlocked()
    {
        return IsOpen(skillTreeUI)
            || IsOpen(inventoryUI)
            || IsOpen(storageUI)
            || IsOpen(craftUI)
            || IsOpen(merchantUI)
            || IsOpen(optionsUI)
            || IsOpen(deathScreenUI)
            || IsOpen(questUI)
            || IsOpen(dialogueUI);
    }

    private bool IsOpen(MonoBehaviour panel)
    {
        return panel != null && panel.gameObject.activeInHierarchy;
    }
    private void SetToolTipAbove()
    {
        itemToolTip.transform.SetAsLastSibling();
        skillToolTip.transform.SetAsLastSibling();
        statToolTip.transform.SetAsLastSibling();
    }

    private void RegisterButtonClickSFX()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (var button in buttons)
        {
            button.onClick.RemoveListener(PlayButtonClickSFX);
            button.onClick.AddListener(PlayButtonClickSFX);
        }
    }

    private void PlayButtonClickSFX()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("Button_Click");
    }
}
