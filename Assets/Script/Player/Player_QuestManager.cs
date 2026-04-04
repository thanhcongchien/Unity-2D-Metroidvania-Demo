using System.Collections.Generic;
using UnityEngine;

public class Player_QuestManager : MonoBehaviour, ISaveable
{
    public List<QuestData> activeQuests;
    public List<QuestData> completedQuests;

    [SerializeField] private QuestDatabaseSO questDatabase;
    private Entity_DropManager dropManager;
    private InventoryPlayer inventory;
    private Player_NpcAffinityManager npcAffinityManager;

    private void Awake()
    {
        dropManager = GetComponent<Entity_DropManager>();
        inventory = GetComponent<InventoryPlayer>();
        npcAffinityManager = GetComponent<Player_NpcAffinityManager>();

        if (activeQuests == null)
            activeQuests = new List<QuestData>();

        if (completedQuests == null)
            completedQuests = new List<QuestData>();
    }

    public void TryGetRewardFrom(RewardType npcType, string rewardNpcId = null)
    {
        List<QuestData> getRewardQuests = new List<QuestData>();

        foreach (var quest in activeQuests)
        {
            if (quest.questDataSO.questType == QuestType.Delivery)
            {
                var requiredItem = quest.questDataSO.itemToDeliver;
                var requiredAmount = quest.questDataSO.requiredAmount;

                if (inventory.HasItemAmount(requiredItem, requiredAmount))
                {
                    inventory.RemoveItemAmount(requiredItem, requiredAmount);
                    quest.AddQuestProgress(requiredAmount);
                }
            }

            if (quest.questDataSO.rewardType == npcType && quest.CanGetReward())
                getRewardQuests.Add(quest);
        }

        foreach (var quest in getRewardQuests)
        {
            GiveQuestReward(quest.questDataSO, rewardNpcId);
            CompleteQuest(quest);
        }
    }

    private void GiveQuestReward(QuestDataSO questDataSO, string rewardNpcId = null)
    {
        if (questDataSO == null)
            return;

        GiveQuestAffinityReward(questDataSO, rewardNpcId);

        if (questDataSO.rewardItems == null)
            return;

        foreach (var item in questDataSO.rewardItems)
        {
            if (item == null || item.itemData == null)
                continue;

            for (int i = 0; i < item.stackSize; i++)
            {
                dropManager.CreateItemDrop(item.itemData);
            }
        }
    }

    public bool HasCompletedQuest()
    {
        for (int i = 0; i < activeQuests.Count; i++)
        {
            QuestData quest = activeQuests[i];
            if (quest.questDataSO.questType == QuestType.Delivery)
            {
                var requiredItem = quest.questDataSO.itemToDeliver;
                var requiredAmount = quest.questDataSO.requiredAmount;

                if (inventory.HasItemAmount(requiredItem, requiredAmount))
                    return true;
            }

            if (quest.CanGetReward())
                return true;
        }

        return false;
    }

    public void AddProgress(string questTargetId, int amount = 1)
    {
        List<QuestData> getRewardQuests = new List<QuestData>();

        foreach (var quest in activeQuests)
        {
            if (quest.questDataSO.questTargetId != questTargetId)
                continue;

            if (quest.CanGetReward() == false)
                quest.AddQuestProgress(amount);

            if (quest.questDataSO.rewardType == RewardType.None && quest.CanGetReward())
                getRewardQuests.Add(quest);
        }

        foreach (var quest in getRewardQuests)
        {
            GiveQuestReward(quest.questDataSO);
            CompleteQuest(quest);
        }
    }

    private void GiveQuestAffinityReward(QuestDataSO questDataSO, string rewardNpcId = null)
    {
        if (questDataSO == null)
            return;

        if (npcAffinityManager == null)
            npcAffinityManager = GetComponent<Player_NpcAffinityManager>();

        if (npcAffinityManager == null && Player.instance != null)
            npcAffinityManager = Player.instance.npcAffinityManager;

        if (npcAffinityManager == null)
            return;

        if (questDataSO.affinityRewardAmount == 0)
            return;

        string targetNpcId = ResolveAffinityRewardNpcId(questDataSO, rewardNpcId);
        if (string.IsNullOrWhiteSpace(targetNpcId))
            return;

        npcAffinityManager.AddAffinity(targetNpcId, questDataSO.affinityRewardAmount);
    }

