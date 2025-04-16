using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveQuestUIItem : MonoBehaviour
{
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI descriptionText;

    public void Setup(QuestData quest)
    {
        questNameText.text = quest.questName;
        descriptionText.text = quest.description;
        // В будущем можно сюда добавить отображение целей и прогресса
    }
}
