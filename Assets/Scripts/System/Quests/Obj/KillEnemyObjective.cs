using UnityEngine;

[CreateAssetMenu(fileName = "KillObjective", menuName = "Quests/Objectives/Kill Enemy")]
public class KillEnemyObjective : QuestObjective
{
    public int targetCount;
    public int currentCount;

    public override void Initialize()
    {
        Debug.Log(" вест обнулен");
        currentCount = 0;
        isCompleted = false;
    }

    public void OnEnemyKilled()
    {
        currentCount++;
        Debug.Log("¬раг умер и это в квесте" + currentCount);
        if (currentCount >= targetCount)
        {
            isCompleted = true;
        }
    }

    public override void CheckProgress()
    {
        
    }
}