    private string ResolveAffinityRewardNpcId(QuestDataSO questDataSO, string rewardNpcId)
    {
        if (string.IsNullOrWhiteSpace(rewardNpcId) == false)
            return rewardNpcId.Trim();

        if (string.IsNullOrWhiteSpace(questDataSO.affinityRewardNpcId) == false)
            return questDataSO.affinityRewardNpcId.Trim();

        return null;
    }

    public int GetQuestProgress(QuestData questToCheck)
    {
        QuestData quest = activeQuests.Find(q => q == questToCheck);
        return quest != null ? quest.currentAmount : 0;
    }

    public void AcceptQuest(QuestDataSO questDataSO)
    {
        if (CanAcceptQuest(questDataSO) == false)
            return;

        activeQuests.Add(new QuestData(questDataSO));
    }

    public void CompleteQuest(QuestData questData)
    {
        if (questData == null || questData.questDataSO == null)
            return;

        if (QuestIsCompleted(questData.questDataSO) == false)
            completedQuests.Add(questData);

        activeQuests.Remove(questData);
    }

    public bool QuestIsActive(QuestDataSO questToCheck)
    {
        if (questToCheck == null)
            return false;

        return activeQuests.Find(q => q.questDataSO == questToCheck) != null;
    }

    public bool QuestIsCompleted(QuestDataSO questToCheck)
    {
        if (questToCheck == null)
            return false;

        return completedQuests.Find(q => q.questDataSO == questToCheck) != null;
    }

    public bool ArePrerequisitesMet(QuestDataSO questToCheck)
    {
        if (questToCheck == null)
            return false;

        if (questToCheck.prerequisiteQuests == null || questToCheck.prerequisiteQuests.Length == 0)
            return true;

        foreach (var prerequisiteQuest in questToCheck.prerequisiteQuests)
        {
            if (prerequisiteQuest == null)
                continue;

            if (QuestIsCompleted(prerequisiteQuest) == false)
                return false;
        }

        return true;
    }

    public bool CanAcceptQuest(QuestDataSO questToCheck)
    {
        if (questToCheck == null)
            return false;

        return QuestIsActive(questToCheck) == false
            && QuestIsCompleted(questToCheck) == false
            && ArePrerequisitesMet(questToCheck);
    }

    public void LoadData(GameData data)
    {
        activeQuests.Clear();
        completedQuests.Clear();

        foreach (var entry in data.activeQuests)
        {
            string questSaveId = entry.Key;
            int progress = entry.Value;

            QuestDataSO questDataSO = questDatabase.GetQuestById(questSaveId);
            if (questDataSO == null)
                continue;

            QuestData questToLoad = new QuestData(questDataSO);
            questToLoad.currentAmount = progress;
            activeQuests.Add(questToLoad);
        }

        foreach (var entry in data.completedQuests)
        {
            if (entry.Value == false)
                continue;

            string questSaveId = entry.Key;
            QuestDataSO questDataSO = questDatabase.GetQuestById(questSaveId);
            if (questDataSO == null)
                continue;

            QuestData questToLoad = new QuestData(questDataSO);
            questToLoad.currentAmount = questDataSO.requiredAmount;
            completedQuests.Add(questToLoad);
        }
    }

    public void SaveData(ref GameData data)
    {
        data.activeQuests.Clear();
        data.completedQuests.Clear();

        foreach (var quest in activeQuests)
        {
            data.activeQuests[quest.questDataSO.questSaveID] = quest.currentAmount;
        }

        foreach (var quest in completedQuests)
        {
            data.completedQuests[quest.questDataSO.questSaveID] = true;
        }
    }
}
