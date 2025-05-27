using UnityEngine;

[System.Serializable]
public class ExternalQuestData
{
    public string questName;
    public string description;
    public int rewardGold;
    public int rewardXP;
    public int hardReward;

    public bool isCompleted;

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