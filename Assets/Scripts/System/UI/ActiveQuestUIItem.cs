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
        descriptionText.text = currentQuest.description;
        rewardText.text = currentQuest.rewardGold.ToString();
        readyBut.interactable = false;

        UpdateQuest(quest); // На всякий случай
    }

    public void SetupExternal(ExternalQuestData quest)
    {
        isExternal = true;
        currentExternal = quest;
        questNameText.text = quest.questName;
        descriptionText.text = quest.description;
        rewardText.text = quest.rewardGold.ToString();

        readyBut.interactable = quest.isComplete;
    }

    public void UpdateQuest(QuestData quest)
    {
        if (quest.CheckReady())
            readyBut.interactable = true;
    }

    public void CompleteQuest()
    {
        if (isExternal)
        {
            if (!currentExternal.isComplete)
            {
                Debug.LogWarning("Нельзя сдать невыполненный внешний квест");
                return;
            }

            PlayerStats.Instance.AddExperience(currentExternal.rewardXP);
            PlayerStats.Instance.AddMoney(currentExternal.rewardGold);
            //PlayerStats.Instance.AddHardCurrency(currentExternal.hardReward); // если такая есть

            QuestManager.Instance.externalQuestDatas.Remove(currentExternal);
        }
        else
        {
            PlayerStats.Instance.AddExperience(currentQuest.rewardXP);
            PlayerStats.Instance.AddMoney(currentQuest.rewardGold);

            QuestManager.Instance.RemoveQuest(currentQuest);
        }

        Destroy(gameObject);
    }
}
