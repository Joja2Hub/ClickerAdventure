using UnityEngine;

[CreateAssetMenu(fileName = "KillObjective", menuName = "Quests/Objectives/Kill Enemy")]
public class KillEnemyObjective : QuestObjective
{
    public int targetCount;
    public int currentCount;

    public override void Initialize()
    {
        Debug.Log("����� �������");
        currentCount = 0;
        isCompleted = false;
    }

    public void OnEnemyKilled()
    {
        currentCount++;
        Debug.Log("���� ���� � ��� � ������" + currentCount);
        if (currentCount >= targetCount)
        {
            isCompleted = true;
        }
    }

    public override void CheckProgress()
    {
        
    }
}
