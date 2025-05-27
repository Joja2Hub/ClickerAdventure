using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

public class QuestReceiver : MonoBehaviour
{
    FirebaseFirestore db;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        string userId = "id1"; // Замените на нужного пользователя
        LoadExternalQuests(userId);
    }

    public void LoadExternalQuests(string userId)
    {
        db.Collection("users").Document(userId).Collection("quests").GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Ошибка загрузки квестов из Firestore");
                return;
            }

            QuerySnapshot snapshot = task.Result;

            foreach (var doc in snapshot.Documents)
            {
                doc.TryGetValue("questName", out string questName);
                doc.TryGetValue("description", out string description);
                doc.TryGetValue("rewardGold", out int rewardGold);
                doc.TryGetValue("rewardXP", out int rewardXP);
                doc.TryGetValue("hardReward", out int hardReward);

                ExternalQuestData external = new ExternalQuestData
                {
                    externalId = doc.Id,
                    questName = questName,
                    description = description,
                    rewardGold = rewardGold,
                    rewardXP = rewardXP,
                    hardReward = hardReward
                };
                QuestManager.Instance.externalQuestDatas.Add(external);
                
            }
        });
    }
}