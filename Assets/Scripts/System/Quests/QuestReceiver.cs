using Firebase.Firestore;
using UnityEngine;

public class QuestReceiver : MonoBehaviour
{
    private FirebaseFirestore db;
    private ListenerRegistration listener;

    [SerializeField] private string userId = "id1";

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        StartListeningForQuestChanges(userId);
    }

    private void StartListeningForQuestChanges(string userId)
    {
        listener = db.Collection("users").Document(userId).Collection("quests")
            .Listen(snapshot =>
            {
                Debug.Log("Quest changes received from Firebase.");

                if (QuestManager.Instance == null)
                    return;

                QuestManager.Instance.externalQuestDatas.Clear();

                foreach (var doc in snapshot.Documents)
                {
                    doc.TryGetValue("questName", out string questName);
                    doc.TryGetValue("description", out string description);
                    doc.TryGetValue("rewardGold", out int rewardGold);
                    doc.TryGetValue("rewardXP", out int rewardXP);
                    doc.TryGetValue("hardReward", out int hardReward);
                    doc.TryGetValue("isComplete", out bool isComplete);

                    ExternalQuestData external = new ExternalQuestData
                    {
                        externalId = doc.Id,
                        questName = questName,
                        description = description,
                        rewardGold = rewardGold,
                        rewardXP = rewardXP,
                        hardReward = hardReward,
                        isComplete = isComplete
                    };

                    QuestManager.Instance.externalQuestDatas.Add(external);
                }

                var panel = FindFirstObjectByType<ActiveQuestsPanel>();
                if (panel != null && panel.isActiveAndEnabled)
                    panel.RefreshActiveQuests();
            });
    }

    private void OnDestroy()
    {
        listener?.Stop();
    }
}
