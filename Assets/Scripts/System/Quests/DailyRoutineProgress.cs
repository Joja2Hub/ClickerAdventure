using System;
using UnityEngine;

public class DailyRoutineProgress : MonoBehaviour
{
    private static DailyRoutineProgress instance;
    private const string SaveKey = "DailyRoutineProgress";
    private const int DefaultDailyGoal = 3;

    public event Action OnProgressChanged;

    [SerializeField] private int dailyGoal = DefaultDailyGoal;

    private DailyRoutineSaveData saveData = new DailyRoutineSaveData();

    public static DailyRoutineProgress Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindFirstObjectByType<DailyRoutineProgress>();
            if (instance != null)
                return instance;

            GameObject progressObject = new GameObject("DailyRoutineProgress");
            instance = progressObject.AddComponent<DailyRoutineProgress>();
            DontDestroyOnLoad(progressObject);
            return instance;
        }
    }

    public int DailyGoal => Mathf.Max(1, dailyGoal);
    public int CompletedToday => saveData.completedToday;
    public int CurrentStreak => saveData.currentStreak;
    public bool HasClaimedGoalBonusToday => saveData.claimedGoalBonus;
    public float GoalProgress => Mathf.Clamp01((float)CompletedToday / DailyGoal);

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
        ResetForCurrentDayIfNeeded();
    }

    public DailyRoutineRewardResult RecordRealTaskClaim()
    {
        ResetForCurrentDayIfNeeded();

        if (saveData.completedToday == 0)
            UpdateStreakForToday();

        saveData.completedToday++;

        int bonusGold = 0;
        int bonusExperience = 0;
        bool completedDailyGoal = false;

        if (!saveData.claimedGoalBonus && saveData.completedToday >= DailyGoal)
        {
            completedDailyGoal = true;
            saveData.claimedGoalBonus = true;
            bonusGold = 25 + saveData.currentStreak * 5;
            bonusExperience = 20 + saveData.currentStreak * 3;
        }

        Save();
        OnProgressChanged?.Invoke();

        return new DailyRoutineRewardResult(
            saveData.completedToday,
            DailyGoal,
            saveData.currentStreak,
            bonusGold,
            bonusExperience,
            completedDailyGoal);
    }

    public void ResetForCurrentDayIfNeeded()
    {
        string today = GetTodayKey();
        if (saveData.currentDate == today)
            return;

        saveData.currentDate = today;
        saveData.completedToday = 0;
        saveData.claimedGoalBonus = false;

        if (!string.IsNullOrEmpty(saveData.lastCompletionDate) && !IsYesterday(saveData.lastCompletionDate))
            saveData.currentStreak = 0;

        Save();
        OnProgressChanged?.Invoke();
    }

    private void UpdateStreakForToday()
    {
        string today = GetTodayKey();
        if (saveData.lastCompletionDate == today)
            return;

        saveData.currentStreak = IsYesterday(saveData.lastCompletionDate)
            ? Mathf.Max(1, saveData.currentStreak + 1)
            : 1;

        saveData.lastCompletionDate = today;
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            saveData.currentDate = GetTodayKey();
            return;
        }

        DailyRoutineSaveData loaded = JsonUtility.FromJson<DailyRoutineSaveData>(PlayerPrefs.GetString(SaveKey));
        if (loaded != null)
            saveData = loaded;

        if (string.IsNullOrEmpty(saveData.currentDate))
            saveData.currentDate = GetTodayKey();
    }

    private void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saveData));
        PlayerPrefs.Save();
    }

    private string GetTodayKey()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }

    private bool IsYesterday(string dateKey)
    {
        if (string.IsNullOrEmpty(dateKey) || !DateTime.TryParse(dateKey, out DateTime date))
            return false;

        return date.Date == DateTime.Now.Date.AddDays(-1);
    }
}

public readonly struct DailyRoutineRewardResult
{
    public DailyRoutineRewardResult(int completedToday, int dailyGoal, int currentStreak, int bonusGold, int bonusExperience, bool completedDailyGoal)
    {
        CompletedToday = completedToday;
        DailyGoal = dailyGoal;
        CurrentStreak = currentStreak;
        BonusGold = bonusGold;
        BonusExperience = bonusExperience;
        CompletedDailyGoal = completedDailyGoal;
    }

    public int CompletedToday { get; }
    public int DailyGoal { get; }
    public int CurrentStreak { get; }
    public int BonusGold { get; }
    public int BonusExperience { get; }
    public bool CompletedDailyGoal { get; }
    public bool HasBonus => BonusGold > 0 || BonusExperience > 0;
}

[Serializable]
public class DailyRoutineSaveData
{
    public string currentDate;
    public string lastCompletionDate;
    public int completedToday;
    public int currentStreak;
    public bool claimedGoalBonus;
}
