using UnityEngine;

[CreateAssetMenu(fileName = "KillObjective", menuName = "Quests/Objectives/Kill Enemy")]
public class KillEnemyObjective : QuestObjective
{
    public string enemyID;  // Уникальный идентификатор врага (например "wolf")
    public int targetCount;
    private int currentCount;

    public override void Initialize()
    {
        currentCount = 0;
        isCompleted = false;
    }

    public void OnEnemyKilled()
    {
        currentCount++;
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
