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
        
        for (int i = 0; i < objectives.Length; i++)
            if (objectives[i].isCompleted != true)
            {
                return false;
                Debug.Log("Нет");
            }

        Debug.Log("Квест сдан все круто");
        return true;

    }

}



