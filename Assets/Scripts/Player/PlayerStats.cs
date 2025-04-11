using UnityEngine;
using System; // Для Action

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public int level = 1;
    public int currentExperience = 0;
    public int experienceToNextLevel = 100;
    public int money = 100;

    public int currentHealth = 100;
    public int maxHealth = 100;
    public int currentDmg = 1;

    // События
    public event Action<int> OnMoneyChanged;
    public event Action<int> OnLevelChanged;
    public event Action<int, int> OnExperienceChanged; // (текущий, до следующего)
    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddMoney(int amount)
    {
        money += amount;
        OnMoneyChanged?.Invoke(money);
    }

    public void IncreaseLevel()
    {
        level++;
        OnLevelChanged?.Invoke(level);
    }

    public void AddExperience(int amount)
    {
        currentExperience += amount;
        while (currentExperience >= experienceToNextLevel)
        {
            LevelUp();
        }

        OnExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);
    }

    private void LevelUp()
    {
        currentExperience -= experienceToNextLevel;
        level++;
        CalculateExperienceForNextLevel();
        OnLevelChanged?.Invoke(level);
        OnExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);

        Debug.Log($"Level up! You are now level {level}");
    }

    private void CalculateExperienceForNextLevel()
    {
        experienceToNextLevel = Mathf.RoundToInt(100 * Mathf.Pow(1.2f, level - 1));
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }


}
