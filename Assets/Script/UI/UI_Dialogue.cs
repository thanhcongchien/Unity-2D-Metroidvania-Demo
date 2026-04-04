using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Dialogue : MonoBehaviour
{
    private UI ui;
    private DialogueNPCData npcData;
    private Player_QuestManager questManager;
    private Player_NpcAffinityManager affinityManager;
    private InventoryPlayer inventory;

    [SerializeField] private Image speakerPortrait;
    [SerializeField] private TextMeshProUGUI speakerName;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI[] dialogueChoicesText;
    [Header("Affinity UI")]
    [SerializeField] private GameObject affinityRoot;
    [SerializeField] private Image affinityFillImage;
    [SerializeField] private TextMeshProUGUI affinityValueText;
    [Space]
    [SerializeField] private float textSpeed = .1f;
    private string fullTextToShow;
    private Coroutine typeTextCo;

    private DialogueLineSO currentLine;
    private DialogueLineSO[] currentChoices;
    private DialogueLineSO selectedChoice;
    private int selectedChoiceIndex;
    private bool appliedAffinityThisLine;

    private bool waitingForConfirm;
    private bool canInteract;

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        EnsureRuntimeReferences();
    }
    public void SetupNpcData(DialogueNPCData npcData)
    {
        this.npcData = npcData;
        EnsureRuntimeReferences();

    }
    public void PlayDialogueLine(DialogueLineSO line)
    {
        EnsureRuntimeReferences();
        if (line == null)
        {
            HideAffinityUI();
            ui.SwitchToIngameUI();
            return;
        }

        if (typeTextCo != null)
        {
            StopCoroutine(typeTextCo);
            typeTextCo = null;
        }

        currentLine = line;
        canInteract = false;
        waitingForConfirm = false;
        selectedChoice = null;
        selectedChoiceIndex = 0;
        appliedAffinityThisLine = false;
        TryApplyLineAffinity();
        currentChoices = GetAvailableChoices(line.choiceLines);

        HideAllChoices();

        if (speakerPortrait != null)
            speakerPortrait.sprite = line.speaker != null ? line.speaker.speakerPortrait : null;

        if (speakerName != null)
            speakerName.text = line.speaker != null ? line.speaker.speakerName : string.Empty;

        RefreshAffinityUI();
        fullTextToShow = ResolveDialogueText(line);
        typeTextCo = StartCoroutine(TypeTextCo(fullTextToShow));
        StartCoroutine(EnableInteractionCo());
    }

    private string ResolveDialogueText(DialogueLineSO line)
    {
        bool useActionLine = line.actionType != DialogueActionType.None
            && line.actionType != DialogueActionType.PlayerMakeChoice
            && string.IsNullOrWhiteSpace(line.actionLine) == false;

        return useActionLine ? line.actionLine : line.GetRandomLine();
    }

    private DialogueLineSO[] GetAvailableChoices(DialogueLineSO[] choices)
    {
        if (choices == null || choices.Length == 0)
            return System.Array.Empty<DialogueLineSO>();

        List<DialogueLineSO> availableChoices = new List<DialogueLineSO>();
        foreach (var choice in choices)
        {
            if (choice == null)
                continue;

            if (choice.IsAvailable(questManager, affinityManager, inventory, npcData?.npcId) == false)
                continue;

            if (choice.actionType == DialogueActionType.GetQuestReward && questManager != null && questManager.HasCompletedQuest() == false)
                continue;

            availableChoices.Add(choice);
        }

        return availableChoices.ToArray();
    }

    private void TryApplyLineAffinity()
    {
        if (appliedAffinityThisLine)
            return;
        if (currentLine == null || currentLine.affinityDelta == 0)
            return;
        if (npcData == null || string.IsNullOrWhiteSpace(npcData.npcId))
            return;
        if (affinityManager == null)
            return;

        affinityManager.AddAffinity(npcData.npcId, currentLine.affinityDelta);
        appliedAffinityThisLine = true;
    }

    private void EnsureRuntimeReferences()
    {
        if (Player.instance == null)
            return;

        if (questManager == null)
            questManager = Player.instance.questManager;

        if (affinityManager == null)
            affinityManager = Player.instance.npcAffinityManager;

        if (inventory == null)
            inventory = Player.instance.inventory;
    }

    private void RefreshAffinityUI()
    {
        if (affinityRoot == null)
            return;

        if (affinityManager == null || npcData == null || string.IsNullOrWhiteSpace(npcData.npcId))
        {
            HideAffinityUI();
            return;
        }

        affinityRoot.SetActive(true);

        if (affinityFillImage != null)
            affinityFillImage.fillAmount = affinityManager.GetNormalizedAffinity(npcData.npcId);

        if (affinityValueText != null)
        {
            int affinity = affinityManager.GetAffinity(npcData.npcId);
            affinityValueText.text = affinity.ToString("+0;-0;0");
        }
    }

    private void HideAffinityUI()
    {
        if (affinityRoot != null)
            affinityRoot.SetActive(false);
    }

    private void HandleNextAction()
    {
        switch(currentLine.actionType)
        {
            case DialogueActionType.OpenShop:
                ui.SwitchToIngameUI();
                ui.ToggleMerchant(true);
                break;

            case DialogueActionType.PlayerMakeChoice:
                if (currentChoices == null || currentChoices.Length == 0)
                {
                    ui.SwitchToIngameUI();
                    break;
                }

                if(selectedChoice == null)
                {
                    ShowChoices();
                }
                else
                {
                    DialogueLineSO nextChoice = selectedChoice;
                    selectedChoice = null;
                    PlayDialogueLine(nextChoice);
                }
                break;
            case DialogueActionType.OpenQuest:
                ui.SwitchToIngameUI();
                ui.ToggleQuestUI(npcData.quests);
                break;
            case DialogueActionType.GetQuestReward:
                ui.SwitchToIngameUI();
                questManager.TryGetRewardFrom(npcData.npcRewardType, npcData != null ? npcData.npcId : null);
                break;
            case DialogueActionType.OpenCraft:
                ui.SwitchToIngameUI();
                ui.ToggleCraftUI(true);
                break;
            case DialogueActionType.OpenStorage:
                ui.SwitchToIngameUI();
                ui.ToggleStorage(true);
                break;
            case DialogueActionType.CloseDialogue:
                ui.SwitchToIngameUI();
                break;


        }
    }

    public void DialogueInteraction()
    {
        if (canInteract == false)
            return;

        if(typeTextCo != null)
        {
            CompleteTyping();
            if (currentLine.actionType != DialogueActionType.PlayerMakeChoice)
                waitingForConfirm = true; 
            else
                HandleNextAction();
           
            return;
        }
        if(waitingForConfirm)
        {
            waitingForConfirm = false;
            HandleNextAction();
            return;
        }

        if (currentLine.actionType == DialogueActionType.PlayerMakeChoice && selectedChoice != null)
        {
            HandleNextAction();
        }
    }
    private void CompleteTyping()
    {
        if (typeTextCo != null)
        {
            StopCoroutine(typeTextCo);
            dialogueText.text = fullTextToShow;
            typeTextCo = null;
        }
    }

    private void ShowChoices()
    {
        if (currentChoices == null || currentChoices.Length == 0)
        {
            HideAllChoices();
            selectedChoice = null;
            return;
        }

        selectedChoiceIndex = Mathf.Clamp(selectedChoiceIndex, 0, currentChoices.Length - 1);

        for(int i = 0; i < dialogueChoicesText.Length; i++)
        {
            if(i < currentChoices.Length)
            {
                DialogueLineSO choice = currentChoices[i];
                string choiceText = choice.playerChoiceAnswer;


                dialogueChoicesText[i].gameObject.SetActive(true);
                dialogueChoicesText[i].text = selectedChoiceIndex == i ?
                   $"<color=yellow> {i + 1}) {choiceText}" :
                   $"{i + 1}) {choiceText}";
            }
            else
            {
                dialogueChoicesText[i].gameObject.SetActive(false);
            }
        }

        selectedChoice = currentChoices[selectedChoiceIndex];
    }

    private void HideAllChoices()
    {
        foreach(var obj in dialogueChoicesText)
        {
            obj.gameObject.SetActive(false);
        }

    }
    public void NavigateChoice(int direction)
    {
        if (currentChoices == null || currentChoices.Length <= 1)
            return;

        selectedChoiceIndex += direction;
        selectedChoiceIndex = Mathf.Clamp(selectedChoiceIndex, 0, currentChoices.Length - 1);
        ShowChoices();

    }

    private IEnumerator TypeTextCo(string text)
    {
        dialogueText.text = "";

        foreach(char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }
        if(currentLine.actionType!=DialogueActionType.PlayerMakeChoice)
        { waitingForConfirm = true; }
        
        else
        {
            yield return new WaitForSeconds(.2f);
            selectedChoice = null;
            HandleNextAction();
        }
        typeTextCo = null;
    }
    private IEnumerator EnableInteractionCo()
    {
        yield return null;
        canInteract = true;
    }

    private void OnDisable()
    {
        if (typeTextCo != null)
        {
            StopCoroutine(typeTextCo);
            typeTextCo = null;
        }

        currentLine = null;
        currentChoices = null;
        selectedChoice = null;
        waitingForConfirm = false;
        canInteract = false;
        HideAllChoices();
        HideAffinityUI();
    }

}
