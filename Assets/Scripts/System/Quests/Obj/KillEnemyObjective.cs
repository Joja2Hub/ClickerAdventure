using UnityEngine;

[CreateAssetMenu(fileName = "KillObjective", menuName = "Quests/Objectives/Kill Enemy")]
public class KillEnemyObjective : QuestObjective
{
    public int targetCount;
    private int currentCount;

    public override void Initialize()
    {
        Debug.Log("Квест обнулен");
        currentCount = 0;
        isCompleted = false;
    }

    public void OnEnemyKilled()
    {
        currentCount++;
        Debug.Log("Враг умер и это в квесте" + currentCount);
        if (currentCount >= targetCount)
        {
            isCompleted = true;
        }
    }

    public override void CheckProgress()
    {
        // Можно использовать если надо вручную проверять выполнение
    }
}
