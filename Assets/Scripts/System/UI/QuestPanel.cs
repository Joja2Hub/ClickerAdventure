using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestPanel : MonoBehaviour
{
    public GameObject questPrefab; // ������ ������ ������
    public Transform questListParent;    // �������� ��� ������

    public List<QuestData> availableQuests = new List<QuestData>();

    private void Start()
    {
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
            if (!QuestManager.Instance.activeQuests.Contains(quest))
            {
                GameObject buttonGO = Instantiate(questPrefab, questListParent);
                QuestUIItem questUI = buttonGO.GetComponent<QuestUIItem>();
                QuestData questCopy = quest; // ��� ���������

                questUI.Setup(questCopy, AcceptQuest);
            }
        }
    }


    public void AcceptQuest(QuestData quest)
    {
        QuestManager.Instance.AcceptQuest(quest);
        availableQuests.Remove(quest);
        RefreshQuestList(); // ��������� ������
        // ����� ������� ���������� UI �������� ������� ���
    }
}
