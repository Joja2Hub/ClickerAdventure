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
    private Image backgroundImage;
    private Image buttonImage;

    public void Setup(QuestData quest)
    {
        isExternal = false;
        currentQuest = quest;
        CacheVisuals();

        questNameText.text = currentQuest.questName;
        descriptionText.text = GetProgressDescription(currentQuest);
        rewardText.text = FormatReward(currentQuest.rewardGold, currentQuest.rewardXP);
        ApplyCardStyle(new Color(0.16f, 0.20f, 0.27f, 1f), new Color(0.22f, 0.48f, 0.78f, 1f));

        SetButtonState(quest.CheckReady(), "Claim");
    }

    public void SetupExternal(ExternalQuestData quest)
    {
        isExternal = true;
        currentExternal = quest;
        CacheVisuals();

        questNameText.text = quest.questName;
        descriptionText.text = BuildExternalDescription(quest);
        rewardText.text = FormatReward(quest.rewardGold, quest.rewardXP);
        ApplyExternalStyle(quest);

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
            RealWorldTaskSubmitPopup.Show(currentExternal, SubmitExternalForReview);
            return;
        }

        if (!currentExternal.CanClaimReward)
        {
            Debug.LogWarning("This real-world task is still waiting for parent approval.");
            return;
        }

        DailyRoutineRewardResult routineReward = DailyRoutineProgress.Instance.RecordRealTaskClaim();
        int totalGold = currentExternal.rewardGold + routineReward.BonusGold;
        int totalExperience = currentExternal.rewardXP + routineReward.BonusExperience;

        PlayerStats.Instance.AddExperience(totalExperience);
        PlayerStats.Instance.AddMoney(totalGold);

        if (routineReward.HasBonus)
        {
            RewardPopup.ShowMessage(
                "Daily goal complete",
                $"+{totalGold} gold\n+{totalExperience} XP\nStreak: {routineReward.CurrentStreak} days");
        }
        else
        {
            RewardPopup.ShowMessage(
                "Real task reward",
                $"+{totalGold} gold\n+{totalExperience} XP\nToday: {routineReward.CompletedToday}/{routineReward.DailyGoal}");
        }

        QuestReceiver.Instance?.MarkRewardClaimed(currentExternal);
        QuestManager.Instance.externalQuestDatas.Remove(currentExternal);

        ActiveQuestsPanel activePanel = FindFirstObjectByType<ActiveQuestsPanel>();
        if (activePanel != null && activePanel.isActiveAndEnabled)
            activePanel.RefreshActiveQuests();

        Destroy(gameObject);
    }

    private void SubmitExternalForReview(string childNote)
    {
        QuestReceiver.Instance?.SubmitForParentApproval(currentExternal, childNote);
        RewardPopup.ShowMessage("Sent to parent", string.IsNullOrWhiteSpace(childNote) ? "Waiting for approval" : "Note included");
        SetupExternal(currentExternal);
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

        return $"{quest.description}\nStatus: <b>{statusText}</b>";
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

    private void CacheVisuals()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (buttonImage == null && readyBut != null)
            buttonImage = readyBut.GetComponent<Image>();
    }

    private void ApplyExternalStyle(ExternalQuestData quest)
    {
        switch (quest.status)
        {
            case RealWorldTaskStatus.Submitted:
                ApplyCardStyle(new Color(0.18f, 0.18f, 0.24f, 1f), new Color(0.42f, 0.44f, 0.52f, 1f));
                break;
            case RealWorldTaskStatus.Approved:
                ApplyCardStyle(new Color(0.12f, 0.24f, 0.17f, 1f), new Color(0.18f, 0.58f, 0.29f, 1f));
                break;
            case RealWorldTaskStatus.Rejected:
                ApplyCardStyle(new Color(0.28f, 0.15f, 0.14f, 1f), new Color(0.72f, 0.29f, 0.24f, 1f));
                break;
            default:
                ApplyCardStyle(new Color(0.15f, 0.21f, 0.21f, 1f), new Color(0.20f, 0.55f, 0.58f, 1f));
                break;
        }
    }

    private void ApplyCardStyle(Color cardColor, Color actionColor)
    {
        if (backgroundImage != null)
            backgroundImage.color = cardColor;

        if (buttonImage != null)
            buttonImage.color = actionColor;
    }

    private string FormatReward(int gold, int experience)
    {
        return $"Reward: {gold} gold / {experience} XP";
    }
}
