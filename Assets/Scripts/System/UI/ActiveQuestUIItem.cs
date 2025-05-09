using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveQuestUIItem : MonoBehaviour
{
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI progressText;
    private QuestData currentQuest;

    public Button readyBut;

    public void Setup(QuestData quest)
    {
        currentQuest = quest;
        questNameText.text = currentQuest.questName;
        descriptionText.text = currentQuest.description;
        readyBut.interactable = false;
        
        // В будущем можно сюда добавить отображение целей и прогресса
    }

    public void UpdateQuest(QuestData quest)
    {
        if(quest.CheckReady()) 
            readyBut.interactable = true;
    }

    public void CompleteQuest()
    {
        PlayerStats.Instance.AddExperience(currentQuest.rewardXP);
        PlayerStats.Instance.AddMoney(currentQuest.rewardGold);
        QuestManager.Instance.RemoveQuest(currentQuest);
        Destroy(gameObject);
    }
 

}
