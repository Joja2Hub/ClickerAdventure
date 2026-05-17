using System.Collections.Generic;
using System.Linq;
using Firebase;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    private const string SaveKey = "QuestManager";

    public List<QuestData> activeQuests = new List<QuestData>();
    public List<ExternalQuestData> externalQuestDatas = new List<ExternalQuestData>();

    private readonly Dictionary<QuestData, QuestProgress> questProgress = new Dictionary<QuestData, QuestProgress>();
    private readonly Dictionary<string, QuestProgressSaveData> pendingActiveQuestSaves = new Dictionary<string, QuestProgressSaveData>();
    private readonly HashSet<string> completedQuestIds = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterAvailableQuests(IEnumerable<QuestData> quests)
    {
        foreach (var quest in quests)
        {
            if (quest == null || IsQuestCompleted(quest) || activeQuests.Contains(quest))
                continue;

            string questId = GetQuestId(quest);
            if (!pendingActiveQuestSaves.TryGetValue(questId, out var saveData))
                continue;

            activeQuests.Add(quest);
            var progress = new QuestProgress(quest);
            progress.ApplySaveData(saveData);
            questProgress[quest] = progress;
        }
    }

    public void AcceptQuest(QuestData quest)
    {
        if (quest == null || IsQuestCompleted(quest) || activeQuests.Contains(quest))
            return;

        activeQuests.Add(quest);
        questProgress[quest] = new QuestProgress(quest);
        Save();
    }

    public void RemoveQuest(QuestData quest)
    {
        if (quest == null)
            return;

        activeQuests.Remove(quest);
        questProgress.Remove(quest);
        pendingActiveQuestSaves.Remove(GetQuestId(quest));
        Save();
    }

    public void CompleteQuest(QuestData quest)
    {
        if (quest == null || !IsQuestReady(quest))
            return;

        completedQuestIds.Add(GetQuestId(quest));
        RemoveQuest(quest);
        Save();
    }

    public bool IsQuestReady(QuestData quest)
    {
        return questProgress.TryGetValue(quest, out var progress) && progress.IsCompleted;
    }

    public bool IsQuestCompleted(QuestData quest)
    {
        return quest != null && completedQuestIds.Contains(GetQuestId(quest));
    }

    public bool IsQuestUnavailable(QuestData quest)
    {
        return activeQuests.Contains(quest) || IsQuestCompleted(quest);
    }

    public QuestProgress GetProgress(QuestData quest)
    {
        if (quest == null)
            return null;

        if (!questProgress.TryGetValue(quest, out var progress))
        {
            progress = new QuestProgress(quest);
            questProgress[quest] = progress;
        }

        return progress;
    }

    public string GetQuestProgressDescription(QuestData quest)
    {
        var progress = GetProgress(quest);
        if (progress == null || progress.Objectives.Count == 0)
            return quest != null ? quest.description : string.Empty;

        return string.Join("\n", progress.Objectives.Select(objective => objective.ProgressText));
    }

    public void RegisterEnemyKilled(EnemyData enemyData)
    {
        foreach (var quest in activeQuests)
        {
            if (!questProgress.TryGetValue(quest, out var progress))
            {
                progress = new QuestProgress(quest);
                questProgress[quest] = progress;
            }

            progress.RegisterEnemyKilled(enemyData);
        }

        Save();
    }

    private string GetQuestId(QuestData quest)
    {
        return quest.name;
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("Firebase initialized successfully.");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    private void Save()
    {
        var saveData = new QuestManagerSaveData
        {
            completedQuestIds = completedQuestIds.ToList(),
            activeQuests = activeQuests
                .Where(quest => quest != null && questProgress.ContainsKey(quest))
                .Select(quest => questProgress[quest].ToSaveData(GetQuestId(quest)))
                .ToList()
        };

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saveData));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        var saveData = JsonUtility.FromJson<QuestManagerSaveData>(PlayerPrefs.GetString(SaveKey));
        if (saveData == null)
            return;

        completedQuestIds.Clear();
        if (saveData.completedQuestIds != null)
        {
            foreach (var questId in saveData.completedQuestIds)
                completedQuestIds.Add(questId);
        }

        pendingActiveQuestSaves.Clear();
        if (saveData.activeQuests == null)
            return;

        foreach (var activeQuest in saveData.activeQuests)
        {
            if (!string.IsNullOrEmpty(activeQuest.questId))
                pendingActiveQuestSaves[activeQuest.questId] = activeQuest;
        }
    }
}
