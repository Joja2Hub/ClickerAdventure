using System.Linq;
using TMPro;
using UnityEngine;

public class ParentApprovalPanel : MonoBehaviour
{
    public Transform taskListParent;
    public GameObject taskPrefab;
    public TextMeshProUGUI emptyStateText;

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Refresh()
    {
        if (taskListParent == null || taskPrefab == null)
            return;

        foreach (Transform child in taskListParent)
        {
            Destroy(child.gameObject);
        }

        if (QuestManager.Instance == null)
        {
            SetEmptyState(true);
            return;
        }

        var submittedTasks = QuestManager.Instance.externalQuestDatas
            .Where(task => task.status == RealWorldTaskStatus.Submitted)
            .ToList();

        SetEmptyState(submittedTasks.Count == 0);

        foreach (var task in submittedTasks)
        {
            GameObject taskObject = Instantiate(taskPrefab, taskListParent);
            ParentTaskUIItem item = taskObject.GetComponent<ParentTaskUIItem>();
            if (item != null)
                item.Setup(task, this);
        }
    }

    public void ApproveTask(ExternalQuestData task, string parentNote)
    {
        QuestReceiver.Instance?.ApproveTask(task, parentNote);
        Refresh();
    }

    public void RejectTask(ExternalQuestData task, string parentNote)
    {
        QuestReceiver.Instance?.RejectTask(task, parentNote);
        Refresh();
    }

    private void Subscribe()
    {
        if (QuestReceiver.Instance != null)
            QuestReceiver.Instance.OnRealWorldTasksChanged += Refresh;
    }

    private void Unsubscribe()
    {
        if (QuestReceiver.Instance != null)
            QuestReceiver.Instance.OnRealWorldTasksChanged -= Refresh;
    }

    private void SetEmptyState(bool isEmpty)
    {
        if (emptyStateText != null)
            emptyStateText.gameObject.SetActive(isEmpty);
    }
}
