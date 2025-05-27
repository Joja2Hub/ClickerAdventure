using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SerializableQuestObjective
{
    public string objectiveName;
    public bool isCompleted;
}

[System.Serializable]
public class SerializableQuestData
{
    public string questName;
    public string description;
    public int rewardGold;
    public int rewardXP;
    public int hardReward;


    public QuestData ToQuestData()
    {
        var quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questName = questName;
        quest.description = description;
        quest.rewardGold = rewardGold;
        quest.rewardXP = rewardXP;
        quest.hardReward = hardReward;
        quest.objectives = new QuestObjective[0];

        return quest;
    }
}