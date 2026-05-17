using Firebase.Firestore;
using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestReceiver : MonoBehaviour
{
    public static QuestReceiver Instance;
    public event Action OnRealWorldTasksChanged;

    private FirebaseFirestore db;
    private ListenerRegistration listener;
    private ParentReviewRuntimeOverlay parentReviewOverlay;
    private readonly Dictionary<string, string> knownTaskStates = new Dictionary<string, string>();
    private bool receivedInitialSnapshot;

    [SerializeField] private string userId = "id1";
    [SerializeField] private string realWorldTasksCollection = "realWorldTasks";
    [SerializeField] private bool enableParentReviewOverlay = true;

    private CollectionReference RealWorldTasks => db.Collection("users").Document(userId).Collection(realWorldTasksCollection);

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        StartListeningForQuestChanges(userId);
        InitializeParentReviewOverlay();
    }

    public void CreateRealWorldTask(string questName, string description, int rewardGold, int rewardXP, int hardReward = 0)
    {
        if (string.IsNullOrWhiteSpace(questName))
            return;

        var taskData = new Dictionary<string, object>
        {
            { "questName", questName.Trim() },
            { "title", questName.Trim() },
            { "description", string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim() },
            { "rewardGold", Mathf.Max(0, rewardGold) },
            { "rewardXP", Mathf.Max(0, rewardXP) },
            { "hardReward", Mathf.Max(0, hardReward) },
            { "status", RealWorldTaskStatus.Assigned },
            { "isComplete", false },
            { "isClaimed", false },
            { "childNote", string.Empty },
            { "parentNote", string.Empty },
            { "createdAt", DateTime.UtcNow.ToString("O") }
        };

        RealWorldTasks.AddAsync(taskData).ContinueWith(taskResult =>
        {
            if (taskResult.IsFaulted)
            {
                Debug.LogError($"Failed to create real-world task '{questName}': {taskResult.Exception}");
                return;
            }

            Debug.Log($"Real-world task created: {questName}");
        });
    }

    public void SubmitForParentApproval(ExternalQuestData task, string childNote = "")
    {
        if (task == null || string.IsNullOrEmpty(task.externalId))
            return;

        task.status = RealWorldTaskStatus.Submitted;
        task.childNote = childNote;
        task.isComplete = false;

        var updates = new Dictionary<string, object>
        {
            { "status", RealWorldTaskStatus.Submitted },
            { "isComplete", false },
            { "childNote", childNote },
            { "submittedAt", DateTime.UtcNow.ToString("O") }
        };

        RealWorldTasks.Document(task.externalId).UpdateAsync(updates).ContinueWith(taskResult =>
        {
            if (taskResult.IsFaulted)
                Debug.LogError($"Failed to submit real-world task '{task.externalId}': {taskResult.Exception}");
        });
    }

    public void MarkRewardClaimed(ExternalQuestData task)
    {
        if (task == null || string.IsNullOrEmpty(task.externalId))
            return;

        task.status = RealWorldTaskStatus.Claimed;
        task.isClaimed = true;

        var updates = new Dictionary<string, object>
        {
            { "status", RealWorldTaskStatus.Claimed },
            { "isClaimed", true },
            { "claimedAt", DateTime.UtcNow.ToString("O") }
        };

        RealWorldTasks.Document(task.externalId).UpdateAsync(updates).ContinueWith(taskResult =>
        {
            if (taskResult.IsFaulted)
                Debug.LogError($"Failed to mark real-world task '{task.externalId}' as claimed: {taskResult.Exception}");
        });
    }

    public void CancelTask(ExternalQuestData task, string parentNote = "Cancelled by parent")
    {
        if (task == null || string.IsNullOrEmpty(task.externalId))
            return;

        task.status = RealWorldTaskStatus.Cancelled;
        task.isClaimed = true;
        task.parentNote = parentNote;

        var updates = new Dictionary<string, object>
        {
            { "status", RealWorldTaskStatus.Cancelled },
            { "isClaimed", true },
            { "isComplete", false },
            { "parentNote", parentNote },
            { "cancelledAt", DateTime.UtcNow.ToString("O") }
        };

        RealWorldTasks.Document(task.externalId).UpdateAsync(updates).ContinueWith(taskResult =>
        {
            if (taskResult.IsFaulted)
                Debug.LogError($"Failed to cancel real-world task '{task.externalId}': {taskResult.Exception}");
        });
    }

    public void ApproveTask(ExternalQuestData task, string parentNote = "")
    {
        if (task == null || string.IsNullOrEmpty(task.externalId))
            return;

        task.status = RealWorldTaskStatus.Approved;
        task.isComplete = true;
        task.parentNote = parentNote;

        var updates = new Dictionary<string, object>
        {
            { "status", RealWorldTaskStatus.Approved },
            { "isComplete", true },
            { "parentNote", parentNote },
            { "reviewedAt", DateTime.UtcNow.ToString("O") }
        };

        RealWorldTasks.Document(task.externalId).UpdateAsync(updates).ContinueWith(taskResult =>
        {
            if (taskResult.IsFaulted)
                Debug.LogError($"Failed to approve real-world task '{task.externalId}': {taskResult.Exception}");
        });
    }

    public void RejectTask(ExternalQuestData task, string parentNote = "")
    {
        if (task == null || string.IsNullOrEmpty(task.externalId))
            return;

        task.status = RealWorldTaskStatus.Rejected;
        task.isComplete = false;
        task.parentNote = parentNote;

        var updates = new Dictionary<string, object>
        {
            { "status", RealWorldTaskStatus.Rejected },
            { "isComplete", false },
            { "parentNote", parentNote },
            { "reviewedAt", DateTime.UtcNow.ToString("O") }
        };

        RealWorldTasks.Document(task.externalId).UpdateAsync(updates).ContinueWith(taskResult =>
        {
            if (taskResult.IsFaulted)
                Debug.LogError($"Failed to reject real-world task '{task.externalId}': {taskResult.Exception}");
        });
    }

    private void StartListeningForQuestChanges(string currentUserId)
    {
        listener = db.Collection("users").Document(currentUserId).Collection(realWorldTasksCollection)
            .Listen(snapshot =>
            {
                Debug.Log("Real-world task changes received from Firebase.");

                if (QuestManager.Instance == null)
                    return;

                QuestManager.Instance.externalQuestDatas.Clear();

                foreach (var doc in snapshot.Documents)
                {
                    ExternalQuestData external = CreateExternalQuest(doc);
                    TrackTaskNotification(external);

                    if (external.isClaimed || external.status == RealWorldTaskStatus.Claimed || external.status == RealWorldTaskStatus.Cancelled)
                        continue;

                    QuestManager.Instance.externalQuestDatas.Add(external);
                }

                receivedInitialSnapshot = true;

                var panel = FindFirstObjectByType<ActiveQuestsPanel>();
                if (panel != null && panel.isActiveAndEnabled)
                    panel.RefreshActiveQuests();

                OnRealWorldTasksChanged?.Invoke();
            });
    }

    private void TrackTaskNotification(ExternalQuestData task)
    {
        if (task == null || string.IsNullOrEmpty(task.externalId))
            return;

        string currentState = BuildNotificationState(task);
        if (!knownTaskStates.TryGetValue(task.externalId, out string previousState))
        {
            knownTaskStates[task.externalId] = currentState;

            if (receivedInitialSnapshot && task.status == RealWorldTaskStatus.Assigned)
            {
                RealWorldTaskNotificationToast.Show(
                    "New real-life task",
                    $"{task.questName}\nReward: {task.rewardGold} gold / {task.rewardXP} XP",
                    new Color(0.1f, 0.32f, 0.42f, 0.96f));
            }

            return;
        }

        if (previousState == currentState)
            return;

        knownTaskStates[task.externalId] = currentState;
        ShowTaskStateNotification(task);
    }

    private string BuildNotificationState(ExternalQuestData task)
    {
        return $"{task.status}|{task.parentNote}|{task.rewardGold}|{task.rewardXP}";
    }

    private void ShowTaskStateNotification(ExternalQuestData task)
    {
        switch (task.status)
        {
            case RealWorldTaskStatus.Approved:
                RealWorldTaskNotificationToast.Show(
                    "Task approved",
                    BuildParentFeedbackMessage(task, "Reward is ready to claim"),
                    new Color(0.1f, 0.38f, 0.2f, 0.96f));
                break;

            case RealWorldTaskStatus.Rejected:
                RealWorldTaskNotificationToast.Show(
                    "Try again",
                    BuildParentFeedbackMessage(task, "Parent asked for another try"),
                    new Color(0.48f, 0.16f, 0.13f, 0.96f));
                break;

            case RealWorldTaskStatus.Cancelled:
                RealWorldTaskNotificationToast.Show(
                    "Task cancelled",
                    BuildParentFeedbackMessage(task, "This task was removed"),
                    new Color(0.24f, 0.25f, 0.3f, 0.96f));
                break;
        }
    }

    private string BuildParentFeedbackMessage(ExternalQuestData task, string fallback)
    {
        string note = string.IsNullOrWhiteSpace(task.parentNote) ? fallback : task.parentNote;
        return $"{task.questName}\n{note}";
    }

    private void OnDestroy()
    {
        listener?.Stop();

        if (Instance == this)
            Instance = null;
    }

    private void InitializeParentReviewOverlay()
    {
        if (!enableParentReviewOverlay)
            return;

        parentReviewOverlay = FindFirstObjectByType<ParentReviewRuntimeOverlay>();
        if (parentReviewOverlay == null)
            parentReviewOverlay = gameObject.AddComponent<ParentReviewRuntimeOverlay>();

        parentReviewOverlay.Initialize(this);
    }

    private ExternalQuestData CreateExternalQuest(DocumentSnapshot doc)
    {
        string questName = GetString(doc, "questName", GetString(doc, "title", "Task"));
        string description = GetString(doc, "description", string.Empty);
        string status = GetString(doc, "status", string.Empty);
        bool isComplete = GetBool(doc, "isComplete", false);

        if (string.IsNullOrEmpty(status))
            status = isComplete ? RealWorldTaskStatus.Approved : RealWorldTaskStatus.Assigned;

        return new ExternalQuestData
        {
            externalId = doc.Id,
            questName = questName,
            description = description,
            rewardGold = GetInt(doc, "rewardGold", 0),
            rewardXP = GetInt(doc, "rewardXP", 0),
            hardReward = GetInt(doc, "hardReward", 0),
            isComplete = isComplete || status == RealWorldTaskStatus.Approved,
            isClaimed = GetBool(doc, "isClaimed", false),
            status = status,
            childNote = GetString(doc, "childNote", string.Empty),
            parentNote = GetString(doc, "parentNote", string.Empty)
        };
    }

    private string GetString(DocumentSnapshot doc, string field, string fallback)
    {
        return doc.TryGetValue(field, out string value) ? value : fallback;
    }

    private int GetInt(DocumentSnapshot doc, string field, int fallback)
    {
        return doc.TryGetValue(field, out int value) ? value : fallback;
    }

    private bool GetBool(DocumentSnapshot doc, string field, bool fallback)
    {
        return doc.TryGetValue(field, out bool value) ? value : fallback;
    }
}
