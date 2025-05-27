using System.Collections.Generic;
using UnityEngine;
using Firebase;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<QuestData> activeQuests = new List<QuestData>();
    public List<ExternalQuestData> externalQuestDatas = new List<ExternalQuestData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(this.gameObject);
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

    public void AcceptQuest(QuestData quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
        }
    }

    public void RemoveQuest(QuestData quest)
    {
        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
        }
    }
}
