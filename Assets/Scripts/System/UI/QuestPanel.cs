using System.Collections.Generic;
using UnityEngine;

public class QuestPanel : MonoBehaviour
{
    public GameObject questPrefab;
    public Transform questListParent;

    public List<QuestData> availableQuests = new List<QuestData>();

    private void Start()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.RegisterAvailableQuests(availableQuests);

        RefreshQuestList();
    }

    public void RefreshQuestList()
    {
        foreach (Transform child in questListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var quest in availableQuests)
        {
            if (QuestManager.Instance.IsQuestUnavailable(quest))
                continue;

            GameObject buttonGO = Instantiate(questPrefab, questListParent);
            QuestUIItem questUI = buttonGO.GetComponent<QuestUIItem>();
            questUI.Setup(quest, AcceptQuest);
        }
    }

    public void AcceptQuest(QuestData quest)
    {
        QuestManager.Instance.AcceptQuest(quest);
        RefreshQuestList();
    }
}
