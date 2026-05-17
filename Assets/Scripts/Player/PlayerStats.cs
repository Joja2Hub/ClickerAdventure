using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    private const string SaveKey = "PlayerStats";

    public int level = 1;
    public int currentExperience = 0;
    public int experienceToNextLevel = 100;
    public int money = 100;

    public int currentHealth = 100;
    public int maxHealth = 100;
    public int currentDmg = 1;
    public int damageUpgradeLevel = 0;
    public int healthUpgradeLevel = 0;

    public event Action<int> OnMoneyChanged;
    public event Action<int> OnLevelChanged;
    public event Action<int, int> OnExperienceChanged;
    public event Action<int, int> OnHealthChanged;
    public event Action OnStatsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
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
        Save();
    }

    public void IncreaseLevel()
    {
        level++;
        CalculateExperienceForNextLevel();
        OnLevelChanged?.Invoke(level);
        Save();
    }

    public void AddExperience(int amount)
    {
        currentExperience += amount;
        while (currentExperience >= experienceToNextLevel)
        {
            LevelUp();
        }

        OnExperienceChanged?.Invoke(currentExperience, experienceToNextLevel);
        Save();
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
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Save();
    }

    public void SetHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Save();
    }

    public void HealToFull()
    {
        SetHealth(maxHealth);
    }

    public bool TrySpendMoney(int amount)
    {
        if (amount <= 0)
            return true;

        if (money < amount)
            return false;

        money -= amount;
        OnMoneyChanged?.Invoke(money);
        Save();
        return true;
    }

    public int GetDamageUpgradeCost()
    {
        return Mathf.RoundToInt(60 * Mathf.Pow(1.35f, damageUpgradeLevel));
    }

    public int GetHealthUpgradeCost()
    {
        return Mathf.RoundToInt(70 * Mathf.Pow(1.35f, healthUpgradeLevel));
    }

    public bool UpgradeDamage()
    {
        int cost = GetDamageUpgradeCost();
        if (!TrySpendMoney(cost))
            return false;

        damageUpgradeLevel++;
        currentDmg += 1 + damageUpgradeLevel / 3;
        OnStatsChanged?.Invoke();
        Save();
        return true;
    }

    public bool UpgradeMaxHealth()
    {
        int cost = GetHealthUpgradeCost();
        if (!TrySpendMoney(cost))
            return false;

        healthUpgradeLevel++;
        maxHealth += 10;
        currentHealth = Mathf.Min(maxHealth, currentHealth + 10);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnStatsChanged?.Invoke();
        Save();
        return true;
    }

    public void Save()
    {
        var saveData = new PlayerStatsSaveData
        {
            level = level,
            currentExperience = currentExperience,
            experienceToNextLevel = experienceToNextLevel,
            money = money,
            currentHealth = currentHealth,
            maxHealth = maxHealth,
            currentDmg = currentDmg,
            damageUpgradeLevel = damageUpgradeLevel,
            healthUpgradeLevel = healthUpgradeLevel
        };

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saveData));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        var saveData = JsonUtility.FromJson<PlayerStatsSaveData>(PlayerPrefs.GetString(SaveKey));
        if (saveData == null)
            return;

        level = saveData.level;
        currentExperience = saveData.currentExperience;
        experienceToNextLevel = saveData.experienceToNextLevel;
        money = saveData.money;
        currentHealth = saveData.currentHealth;
        maxHealth = saveData.maxHealth;
        currentDmg = saveData.currentDmg;
        damageUpgradeLevel = saveData.damageUpgradeLevel;
        healthUpgradeLevel = saveData.healthUpgradeLevel;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            Save();
    }

    private void OnApplicationQuit()
    {
        Save();
    }
}
