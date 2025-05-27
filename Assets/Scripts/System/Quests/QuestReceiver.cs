using Firebase.Firestore;
using UnityEngine;

public class QuestReceiver : MonoBehaviour
{
    private FirebaseFirestore db;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        ListenForQuests("HCCCr1GN0znPBOE1Q2Do"); 
    }

    private void ListenForQuests(string deviceId)
    {
        DocumentReference questRef = db.Collection("Quests").Document(deviceId);

        questRef.Listen(snapshot =>
        {
            if (snapshot.Exists)
            {
                var receivedQuest = snapshot.ConvertTo<ExternalQuestData>();
                QuestData quest = receivedQuest.ToQuestData();

                QuestManager.Instance.AcceptQuest(quest);
                Debug.Log("Получен новый внешний квест: " + quest.questName);
            }
        });
    }
}