using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName; // Имя монстра
    public int health; // Здоровье монстра
    public int damage; // Урон монстра
    public float attackSpeed; // Скорость атаки (в секундах)

    [Header("Награда за победу")]
    public int rewardMoneyMin = 10;
    public int rewardMoneyMax = 20;
    public int rewardExpMin = 5;
    public int rewardExpMax = 10;

    // Метод для получения случайной награды
    public int GetRandomMoneyReward()
    {
        return Random.Range(rewardMoneyMin, rewardMoneyMax + 1);
    }

    public int GetRandomExpReward()
    {
        return Random.Range(rewardExpMin, rewardExpMax + 1);
    }
}