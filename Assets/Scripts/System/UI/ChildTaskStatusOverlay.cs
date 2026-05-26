using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChildTaskStatusOverlay : MonoBehaviour
{
    private UIManager uiManager;
    private QuestReceiver questReceiver;
    private DailyRoutineProgress routineProgress;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI statusText;
    private Image progressFill;
    private Image badgeImage;
    private bool isInitialized;

    public void Initialize(UIManager owner)
    {
        if (isInitialized)
            return;

        uiManager = owner;
        routineProgress = DailyRoutineProgress.Instance;
        questReceiver = QuestReceiver.Instance;

        Build();
        Subscribe();
        Refresh();
        isInitialized = true;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (questReceiver != null)
            questReceiver.OnRealWorldTasksChanged += Refresh;

        if (routineProgress != null)
            routineProgress.OnProgressChanged += Refresh;
    }

    private void Unsubscribe()
    {
        if (questReceiver != null)
            questReceiver.OnRealWorldTasksChanged -= Refresh;

        if (routineProgress != null)
            routineProgress.OnProgressChanged -= Refresh;
    }

    private void Refresh()
    {
        if (QuestManager.Instance == null || routineProgress == null)
            return;

        routineProgress.ResetForCurrentDayIfNeeded();

        int readyCount = QuestManager.Instance.externalQuestDatas.Count(task => task.CanClaimReward);
        int waitingCount = QuestManager.Instance.externalQuestDatas.Count(task => task.status == RealWorldTaskStatus.Submitted);
        int redoCount = QuestManager.Instance.externalQuestDatas.Count(task => task.status == RealWorldTaskStatus.Rejected);
        int activeCount = QuestManager.Instance.externalQuestDatas.Count(task => task.status == RealWorldTaskStatus.Assigned);
        int totalCount = QuestManager.Instance.externalQuestDatas.Count;

        if (titleText != null)
            titleText.text = readyCount > 0 ? $"Tasks: {readyCount} ready" : $"Tasks: {totalCount}";

        if (statusText != null)
            statusText.text = BuildStatusText(activeCount, waitingCount, redoCount, readyCount);

        if (progressFill != null)
            progressFill.fillAmount = routineProgress.GoalProgress;

        if (badgeImage != null)
            badgeImage.color = GetBadgeColor(readyCount, redoCount, waitingCount);
    }

    private string BuildStatusText(int activeCount, int waitingCount, int redoCount, int readyCount)
    {
        string daily = $"Daily {routineProgress.CompletedToday}/{routineProgress.DailyGoal}";

        if (readyCount > 0)
            return $"{daily}  |  Claim rewards";

        if (redoCount > 0)
            return $"{daily}  |  {redoCount} needs redo";

        if (waitingCount > 0)
            return $"{daily}  |  {waitingCount} waiting";

        if (activeCount > 0)
            return $"{daily}  |  {activeCount} to do";

        return $"{daily}  |  All clear";
    }

    private Color GetBadgeColor(int readyCount, int redoCount, int waitingCount)
    {
        if (readyCount > 0)
            return new Color(0.14f, 0.52f, 0.25f, 0.96f);

        if (redoCount > 0)
            return new Color(0.58f, 0.18f, 0.16f, 0.96f);

        if (waitingCount > 0)
            return new Color(0.44f, 0.36f, 0.15f, 0.96f);

        return new Color(0.08f, 0.18f, 0.26f, 0.96f);
    }

    private void OpenTasks()
    {
        if (uiManager == null)
            return;

        uiManager.ShowActiveQuestsPanel();
    }

    private void Build()
    {
        Canvas canvas = RuntimeUiHost.GetCanvas(transform);

        GameObject root = CreatePanel(canvas.transform, "ChildTaskStatusButton", new Color(0.07f, 0.09f, 0.12f, 0.94f));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.sizeDelta = new Vector2(360f, 128f);
        rootRect.anchoredPosition = new Vector2(28f, -108f);

        Button button = root.AddComponent<Button>();
        button.targetGraphic = root.GetComponent<Image>();
        button.onClick.AddListener(OpenTasks);

        VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(22, 22, 14, 14);
        layout.spacing = 6f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        titleText = CreateText(root.transform, "Tasks", 27, FontStyles.Bold, new Color(1f, 0.92f, 0.62f, 1f));
        titleText.alignment = TextAlignmentOptions.Left;

        statusText = CreateText(root.transform, "Daily 0/3", 19, FontStyles.Normal, new Color(0.84f, 0.9f, 0.95f, 1f));
        statusText.alignment = TextAlignmentOptions.Left;

        GameObject progressRoot = CreatePanel(root.transform, "DailyProgress", new Color(0.03f, 0.05f, 0.06f, 1f));
        LayoutElement progressElement = progressRoot.AddComponent<LayoutElement>();
        progressElement.preferredHeight = 14f;
        progressElement.flexibleWidth = 1f;

        progressFill = CreatePanel(progressRoot.transform, "Fill", new Color(0.24f, 0.8f, 0.45f, 1f)).GetComponent<Image>();
        progressFill.type = Image.Type.Filled;
        progressFill.fillMethod = Image.FillMethod.Horizontal;
        progressFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        Stretch(progressFill.GetComponent<RectTransform>());

        GameObject badge = CreatePanel(root.transform, "StatusAccent", new Color(0.08f, 0.18f, 0.26f, 0.96f));
        badgeImage = badge.GetComponent<Image>();
        RectTransform badgeRect = badge.GetComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(1f, 1f);
        badgeRect.anchorMax = new Vector2(1f, 1f);
        badgeRect.pivot = new Vector2(1f, 1f);
        badgeRect.sizeDelta = new Vector2(10f, 128f);
        badgeRect.anchoredPosition = Vector2.zero;
    }

    private GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private TextMeshProUGUI CreateText(Transform parent, string text, int fontSize, FontStyles style, Color color)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.color = color;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;
        return label;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
