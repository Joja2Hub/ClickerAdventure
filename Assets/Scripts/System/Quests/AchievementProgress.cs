using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementProgress : MonoBehaviour
{
    private static AchievementProgress instance;
    private const string SaveKey = "AchievementProgress";

    public event Action OnProgressChanged;

    private readonly AchievementDefinition[] achievements =
    {
        new AchievementDefinition("first_real_task", "First real task", "Completed your first real-life task.", 1, 0, 25, 10),
        new AchievementDefinition("five_real_tasks", "Helpful hero", "Completed 5 real-life tasks.", 5, 0, 70, 35),
        new AchievementDefinition("ten_real_tasks", "Routine champion", "Completed 10 real-life tasks.", 10, 0, 140, 80),
        new AchievementDefinition("three_day_streak", "Three-day streak", "Kept your routine going for 3 days.", 0, 3, 90, 60),
        new AchievementDefinition("seven_day_streak", "Seven-day streak", "Kept your routine going for a full week.", 0, 7, 220, 140)
    };

    private AchievementSaveData saveData = new AchievementSaveData();

    public static AchievementProgress Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindFirstObjectByType<AchievementProgress>();
            if (instance != null)
                return instance;

            GameObject progressObject = new GameObject("AchievementProgress");
            instance = progressObject.AddComponent<AchievementProgress>();
            DontDestroyOnLoad(progressObject);
            return instance;
        }
    }

    public int TotalRealTasks => saveData.totalRealTasks;
    public int UnlockedCount => saveData.unlockedAchievementIds.Count;
    public int TotalAchievements => achievements.Length;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public AchievementRewardResult RecordRealTaskClaim(DailyRoutineRewardResult routineReward)
    {
        saveData.totalRealTasks++;

        List<string> unlockedTitles = new List<string>();
        int bonusGold = 0;
        int bonusExperience = 0;

        foreach (AchievementDefinition achievement in achievements)
        {
            if (saveData.unlockedAchievementIds.Contains(achievement.Id))
                continue;

            if (!achievement.IsUnlocked(saveData.totalRealTasks, routineReward.CurrentStreak))
                continue;

            saveData.unlockedAchievementIds.Add(achievement.Id);
            unlockedTitles.Add(achievement.Title);
            bonusGold += achievement.GoldReward;
            bonusExperience += achievement.ExperienceReward;
        }

        Save();
        OnProgressChanged?.Invoke();

        if (unlockedTitles.Count <= 0)
            return AchievementRewardResult.None(saveData.totalRealTasks, UnlockedCount, TotalAchievements);

        string title = unlockedTitles.Count == 1 ? unlockedTitles[0] : "Achievements unlocked";
        string description = string.Join(", ", unlockedTitles);

        return new AchievementRewardResult(
            title,
            description,
            bonusGold,
            bonusExperience,
            saveData.totalRealTasks,
            UnlockedCount,
            TotalAchievements);
    }

    public AchievementDefinition[] GetAchievements()
    {
        return (AchievementDefinition[])achievements.Clone();
    }

    public bool IsUnlocked(string id)
    {
        return saveData.unlockedAchievementIds.Contains(id);
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        AchievementSaveData loaded = JsonUtility.FromJson<AchievementSaveData>(PlayerPrefs.GetString(SaveKey));
        if (loaded != null)
            saveData = loaded;

        if (saveData.unlockedAchievementIds == null)
            saveData.unlockedAchievementIds = new List<string>();
    }

    private void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saveData));
        PlayerPrefs.Save();
    }
}

public readonly struct AchievementRewardResult
{
    public AchievementRewardResult(string title, string description, int bonusGold, int bonusExperience, int totalRealTasks, int unlockedCount, int totalAchievements)
    {
        Title = title;
        Description = description;
        BonusGold = bonusGold;
        BonusExperience = bonusExperience;
        TotalRealTasks = totalRealTasks;
        UnlockedCount = unlockedCount;
        TotalAchievements = totalAchievements;
    }

    public string Title { get; }
    public string Description { get; }
    public int BonusGold { get; }
    public int BonusExperience { get; }
    public int TotalRealTasks { get; }
    public int UnlockedCount { get; }
    public int TotalAchievements { get; }
    public bool HasReward => !string.IsNullOrEmpty(Title);

    public static AchievementRewardResult None(int totalRealTasks, int unlockedCount, int totalAchievements)
    {
        return new AchievementRewardResult(string.Empty, string.Empty, 0, 0, totalRealTasks, unlockedCount, totalAchievements);
    }
}

[Serializable]
public class AchievementSaveData
{
    public int totalRealTasks;
    public List<string> unlockedAchievementIds = new List<string>();
}

public readonly struct AchievementDefinition
{
    public AchievementDefinition(string id, string title, string description, int requiredRealTasks, int requiredStreak, int goldReward, int experienceReward)
    {
        Id = id;
        Title = title;
        Description = description;
        RequiredRealTasks = requiredRealTasks;
        RequiredStreak = requiredStreak;
        GoldReward = goldReward;
        ExperienceReward = experienceReward;
    }

    public string Id { get; }
    public string Title { get; }
    public string Description { get; }
    public int RequiredRealTasks { get; }
    public int RequiredStreak { get; }
    public int GoldReward { get; }
    public int ExperienceReward { get; }

    public bool IsUnlocked(int totalRealTasks, int currentStreak)
    {
        bool taskRequirementMet = RequiredRealTasks <= 0 || totalRealTasks >= RequiredRealTasks;
        bool streakRequirementMet = RequiredStreak <= 0 || currentStreak >= RequiredStreak;
        return taskRequirementMet && streakRequirementMet;
    }
}
