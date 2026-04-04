using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueQuestStateCondition
{
    Ignore,
    NotAccepted,
    Active,
    Completed
}

[CreateAssetMenu(menuName = "RPG Setup/Dialogue Data/New Line Data", fileName = "Line - ")]
public class DialogueLineSO : ScriptableObject
{
    [Header("Dialogue Info")]
    public string dialogueGroupName;
    public DialogueSpeakerSO speaker;

    [Header("Text Options")]
    [TextArea] public string[] textLine;

    [Header("Choices Info")]
    [TextArea] public string playerChoiceAnswer;
    public DialogueLineSO[] choiceLines;

    //[Header("Answer Setup")]
    //public bool playerCanAnswer;
    //public DialogueLineSO[] answerLine;

    [Header("Dialogue Action")]
    [TextArea] public string actionLine;
    public DialogueActionType actionType;

    [Header("Availability Condition")]
    public bool useAffinityCondition;
    public int minAffinity = -100;
    public int maxAffinity = 100;
    public bool usePlayerGoldCondition;
    public int minPlayerGold;
    public int maxPlayerGold = 999999;
    public DialogueQuestStateCondition questStateCondition = DialogueQuestStateCondition.Ignore;
    public QuestDataSO questToCheck;

    [Header("Affinity Effect")]
    public int affinityDelta;

    public string GetFirstLine()
    {
        if (textLine == null || textLine.Length == 0)
            return string.Empty;

        return textLine[0];
    }

    public string GetRandomLine()
    {
        if (textLine == null || textLine.Length == 0)
            return string.Empty;

        return textLine[Random.Range(0, textLine.Length)];
    }

    public bool IsAvailable(Player_QuestManager questManager, Player_NpcAffinityManager affinityManager, string npcId)
    {
        InventoryPlayer inventory = Player.instance != null ? Player.instance.inventory : null;
        return IsAvailable(questManager, affinityManager, inventory, npcId);
    }

    public bool IsAvailable(Player_QuestManager questManager, Player_NpcAffinityManager affinityManager, InventoryPlayer inventory, string npcId)
    {
        if (useAffinityCondition)
        {
            if (affinityManager == null)
                return false;

            int currentAffinity = affinityManager.GetAffinity(npcId);
            if (currentAffinity < minAffinity || currentAffinity > maxAffinity)
                return false;
        }

        if (usePlayerGoldCondition)
        {
            if (inventory == null)
                return false;

            if (inventory.gold < minPlayerGold || inventory.gold > maxPlayerGold)
                return false;
        }

        return CheckQuestCondition(questManager);
    }

    private bool CheckQuestCondition(Player_QuestManager questManager)
    {
        if (questStateCondition == DialogueQuestStateCondition.Ignore)
            return true;

        if (questManager == null || questToCheck == null)
            return false;

        bool isActive = questManager.QuestIsActive(questToCheck);
        bool isCompleted = questManager.QuestIsCompleted(questToCheck);

        switch (questStateCondition)
        {
            case DialogueQuestStateCondition.NotAccepted:
                return isActive == false && isCompleted == false;

            case DialogueQuestStateCondition.Active:
                return isActive;

            case DialogueQuestStateCondition.Completed:
                return isCompleted;
        }

        return true;
    }
}
