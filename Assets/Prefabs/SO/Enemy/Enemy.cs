using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName; // ��� �������
    public int health; // �������� �������
    public int damage; // ���� �������
    public float attackSpeed; // �������� ����� (� ��������)

    [Header("������� �� ������")]
    public int rewardMoneyMin = 10;
    public int rewardMoneyMax = 20;
    public int rewardExpMin = 5;
    public int rewardExpMax = 10;

    // ����� ��� ��������� ��������� �������
    public int GetRandomMoneyReward()
    {
        return Random.Range(rewardMoneyMin, rewardMoneyMax + 1);
    }

    public int GetRandomExpReward()
    {
        return Random.Range(rewardExpMin, rewardExpMax + 1);
    }
}