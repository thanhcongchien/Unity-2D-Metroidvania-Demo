using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_ActiveQuestPreview : MonoBehaviour
{
   private Player_QuestManager questManager;


    [SerializeField] private TextMeshProUGUI questName;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI progress;
    [SerializeField] private UI_QuestRewardSlot[] questRewardSlots;

    public void SetupQuestPreview(QuestData questData)
    {
        questManager = Player.instance.questManager;
        QuestDataSO questSo = questData.questDataSO;

        questName.text = questSo.questName;
        description.text = questSo.description;

        progress.text = questSo.questGoal + questManager.GetQuestProgress(questData).ToString() + " / " + questSo.requiredAmount;

        foreach(var obj in questRewardSlots)
        {
            obj.gameObject.SetActive(false);
        }

        for(int i = 0; i < questSo.rewardItems.Length; i++)
        {
            questRewardSlots[i].gameObject.SetActive(true);
            questRewardSlots[i].UpdateSlot(questSo.rewardItems[i]);
        }


    }
}
