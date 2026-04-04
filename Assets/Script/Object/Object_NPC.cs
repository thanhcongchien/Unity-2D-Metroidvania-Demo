using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_NPC : MonoBehaviour,IInteractable
{
    protected Transform player;
    protected UI ui;
    protected Player_QuestManager questManager;
    protected Player_NpcAffinityManager npcAffinityManager;

    [Header("NPC Id")]
    [SerializeField] private string npcId;

    [Header("Quest Info")]
    [SerializeField] private string npcTargetQuestId;
    [SerializeField] protected RewardType rewardNpc;
    [Space]
    [SerializeField] private Transform npc;
    [SerializeField] private GameObject interactToolTip;
    private bool facingRight = true;

    [Header("float ToolTip")]
    [SerializeField] private float floatSpeed = 8f;
    [SerializeField] private float floatRange = .1f;
    private Vector3 startPos;

    protected virtual void Awake()
    {
        ui = FindFirstObjectByType<UI>();
        startPos = interactToolTip.transform.position;
        interactToolTip.SetActive(false);
    }

    protected virtual void Start()
    {
        questManager = Player.instance.questManager;
        npcAffinityManager = Player.instance.npcAffinityManager;
    }

    protected virtual void Update()
    {
        HandleNpcFlip();
        HandleToolTipFloat();
    }
    private void HandleToolTipFloat()
    {
        if(interactToolTip.activeSelf)
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
            interactToolTip.transform.position = startPos + new Vector3(0, yOffset);
        }
    }

    private void HandleNpcFlip()
    {
        if(player==null||npc==null)
            return;
        if(npc.position.x>player.position.x&&facingRight)
        {
            npc.transform.Rotate(0,180,0);
            facingRight = false;
        }
        else if(npc.position.x<player.position.x&&!facingRight)
        {
            npc.transform.Rotate(0, 180, 0);
            facingRight = true;
        }
    }


    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        player = collision.transform;
        interactToolTip.SetActive(true);
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        interactToolTip.SetActive(false);
    }

    public virtual void Interact()
    {
        questManager.AddProgress(npcTargetQuestId);
       //questManager.TryGetRewardFrom(rewardNpc);
    }

    protected bool IsDialogueLineAvailable(DialogueLineSO line, InventoryPlayer inventory = null)
    {
        if (line == null)
            return false;

        return line.IsAvailable(questManager, npcAffinityManager, inventory, GetNpcId());
    }

    private bool HasQuestDialogueCondition(DialogueLineSO line)
    {
        return line != null
            && line.questStateCondition != DialogueQuestStateCondition.Ignore
            && line.questToCheck != null;
    }

    private DialogueLineSO GetFirstAvailablePriorityLine(DialogueLineSO[] priorityDialogueLines, InventoryPlayer inventory, bool questConditionOnly)
    {
        if (priorityDialogueLines == null)
            return null;

        foreach (var line in priorityDialogueLines)
        {
            if (line == null)
                continue;

            if (HasQuestDialogueCondition(line) != questConditionOnly)
                continue;

            if (IsDialogueLineAvailable(line, inventory))
                return line;
        }

        return null;
    }

    protected DialogueLineSO ResolveDialogueLine(DialogueLineSO firstDialogueLine, DialogueLineSO[] priorityDialogueLines, InventoryPlayer inventory = null)
    {
        DialogueLineSO questPriorityLine = GetFirstAvailablePriorityLine(priorityDialogueLines, inventory, true);
        if (questPriorityLine != null)
            return questPriorityLine;

        if (HasQuestDialogueCondition(firstDialogueLine) && IsDialogueLineAvailable(firstDialogueLine, inventory))
            return firstDialogueLine;

        DialogueLineSO genericPriorityLine = GetFirstAvailablePriorityLine(priorityDialogueLines, inventory, false);
        if (genericPriorityLine != null)
            return genericPriorityLine;

        if (IsDialogueLineAvailable(firstDialogueLine, inventory))
            return firstDialogueLine;

        if (priorityDialogueLines != null)
        {
            foreach (var line in priorityDialogueLines)
            {
                if (line != null)
                    return line;
            }
        }

        return firstDialogueLine;
    }

    protected DialogueNPCData CreateDialogueNpcData(QuestDataSO[] quests)
    {
        return new DialogueNPCData(GetNpcId(), rewardNpc, quests);
    }

    public string GetNpcId()
    {
        if (string.IsNullOrWhiteSpace(npcId))
            return GetFallbackNpcId();

        return npcId;
    }

    private string GetFallbackNpcId()
    {
        if (this is Object_Blacksmith)
            return "Blacksmith";

        if (this is Object_Merchant)
            return "Merchant";

        return GenerateDefaultNpcId();
    }

    private string GenerateDefaultNpcId()
    {
        string hierarchyPath = gameObject.name;
        Transform current = transform.parent;

        while (current != null)
        {
            hierarchyPath = $"{current.name}/{hierarchyPath}";
            current = current.parent;
        }

        return $"{gameObject.scene.name}:{hierarchyPath}";
    }

    private void OnValidate()
    {
        if (npcId != null)
            npcId = npcId.Trim();
    }
}
