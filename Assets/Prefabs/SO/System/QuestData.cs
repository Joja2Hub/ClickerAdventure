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

}
