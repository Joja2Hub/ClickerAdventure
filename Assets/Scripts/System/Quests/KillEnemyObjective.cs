using UnityEngine;

[CreateAssetMenu(fileName = "KillObjective", menuName = "Quests/Objectives/Kill Enemy")]
public class KillEnemyObjective : QuestObjective
{
    public string enemyID;  // ���������� ������������� ����� (�������� "wolf")
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
        // ����� ������������ ���� ���� ������� ��������� ����������
    }
}
