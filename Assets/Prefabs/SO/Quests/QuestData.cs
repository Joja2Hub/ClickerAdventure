using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class QuestData : ScriptableObject
{
    public string questName;
    [TextArea] public string description;
    public int rewardGold;
    public int rewardXP;
    public int hardReward;

    // Цели внутриигрового квеста
    public QuestObjective[] objectives;

    public bool CheckReady()
    {
        foreach (var objective in objectives)
        {
            if (!objective.isCompleted)
            {
                return false;
            }
        }
        Debug.Log("Квест завершен: " + questName);
        return true;
    }
}