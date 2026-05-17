using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class QuestData : ScriptableObject
{
    public string questName;
    [TextArea] public string description;
    public int rewardGold;
    public int rewardXP;
    public int hardReward;

    public QuestObjective[] objectives;

    public bool CheckReady()
    {
        if (QuestManager.Instance != null)
            return QuestManager.Instance.IsQuestReady(this);

        if (objectives == null || objectives.Length == 0)
            return false;

        foreach (var objective in objectives)
        {
            if (objective == null || !objective.isCompleted)
                return false;
        }

        return true;
    }
}
