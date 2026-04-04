using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNPCData
{
    public string npcId;
    public RewardType npcRewardType;
    public QuestDataSO[] quests;

    public DialogueNPCData(string npcId, RewardType npcRewardType, QuestDataSO[] quests)
    {
        this.npcId = npcId;
        this.npcRewardType = npcRewardType;
        this.quests = quests;
    }
}
