using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParentReviewRuntimeOverlay : MonoBehaviour
{
    private QuestReceiver receiver;
    private Canvas canvas;
    private GameObject root;
    private GameObject panel;
    private Transform listParent;
    private TextMeshProUGUI emptyText;
    private bool isInitialized;

    public void Initialize(QuestReceiver questReceiver)
    {
        if (isInitialized)
            return;

        receiver = questReceiver;
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas();

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
        root = CreateUIObject("ParentReviewOverlay", canvas.transform);
        Stretch(root.GetComponent<RectTransform>());

        Button toggleButton = CreateButton(root.transform, "Parent", new Color(0.12f, 0.18f, 0.24f, 0.95f), new Vector2(170f, 56f));
        RectTransform toggleRect = toggleButton.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(1f, 1f);
        toggleRect.anchorMax = new Vector2(1f, 1f);
        toggleRect.pivot = new Vector2(1f, 1f);
        toggleRect.anchoredPosition = new Vector2(-24f, -24f);
        toggleButton.onClick.AddListener(TogglePanel);

        panel = CreatePanel(root.transform, "ParentReviewPanel", new Color(0.07f, 0.08f, 0.1f, 0.96f));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(900f, 680f);
        panelRect.anchoredPosition = Vector2.zero;
        panel.SetActive(false);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 20, 24);
        layout.spacing = 14f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        GameObject header = CreateUIObject("Header", panel.transform);
        HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
        headerLayout.childControlWidth = true;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childAlignment = TextAnchor.MiddleCenter;
        AddLayoutElement(header, 0f, 56f, 1f);

        TextMeshProUGUI title = CreateText(header.transform, "Parent review", 30, FontStyles.Bold, TextAlignmentOptions.Left);
        AddLayoutElement(title.gameObject, 0f, 56f, 1f);

        Button closeButton = CreateButton(header.transform, "Close", new Color(0.28f, 0.11f, 0.12f, 1f), new Vector2(130f, 48f));
        closeButton.onClick.AddListener(() => panel.SetActive(false));

        emptyText = CreateText(panel.transform, "No tasks are waiting for approval.", 22, FontStyles.Normal, TextAlignmentOptions.Center);
        AddLayoutElement(emptyText.gameObject, 0f, 42f, 1f);

        GameObject scrollRoot = CreateUIObject("TaskScroll", panel.transform);
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
        panel.SetActive(!panel.activeSelf);
        if (panel.activeSelf)
            Refresh();
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
            .Where(task => task.status == RealWorldTaskStatus.Submitted)
            .ToList();

        emptyText.gameObject.SetActive(tasks.Count == 0);

        foreach (ExternalQuestData task in tasks)
        {
            CreateTaskRow(task);
        }
    }

    private void CreateTaskRow(ExternalQuestData task)
    {
        GameObject row = CreatePanel(listParent, "ParentTaskRow", new Color(0.13f, 0.15f, 0.19f, 0.98f));
        AddLayoutElement(row, 0f, 170f, 1f);

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
        CreateText(textBlock.transform, string.IsNullOrEmpty(task.childNote) ? "Child note: none" : $"Child note: {task.childNote}", 18, FontStyles.Italic, TextAlignmentOptions.Left);

        GameObject actions = CreateUIObject("Actions", row.transform);
        VerticalLayoutGroup actionLayout = actions.AddComponent<VerticalLayoutGroup>();
        actionLayout.spacing = 10f;
        actionLayout.childControlWidth = true;
        actionLayout.childForceExpandWidth = true;
        AddLayoutElement(actions, 180f, 0f);

        Button approve = CreateButton(actions.transform, "Approve", new Color(0.12f, 0.42f, 0.22f, 1f), new Vector2(170f, 54f));
        approve.onClick.AddListener(() =>
        {
            receiver.ApproveTask(task);
            task.status = RealWorldTaskStatus.Approved;
            Refresh();
        });

        Button reject = CreateButton(actions.transform, "Reject", new Color(0.52f, 0.16f, 0.14f, 1f), new Vector2(170f, 54f));
        reject.onClick.AddListener(() =>
        {
            receiver.RejectTask(task);
            task.status = RealWorldTaskStatus.Rejected;
            Refresh();
        });
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("RuntimeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas createdCanvas = canvasObject.GetComponent<Canvas>();
        createdCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        return createdCanvas;
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

    private Button CreateButton(Transform parent, string label, Color color, Vector2 size)
    {
        GameObject buttonObject = CreatePanel(parent, label + "Button", color);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();

        TextMeshProUGUI text = CreateText(buttonObject.transform, label, 20, FontStyles.Bold, TextAlignmentOptions.Center);
        Stretch(text.GetComponent<RectTransform>());
        text.raycastTarget = false;

        AddLayoutElement(buttonObject, size.x, size.y);
        return button;
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
}
