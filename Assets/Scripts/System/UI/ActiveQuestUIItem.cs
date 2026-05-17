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
        rewardText.text = $"{currentQuest.rewardGold} gold / {currentQuest.rewardXP} XP";

        SetButtonState(quest.CheckReady(), "Claim");
    }

    public void SetupExternal(ExternalQuestData quest)
    {
        isExternal = true;
        currentExternal = quest;
        questNameText.text = quest.questName;
        descriptionText.text = BuildExternalDescription(quest);
        rewardText.text = $"{quest.rewardGold} gold / {quest.rewardXP} XP";

        if (quest.CanClaimReward)
        {
            SetButtonState(true, "Claim");
        }
        else if (quest.CanSubmitForReview)
        {
            SetButtonState(true, "Done");
        }
        else
        {
            SetButtonState(false, "Waiting");
        }
    }

    public void UpdateQuest(QuestData quest)
    {
        descriptionText.text = GetProgressDescription(quest);
        SetButtonState(quest.CheckReady(), "Claim");
    }

    public void CompleteQuest()
    {
        if (isExternal)
        {
            HandleExternalAction();
            return;
        }

        if (!currentQuest.CheckReady())
        {
            Debug.LogWarning("Cannot complete an unfinished quest.");
            return;
        }

        PlayerStats.Instance.AddExperience(currentQuest.rewardXP);
        PlayerStats.Instance.AddMoney(currentQuest.rewardGold);
        RewardPopup.ShowReward("Quest complete", currentQuest.rewardGold, currentQuest.rewardXP);
        QuestManager.Instance.CompleteQuest(currentQuest);
        Destroy(gameObject);
    }

    private void HandleExternalAction()
    {
        if (currentExternal.CanSubmitForReview)
        {
            QuestReceiver.Instance?.SubmitForParentApproval(currentExternal);
            RewardPopup.ShowMessage("Sent to parent", "Waiting for approval");
            SetupExternal(currentExternal);
            return;
        }

        if (!currentExternal.CanClaimReward)
        {
            Debug.LogWarning("This real-world task is still waiting for parent approval.");
            return;
        }

        PlayerStats.Instance.AddExperience(currentExternal.rewardXP);
        PlayerStats.Instance.AddMoney(currentExternal.rewardGold);
        RewardPopup.ShowReward("Real task reward", currentExternal.rewardGold, currentExternal.rewardXP);
        QuestReceiver.Instance?.MarkRewardClaimed(currentExternal);
        QuestManager.Instance.externalQuestDatas.Remove(currentExternal);
        Destroy(gameObject);
    }

    private string GetProgressDescription(QuestData quest)
    {
        if (QuestManager.Instance == null)
            return quest.description;

        return QuestManager.Instance.GetQuestProgressDescription(quest);
    }

    private string BuildExternalDescription(ExternalQuestData quest)
    {
        string statusText = GetExternalStatusText(quest);
        if (!string.IsNullOrEmpty(quest.parentNote))
            statusText += $"\nParent: {quest.parentNote}";

        return $"{quest.description}\nStatus: {statusText}";
    }

    private string GetExternalStatusText(ExternalQuestData quest)
    {
        switch (quest.status)
        {
            case RealWorldTaskStatus.Submitted:
                return "waiting for parent approval";
            case RealWorldTaskStatus.Approved:
                return "approved, reward ready";
            case RealWorldTaskStatus.Rejected:
                return "needs another try";
            case RealWorldTaskStatus.Claimed:
                return "claimed";
            default:
                return "ready to do";
        }
    }

    private void SetButtonState(bool interactable, string label)
    {
        readyBut.interactable = interactable;

        var buttonText = readyBut.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
            buttonText.text = label;
    }
}
