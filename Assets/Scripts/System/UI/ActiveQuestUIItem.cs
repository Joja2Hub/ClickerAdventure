using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveQuestUIItem : MonoBehaviour
{
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI progressText;

    public void Setup(QuestData quest)
    {
        questNameText.text = quest.questName;
        descriptionText.text = quest.description;
        // � ������� ����� ���� �������� ����������� ����� � ���������
    }

 

}
