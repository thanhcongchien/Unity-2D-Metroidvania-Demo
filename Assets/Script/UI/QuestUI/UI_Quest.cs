using System;
using UnityEngine;

public class UI_Quest : MonoBehaviour, ISaveable
{
    [SerializeField] private UI_ItemSlotParent inventorySlots;
    [SerializeField] private UI_QuestPreview questPreview;

    private UI_QuestSlot[] questSlots;
    public Player_QuestManager questManager { get; private set; }

    private void Awake()
    {
        questSlots = GetComponentsInChildren<UI_QuestSlot>(true);
        questManager = Player.instance.questManager;
    }

    public void SetupQuestUI(QuestDataSO[] questsToSetup)
    {
        if (questsToSetup == null)
            questsToSetup = Array.Empty<QuestDataSO>();

        foreach (var slot in questSlots)
        {
            slot.gameObject.SetActive(false);
        }

        for (int i = 0; i < questsToSetup.Length; i++)
        {
            questSlots[i].gameObject.SetActive(true);
            questSlots[i].SetupQuestSlot(questsToSetup[i]);
        }

        questPreview.CleanUpQuestPreview();
        inventorySlots.UpdateSlots(Player.instance.inventory.itemList);
        UpdateQuestList();
    }

    public void UpdateQuestList()
    {
        foreach (var slot in questSlots)
        {
            if (slot.questInSlot == null)
                continue;

            if (slot.gameObject.activeSelf && CanTakeQuest(slot.questInSlot) == false)
                slot.gameObject.SetActive(false);
        }
    }

    private bool CanTakeQuest(QuestDataSO questToCheck)
    {
        return questManager.CanAcceptQuest(questToCheck);
    }

    public UI_QuestPreview GetQuestPreview()
    {
        return questPreview;
    }

    public void LoadData(GameData data)
    {
    }

    public void SaveData(ref GameData data)
    {
    }
}
