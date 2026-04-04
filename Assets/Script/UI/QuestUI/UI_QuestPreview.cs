using TMPro;
using UnityEngine;

public class UI_QuestPreview : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questName;
    [SerializeField] private TextMeshProUGUI questDescription;
    [SerializeField] private TextMeshProUGUI questGoal;
    [SerializeField] private UI_QuestRewardSlot[] questReward;

    [SerializeField] private GameObject[] additionalObjects;
    private UI_Quest questUI;
    private QuestDataSO previewQuest;

    public void SetupQuestPreview(QuestDataSO questDataSO)
    {
        questUI = transform.root.GetComponentInChildren<UI_Quest>();
        previewQuest = questDataSO;

        foreach (var obj in additionalObjects)
        {
            obj.SetActive(true);
        }

        questName.text = questDataSO.questName;
        questDescription.text = questDataSO.description;
        questGoal.text = questDataSO.questGoal + " " + questDataSO.requiredAmount;

        foreach (var obj in questReward)
        {
            obj.gameObject.SetActive(false);
        }

        for (int i = 0; i < questDataSO.rewardItems.Length; i++)
        {
            InventoryItem rewardItem = new InventoryItem(questDataSO.rewardItems[i].itemData);
            rewardItem.stackSize = questDataSO.rewardItems[i].stackSize;

            questReward[i].gameObject.SetActive(true);
            questReward[i].UpdateSlot(rewardItem);
        }
    }

    public void AcceptQuestBTN()
    {
        if (questUI.questManager.CanAcceptQuest(previewQuest) == false)
            return;

        CleanUpQuestPreview();
        questUI.questManager.AcceptQuest(previewQuest);
        questUI.UpdateQuestList();
    }

    public void CleanUpQuestPreview()
    {
        questName.text = "";
        questDescription.text = "";

        foreach (var obj in additionalObjects)
        {
            obj.SetActive(false);
        }

        foreach (var obj in questReward)
        {
            obj.gameObject.SetActive(false);
        }
    }
}
