using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParentTaskUIItem : MonoBehaviour
{
    public TextMeshProUGUI taskNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI childNoteText;
    public TMP_InputField parentNoteInput;
    public Button approveButton;
    public Button rejectButton;

    private ExternalQuestData task;
    private ParentApprovalPanel parentPanel;

    private void OnDestroy()
    {
        if (approveButton != null)
            approveButton.onClick.RemoveListener(Approve);

        if (rejectButton != null)
            rejectButton.onClick.RemoveListener(Reject);
    }

    public void Setup(ExternalQuestData taskData, ParentApprovalPanel panel)
    {
        task = taskData;
        parentPanel = panel;

        if (taskNameText != null)
            taskNameText.text = task.questName;

        if (descriptionText != null)
            descriptionText.text = task.description;

        if (rewardText != null)
            rewardText.text = $"{task.rewardGold} gold / {task.rewardXP} XP";

        if (childNoteText != null)
            childNoteText.text = string.IsNullOrEmpty(task.childNote) ? "No note from child" : task.childNote;

        if (approveButton != null)
        {
            approveButton.onClick.RemoveListener(Approve);
            approveButton.onClick.AddListener(Approve);
        }

        if (rejectButton != null)
        {
            rejectButton.onClick.RemoveListener(Reject);
            rejectButton.onClick.AddListener(Reject);
        }
    }

    private void Approve()
    {
        parentPanel?.ApproveTask(task, GetParentNote());
    }

    private void Reject()
    {
        parentPanel?.RejectTask(task, GetParentNote());
    }

    private string GetParentNote()
    {
        return parentNoteInput != null ? parentNoteInput.text : string.Empty;
    }
}
