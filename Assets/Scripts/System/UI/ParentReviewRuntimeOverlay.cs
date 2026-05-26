using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParentReviewRuntimeOverlay : MonoBehaviour
{
    private readonly ParentTaskTemplate[] taskTemplates =
    {
        new ParentTaskTemplate("Clean room", "Tidy toys, clothes, and desk.", 30, 15),
        new ParentTaskTemplate("Homework focus", "Finish today's homework block.", 40, 25),
        new ParentTaskTemplate("Read 20 min", "Read a book for twenty minutes.", 25, 20),
        new ParentTaskTemplate("Help at home", "Help with one useful home chore.", 30, 20)
    };

    private QuestReceiver receiver;
    private Canvas canvas;
    private GameObject root;
    private GameObject panel;
    private Transform listParent;
    private TextMeshProUGUI emptyText;
    private TextMeshProUGUI statusText;
    private TMP_InputField dailyGoalInput;
    private TMP_InputField customTitleInput;
    private TMP_InputField customDescriptionInput;
    private TMP_InputField customGoldInput;
    private TMP_InputField customExperienceInput;
    private bool isInitialized;

    public void Initialize(QuestReceiver questReceiver)
    {
        if (isInitialized)
            return;

        receiver = questReceiver;
        canvas = RuntimeUiHost.GetCanvas(transform);

        BuildOverlay();
        receiver.OnRealWorldTasksChanged += Refresh;
        Refresh();
        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (receiver != null)
            receiver.OnRealWorldTasksChanged -= Refresh;
    }

    private void BuildOverlay()
    {
        root = CreateUIObject("ParentReviewOverlay", RuntimeUiHost.GetPanelsRoot(canvas));
        Stretch(root.GetComponent<RectTransform>());
        root.transform.SetAsLastSibling();

        Transform buttonRoot = RuntimeUiHost.GetButtonRoot(canvas);
        Button toggleButton = CreateButton(buttonRoot, "Parent", new Color(0.12f, 0.18f, 0.24f, 0.95f), new Vector2(RuntimeUiStyle.MainButtonWidth, RuntimeUiStyle.MainButtonHeight), RuntimeUiStyle.ButtonTextSize);
        RuntimeUiStyle.ApplyButton(toggleButton, new Color(0.12f, 0.18f, 0.24f, 0.95f));
        RectTransform toggleRect = toggleButton.GetComponent<RectTransform>();
        if (!RuntimeUiHost.UsesLayout(buttonRoot))
        {
            toggleRect.anchorMin = new Vector2(1f, 1f);
            toggleRect.anchorMax = new Vector2(1f, 1f);
            toggleRect.pivot = new Vector2(1f, 1f);
            toggleRect.anchoredPosition = new Vector2(-24f, -24f);
        }

        toggleButton.onClick.AddListener(TogglePanel);

        panel = CreatePanel(root.transform, "ParentReviewPanel", new Color(0.07f, 0.08f, 0.1f, 0.97f));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(980f, 1160f);
        panelRect.anchoredPosition = Vector2.zero;
        panel.SetActive(false);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 20, 24);
        layout.spacing = 14f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        BuildHeader(panel.transform);
        BuildQuickAssign(panel.transform);
        BuildRoutineSettings(panel.transform);
        BuildCustomAssign(panel.transform);

        statusText = CreateText(panel.transform, "Active: 0  |  Waiting: 0  |  Approved: 0", 21, FontStyles.Bold, TextAlignmentOptions.Left);
        statusText.color = new Color(0.82f, 0.9f, 1f, 1f);
        AddLayoutElement(statusText.gameObject, 0f, 34f, 1f);

        emptyText = CreateText(panel.transform, "No real-life tasks yet.", 22, FontStyles.Normal, TextAlignmentOptions.Center);
        emptyText.color = new Color(0.78f, 0.8f, 0.84f, 1f);
        AddLayoutElement(emptyText.gameObject, 0f, 40f, 1f);

        BuildTaskScroll(panel.transform);
    }

    private void BuildHeader(Transform parent)
    {
        GameObject header = CreateUIObject("Header", parent);
        HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
        headerLayout.childControlWidth = true;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childAlignment = TextAnchor.MiddleCenter;
        AddLayoutElement(header, 0f, 58f, 1f);

        TextMeshProUGUI title = CreateText(header.transform, "Parent center", 31, FontStyles.Bold, TextAlignmentOptions.Left);
        title.color = new Color(1f, 0.92f, 0.68f, 1f);
        AddLayoutElement(title.gameObject, 0f, 56f, 1f);

        Button closeButton = CreateButton(header.transform, "Close", new Color(0.28f, 0.11f, 0.12f, 1f), new Vector2(130f, 48f));
        closeButton.onClick.AddListener(() => panel.SetActive(false));

        Button lockButton = CreateButton(header.transform, "Lock", new Color(0.2f, 0.21f, 0.27f, 1f), new Vector2(130f, 48f));
        lockButton.onClick.AddListener(() =>
        {
            ParentAccessGatePopup.LockSession();
            panel.SetActive(false);
        });
    }

    private void BuildQuickAssign(Transform parent)
    {
        GameObject section = CreatePanel(parent, "QuickAssign", new Color(0.12f, 0.13f, 0.17f, 1f));
        AddLayoutElement(section, 0f, 186f, 1f);

        VerticalLayoutGroup sectionLayout = section.AddComponent<VerticalLayoutGroup>();
        sectionLayout.padding = new RectOffset(18, 18, 14, 16);
        sectionLayout.spacing = 12f;
        sectionLayout.childControlWidth = true;
        sectionLayout.childControlHeight = true;
        sectionLayout.childForceExpandHeight = false;

        TextMeshProUGUI title = CreateText(section.transform, "Quick assign", 24, FontStyles.Bold, TextAlignmentOptions.Left);
        AddLayoutElement(title.gameObject, 0f, 32f, 1f);

        GameObject buttonRow = CreateUIObject("TemplateRow", section.transform);
        HorizontalLayoutGroup rowLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 12f;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        AddLayoutElement(buttonRow, 0f, 98f, 1f);

        foreach (ParentTaskTemplate template in taskTemplates)
        {
            Button button = CreateButton(buttonRow.transform, $"{template.Title}\n+{template.Gold}g +{template.Experience}xp", new Color(0.1f, 0.32f, 0.42f, 1f), new Vector2(206f, 92f), 18);
            button.onClick.AddListener(() => CreateTaskFromTemplate(template));
        }
    }

    private void BuildRoutineSettings(Transform parent)
    {
        GameObject section = CreatePanel(parent, "RoutineSettings", new Color(0.1f, 0.14f, 0.17f, 1f));
        AddLayoutElement(section, 0f, 112f, 1f);

        HorizontalLayoutGroup sectionLayout = section.AddComponent<HorizontalLayoutGroup>();
        sectionLayout.padding = new RectOffset(18, 18, 14, 14);
        sectionLayout.spacing = 12f;
        sectionLayout.childControlWidth = true;
        sectionLayout.childControlHeight = true;
        sectionLayout.childForceExpandWidth = false;
        sectionLayout.childAlignment = TextAnchor.MiddleCenter;

        GameObject textBlock = CreateUIObject("Text", section.transform);
        VerticalLayoutGroup textLayout = textBlock.AddComponent<VerticalLayoutGroup>();
        textLayout.spacing = 2f;
        textLayout.childControlWidth = true;
        textLayout.childControlHeight = true;
        textLayout.childForceExpandHeight = false;
        AddLayoutElement(textBlock, 0f, 0f, 1f);

        TextMeshProUGUI title = CreateText(textBlock.transform, "Daily goal", 24, FontStyles.Bold, TextAlignmentOptions.Left);
        title.color = new Color(0.7f, 1f, 0.78f, 1f);

        TextMeshProUGUI subtitle = CreateText(textBlock.transform, "How many real-life tasks complete the day.", 18, FontStyles.Normal, TextAlignmentOptions.Left);
        subtitle.color = new Color(0.82f, 0.88f, 0.92f, 1f);

        dailyGoalInput = CreateInput(section.transform, "Goal", 140f, 58f);
        dailyGoalInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        dailyGoalInput.text = DailyRoutineProgress.Instance.DailyGoal.ToString();

        Button saveButton = CreateButton(section.transform, "Save", new Color(0.12f, 0.42f, 0.28f, 1f), new Vector2(150f, 58f));
        saveButton.onClick.AddListener(SaveDailyGoal);
    }

    private void SaveDailyGoal()
    {
        int goal = ParseDailyGoal(dailyGoalInput, DailyRoutineProgress.Instance.DailyGoal);
        DailyRoutineProgress.Instance.SetDailyGoal(goal);
        dailyGoalInput.text = DailyRoutineProgress.Instance.DailyGoal.ToString();
        RewardPopup.ShowMessage("Daily goal updated", $"{DailyRoutineProgress.Instance.DailyGoal} tasks per day");
    }

    private void BuildCustomAssign(Transform parent)
    {
        GameObject section = CreatePanel(parent, "CustomAssign", new Color(0.1f, 0.12f, 0.16f, 1f));
        AddLayoutElement(section, 0f, 238f, 1f);

        VerticalLayoutGroup sectionLayout = section.AddComponent<VerticalLayoutGroup>();
        sectionLayout.padding = new RectOffset(18, 18, 14, 16);
        sectionLayout.spacing = 10f;
        sectionLayout.childControlWidth = true;
        sectionLayout.childControlHeight = true;
        sectionLayout.childForceExpandHeight = false;

        TextMeshProUGUI title = CreateText(section.transform, "Custom task", 24, FontStyles.Bold, TextAlignmentOptions.Left);
        AddLayoutElement(title.gameObject, 0f, 30f, 1f);

        customTitleInput = CreateInput(section.transform, "Task name", 0f, 48f);
        customDescriptionInput = CreateInput(section.transform, "Description", 0f, 54f);

        GameObject rewardRow = CreateUIObject("RewardRow", section.transform);
        HorizontalLayoutGroup rewardLayout = rewardRow.AddComponent<HorizontalLayoutGroup>();
        rewardLayout.spacing = 10f;
        rewardLayout.childControlWidth = true;
        rewardLayout.childControlHeight = true;
        rewardLayout.childForceExpandWidth = false;
        AddLayoutElement(rewardRow, 0f, 56f, 1f);

        customGoldInput = CreateInput(rewardRow.transform, "Gold", 170f, 52f);
        customGoldInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        customGoldInput.text = "30";

        customExperienceInput = CreateInput(rewardRow.transform, "XP", 170f, 52f);
        customExperienceInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        customExperienceInput.text = "20";

        Button createButton = CreateButton(rewardRow.transform, "Assign", new Color(0.12f, 0.42f, 0.3f, 1f), new Vector2(190f, 52f));
        createButton.onClick.AddListener(CreateCustomTask);
    }

    private void BuildTaskScroll(Transform parent)
    {
        GameObject scrollRoot = CreateUIObject("TaskScroll", parent);
        AddLayoutElement(scrollRoot, 0f, 0f, 1f, 1f);
        Image scrollImage = scrollRoot.AddComponent<Image>();
        scrollImage.color = new Color(1f, 1f, 1f, 0.04f);

        ScrollRect scrollRect = scrollRoot.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        GameObject viewport = CreateUIObject("Viewport", scrollRoot.transform);
        Stretch(viewport.GetComponent<RectTransform>());
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = Color.clear;

        GameObject content = CreateUIObject("Content", viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(14, 14, 14, 14);
        contentLayout.spacing = 12f;
        contentLayout.childControlHeight = true;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = contentRect;
        listParent = content.transform;
    }

    private void TogglePanel()
    {
        if (panel.activeSelf)
        {
            panel.SetActive(false);
            return;
        }

        ParentAccessGatePopup.RequestAccess(OpenPanel);
    }

    private void OpenPanel()
    {
        panel.SetActive(true);
        Refresh();
    }

    private void CreateTaskFromTemplate(ParentTaskTemplate template)
    {
        receiver.CreateRealWorldTask(template.Title, template.Description, template.Gold, template.Experience);
        RewardPopup.ShowMessage("Task assigned", $"+{template.Gold} gold\n+{template.Experience} XP");
    }

    private void CreateCustomTask()
    {
        string taskTitle = customTitleInput != null ? customTitleInput.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(taskTitle))
        {
            RewardPopup.ShowMessage("Task needs a name", "Add a title first");
            return;
        }

        string description = customDescriptionInput != null ? customDescriptionInput.text.Trim() : string.Empty;
        int gold = ParseReward(customGoldInput, 30);
        int experience = ParseReward(customExperienceInput, 20);

        receiver.CreateRealWorldTask(taskTitle, description, gold, experience);
        RewardPopup.ShowMessage("Task assigned", $"+{gold} gold\n+{experience} XP");

        customTitleInput.text = string.Empty;
        customDescriptionInput.text = string.Empty;
        customGoldInput.text = "30";
        customExperienceInput.text = "20";
    }

    private void Refresh()
    {
        if (listParent == null || QuestManager.Instance == null)
            return;

        foreach (Transform child in listParent)
        {
            Destroy(child.gameObject);
        }

        List<ExternalQuestData> tasks = QuestManager.Instance.externalQuestDatas
            .Where(task => task.status != RealWorldTaskStatus.Claimed && !task.isClaimed)
            .OrderByDescending(task => task.status == RealWorldTaskStatus.Submitted)
            .ThenByDescending(task => task.status == RealWorldTaskStatus.Approved)
            .ThenBy(task => task.questName)
            .ToList();

        int waitingCount = tasks.Count(task => task.status == RealWorldTaskStatus.Submitted);
        int approvedCount = tasks.Count(task => task.status == RealWorldTaskStatus.Approved || task.isComplete);
        int assignedCount = tasks.Count(task => task.status == RealWorldTaskStatus.Assigned || task.status == RealWorldTaskStatus.Rejected);

        statusText.text = $"Active: {assignedCount}  |  Waiting: {waitingCount}  |  Approved: {approvedCount}";
        emptyText.gameObject.SetActive(tasks.Count == 0);

        foreach (ExternalQuestData task in tasks)
        {
            CreateTaskRow(task);
        }
    }

    private void CreateTaskRow(ExternalQuestData task)
    {
        Color rowColor = task.status == RealWorldTaskStatus.Submitted
            ? new Color(0.16f, 0.17f, 0.23f, 1f)
            : new Color(0.12f, 0.14f, 0.18f, 0.98f);

        GameObject row = CreatePanel(listParent, "ParentTaskRow", rowColor);
        AddLayoutElement(row, 0f, 172f, 1f);

        HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.padding = new RectOffset(18, 18, 14, 14);
        rowLayout.spacing = 16f;
        rowLayout.childControlHeight = true;
        rowLayout.childControlWidth = true;
        rowLayout.childForceExpandWidth = false;

        GameObject textBlock = CreateUIObject("Text", row.transform);
        VerticalLayoutGroup textLayout = textBlock.AddComponent<VerticalLayoutGroup>();
        textLayout.spacing = 4f;
        textLayout.childControlWidth = true;
        textLayout.childControlHeight = true;
        textLayout.childForceExpandHeight = false;
        AddLayoutElement(textBlock, 0f, 0f, 1f);

        CreateText(textBlock.transform, task.questName, 24, FontStyles.Bold, TextAlignmentOptions.Left);
        CreateText(textBlock.transform, task.description, 18, FontStyles.Normal, TextAlignmentOptions.Left);
        CreateText(textBlock.transform, $"Reward: {task.rewardGold} gold / {task.rewardXP} XP", 18, FontStyles.Normal, TextAlignmentOptions.Left);
        CreateText(textBlock.transform, $"Status: {GetReadableStatus(task)}", 18, FontStyles.Bold, TextAlignmentOptions.Left).color = GetStatusColor(task);

        string noteText = string.IsNullOrEmpty(task.childNote) ? "Child note: none" : $"Child note: {task.childNote}";
        CreateText(textBlock.transform, noteText, 17, FontStyles.Italic, TextAlignmentOptions.Left).color = new Color(0.78f, 0.8f, 0.86f, 1f);

        GameObject actions = CreateUIObject("Actions", row.transform);
        VerticalLayoutGroup actionLayout = actions.AddComponent<VerticalLayoutGroup>();
        actionLayout.spacing = 10f;
        actionLayout.childControlWidth = true;
        actionLayout.childForceExpandWidth = true;
        AddLayoutElement(actions, 184f, 0f);

        if (task.status == RealWorldTaskStatus.Submitted)
        {
            Button approve = CreateButton(actions.transform, "Approve", new Color(0.12f, 0.42f, 0.22f, 1f), new Vector2(176f, 54f));
            approve.onClick.AddListener(() =>
            {
                ParentTaskReviewPopup.Show(task, true, parentNote =>
                {
                    receiver.ApproveTask(task, parentNote);
                    task.status = RealWorldTaskStatus.Approved;
                    task.isComplete = true;
                    task.parentNote = parentNote;
                    Refresh();
                });
            });

            Button reject = CreateButton(actions.transform, "Reject", new Color(0.52f, 0.16f, 0.14f, 1f), new Vector2(176f, 54f));
            reject.onClick.AddListener(() =>
            {
                ParentTaskReviewPopup.Show(task, false, parentNote =>
                {
                    receiver.RejectTask(task, parentNote);
                    task.status = RealWorldTaskStatus.Rejected;
                    task.isComplete = false;
                    task.parentNote = parentNote;
                    Refresh();
                });
            });
        }
        else
        {
            TextMeshProUGUI badge = CreateText(actions.transform, GetReadableStatus(task), 20, FontStyles.Bold, TextAlignmentOptions.Center);
            badge.color = GetStatusColor(task);
            AddLayoutElement(badge.gameObject, 176f, 54f);
        }

        if (CanCancel(task))
        {
            Button cancel = CreateButton(actions.transform, "Cancel", new Color(0.24f, 0.25f, 0.3f, 1f), new Vector2(176f, 48f), 18);
            cancel.onClick.AddListener(() =>
            {
                receiver.CancelTask(task);
                task.status = RealWorldTaskStatus.Cancelled;
                task.isClaimed = true;
                QuestManager.Instance.externalQuestDatas.Remove(task);
                Refresh();
            });
        }
    }

    private string GetReadableStatus(ExternalQuestData task)
    {
        if (task.status == RealWorldTaskStatus.Submitted)
            return "Waiting review";

        if (task.status == RealWorldTaskStatus.Approved || task.isComplete)
            return "Ready to claim";

        if (task.status == RealWorldTaskStatus.Rejected)
            return "Needs redo";

        if (task.status == RealWorldTaskStatus.Cancelled)
            return "Cancelled";

        return "Assigned";
    }

    private Color GetStatusColor(ExternalQuestData task)
    {
        if (task.status == RealWorldTaskStatus.Submitted)
            return new Color(1f, 0.8f, 0.34f, 1f);

        if (task.status == RealWorldTaskStatus.Approved || task.isComplete)
            return new Color(0.35f, 1f, 0.52f, 1f);

        if (task.status == RealWorldTaskStatus.Rejected)
            return new Color(1f, 0.42f, 0.36f, 1f);

        if (task.status == RealWorldTaskStatus.Cancelled)
            return new Color(0.6f, 0.62f, 0.68f, 1f);

        return new Color(0.72f, 0.82f, 1f, 1f);
    }

    private bool CanCancel(ExternalQuestData task)
    {
        return task.status == RealWorldTaskStatus.Assigned
            || task.status == RealWorldTaskStatus.Submitted
            || task.status == RealWorldTaskStatus.Rejected;
    }

    private int ParseReward(TMP_InputField input, int fallback)
    {
        if (input == null || !int.TryParse(input.text, out int value))
            return fallback;

        return Mathf.Clamp(value, 0, 9999);
    }

    private int ParseDailyGoal(TMP_InputField input, int fallback)
    {
        if (input == null || !int.TryParse(input.text, out int value))
            return fallback;

        return Mathf.Clamp(value, 1, 12);
    }

    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panelObject = CreateUIObject(name, parent);
        Image image = panelObject.AddComponent<Image>();
        image.color = color;
        return panelObject;
    }

    private Button CreateButton(Transform parent, string label, Color color, Vector2 size, int fontSize = 20)
    {
        GameObject buttonObject = CreatePanel(parent, label + "Button", color);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();

        TextMeshProUGUI text = CreateText(buttonObject.transform, label, fontSize, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(text.GetComponent<RectTransform>());
        text.raycastTarget = false;

        AddLayoutElement(buttonObject, size.x, size.y);
        return button;
    }

    private TMP_InputField CreateInput(Transform parent, string placeholder, float preferredWidth, float preferredHeight)
    {
        GameObject inputObject = CreatePanel(parent, placeholder + "Input", new Color(0.06f, 0.07f, 0.1f, 1f));
        AddLayoutElement(inputObject, preferredWidth, preferredHeight, preferredWidth <= 0f ? 1f : 0f);

        TMP_InputField input = inputObject.AddComponent<TMP_InputField>();
        input.textViewport = inputObject.GetComponent<RectTransform>();

        TextMeshProUGUI text = CreateText(inputObject.transform, string.Empty, 20, FontStyles.Normal, TextAlignmentOptions.Left);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(14f, 6f);
        textRect.offsetMax = new Vector2(-14f, -6f);
        input.textComponent = text;

        TextMeshProUGUI placeholderText = CreateText(inputObject.transform, placeholder, 20, FontStyles.Italic, TextAlignmentOptions.Left);
        placeholderText.color = new Color(0.55f, 0.58f, 0.64f, 1f);
        RectTransform placeholderRect = placeholderText.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(14f, 6f);
        placeholderRect.offsetMax = new Vector2(-14f, -6f);
        input.placeholder = placeholderText;

        return input;
    }

    private TextMeshProUGUI CreateText(Transform parent, string text, int size, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject textObject = CreateUIObject("Text", parent);
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.alignment = alignment;
        label.color = Color.white;
        label.textWrappingMode = TextWrappingModes.Normal;
        return label;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight, float flexibleWidth = 0f, float flexibleHeight = 0f)
    {
        LayoutElement layoutElement = target.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = target.AddComponent<LayoutElement>();

        if (preferredWidth > 0f)
            layoutElement.preferredWidth = preferredWidth;

        if (preferredHeight > 0f)
            layoutElement.preferredHeight = preferredHeight;

        layoutElement.flexibleWidth = flexibleWidth;
        layoutElement.flexibleHeight = flexibleHeight;
    }

    private readonly struct ParentTaskTemplate
    {
        public ParentTaskTemplate(string title, string description, int gold, int experience)
        {
            Title = title;
            Description = description;
            Gold = gold;
            Experience = experience;
        }

        public string Title { get; }
        public string Description { get; }
        public int Gold { get; }
        public int Experience { get; }
    }
}
