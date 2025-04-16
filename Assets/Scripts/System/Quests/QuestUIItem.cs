using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIItem : MonoBehaviour
{
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI descriptionText;
    public Button acceptButton;

    private QuestData questData;

    public void Setup(QuestData quest, System.Action<QuestData> onAccept)
    {
        questData = quest;
        questNameText.text = quest.questName;
        descriptionText.text = quest.description;

        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(() => onAccept?.Invoke(quest));
    }
}
