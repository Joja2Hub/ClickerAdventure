using UnityEngine;

[System.Serializable]
public class ExternalQuestData
{
    public string externalId;
    public string questName;
    public string description;
    public int rewardGold;
    public int rewardXP;
    public int hardReward;
    public bool isComplete;
    public bool isClaimed;
    public string status = RealWorldTaskStatus.Assigned;
    public string childNote;
    public string parentNote;

    public bool CanSubmitForReview => status == RealWorldTaskStatus.Assigned || status == RealWorldTaskStatus.Rejected;
    public bool IsWaitingForParent => status == RealWorldTaskStatus.Submitted;
    public bool CanClaimReward => isComplete || status == RealWorldTaskStatus.Approved;
}
