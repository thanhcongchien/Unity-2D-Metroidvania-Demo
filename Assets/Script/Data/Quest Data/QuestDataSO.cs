using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum RewardType { Merchant, Blacksmith, None }
public enum QuestType { Kill,Talk,Delivery }

[CreateAssetMenu(menuName = "RPG Setup/Quest Data/New Quest", fileName = "Quest - ")]
public class QuestDataSO : ScriptableObject
{
    public string questSaveID;
    [Space]
    public QuestType questType;
    public string questName;
    [TextArea] public string description;
    [TextArea] public string questGoal;


    public string questTargetId;
    public int requiredAmount;
    public ItemDataSo itemToDeliver;

    [Header("Quest Chain")]
    public QuestDataSO[] prerequisiteQuests;

    [Header("Reward")]
    public RewardType rewardType;
    public InventoryItem[] rewardItems;

    [Header("Affinity Reward")]
    public int affinityRewardAmount;
    public string affinityRewardNpcId;


    private void OnValidate()
    {

#if UNITY_EDITOR        
        string path = AssetDatabase.GetAssetPath(this);
        questSaveID = AssetDatabase.AssetPathToGUID(path);
#endif

        if (affinityRewardNpcId != null)
            affinityRewardNpcId = affinityRewardNpcId.Trim();
    }
}
