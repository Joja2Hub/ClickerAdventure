using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveQuestUIItem : MonoBehaviour
{
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI rewardText;
    public Button readyBut;

    private bool isExternal;
    private QuestData currentQuest;
    private ExternalQuestData currentExternal;

    public void Setup(QuestData quest)
    {
        isExternal = false;
        currentQuest = quest;
        questNameText.text = currentQuest.questName;
        descriptionText.text = GetProgressDescription(currentQuest);
        rewardText.text = $"{currentQuest.rewardGold} / {currentQuest.rewardXP} XP";

        UpdateQuest(quest);
    }

    public void SetupExternal(ExternalQuestData quest)
    {
        isExternal = true;
        currentExternal = quest;
        questNameText.text = quest.questName;
        descriptionText.text = quest.description;
        rewardText.text = $"{quest.rewardGold} / {quest.rewardXP} XP";

        readyBut.interactable = quest.isComplete;
    }

    public void UpdateQuest(QuestData quest)
    {
        descriptionText.text = GetProgressDescription(quest);
        readyBut.interactable = quest.CheckReady();
    }

    public void CompleteQuest()
    {
        if (isExternal)
        {
            if (!currentExternal.isComplete)
            {
                Debug.LogWarning("Cannot complete an unfinished external quest.");
                return;
            }

            PlayerStats.Instance.AddExperience(currentExternal.rewardXP);
            PlayerStats.Instance.AddMoney(currentExternal.rewardGold);
            QuestManager.Instance.externalQuestDatas.Remove(currentExternal);
        }
        else
        {
            if (!currentQuest.CheckReady())
            {
                Debug.LogWarning("Cannot complete an unfinished quest.");
                return;
            }

            PlayerStats.Instance.AddExperience(currentQuest.rewardXP);
            PlayerStats.Instance.AddMoney(currentQuest.rewardGold);
            QuestManager.Instance.CompleteQuest(currentQuest);
        }

        Destroy(gameObject);
    }

    private string GetProgressDescription(QuestData quest)
    {
        if (QuestManager.Instance == null)
            return quest.description;

        return QuestManager.Instance.GetQuestProgressDescription(quest);
    }
}
