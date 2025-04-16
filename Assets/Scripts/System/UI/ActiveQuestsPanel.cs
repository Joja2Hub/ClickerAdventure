using UnityEngine;

public class ActiveQuestsPanel : MonoBehaviour
{
    public Transform questListParent; // ���� ��������� UI-������
    public GameObject questPrefab; // ������ ������ UI-��������

    public void RefreshActiveQuests()
    {
        foreach (Transform child in questListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            GameObject questGO = Instantiate(questPrefab, questListParent);
            ActiveQuestUIItem uiItem = questGO.GetComponent<ActiveQuestUIItem>();
            uiItem.Setup(quest);
        }
    }

    private void OnEnable()
    {
        RefreshActiveQuests(); // ��������� ������ ��� ��� �������� ������
    }
}
