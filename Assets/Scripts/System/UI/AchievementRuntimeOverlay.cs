using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementRuntimeOverlay : MonoBehaviour
{
    private AchievementProgress achievementProgress;
    private DailyRoutineProgress routineProgress;
    private Canvas canvas;
    private CanvasGroup panelGroup;
    private RectTransform panelRect;
    private TextMeshProUGUI summaryText;
    private Transform listRoot;

    public void Initialize()
    {
        if (canvas != null)
            return;

        achievementProgress = AchievementProgress.Instance;
        routineProgress = DailyRoutineProgress.Instance;

        Build();
        Subscribe();
        Refresh();
        HidePanel();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (achievementProgress != null)
            achievementProgress.OnProgressChanged += Refresh;

        if (routineProgress != null)
            routineProgress.OnProgressChanged += Refresh;
    }

    private void Unsubscribe()
    {
        if (achievementProgress != null)
            achievementProgress.OnProgressChanged -= Refresh;

        if (routineProgress != null)
            routineProgress.OnProgressChanged -= Refresh;
    }

    private void ShowPanel()
    {
        Refresh();
        panelGroup.alpha = 1f;
        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;
        panelRect.localScale = Vector3.one;
    }

    private void HidePanel()
    {
        panelGroup.alpha = 0f;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;
        panelRect.localScale = Vector3.one * 0.96f;
    }

    private void Refresh()
    {
        if (achievementProgress == null)
            achievementProgress = AchievementProgress.Instance;

        if (routineProgress == null)
            routineProgress = DailyRoutineProgress.Instance;

        if (achievementProgress == null || routineProgress == null || listRoot == null)
            return;

        routineProgress.ResetForCurrentDayIfNeeded();

        summaryText.text = $"{achievementProgress.UnlockedCount}/{achievementProgress.TotalAchievements} unlocked  |  {achievementProgress.TotalRealTasks} real tasks  |  Streak {routineProgress.CurrentStreak}";

        foreach (Transform child in listRoot)
            Destroy(child.gameObject);

        AchievementDefinition[] achievements = achievementProgress.GetAchievements();
        foreach (AchievementDefinition achievement in achievements)
            CreateAchievementRow(listRoot, achievement);
    }

    private void CreateAchievementRow(Transform parent, AchievementDefinition achievement)
    {
        bool unlocked = achievementProgress.IsUnlocked(achievement.Id);
        int current = GetCurrentProgress(achievement);
        int target = GetTargetProgress(achievement);
        float progress = target <= 0 ? 1f : Mathf.Clamp01((float)current / target);

        GameObject row = CreateColoredObject(parent, achievement.Id, unlocked
            ? new Color(0.12f, 0.2f, 0.16f, 1f)
            : new Color(0.12f, 0.13f, 0.16f, 1f));

        LayoutElement rowElement = row.AddComponent<LayoutElement>();
        rowElement.preferredHeight = 132f;

        HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.padding = new RectOffset(22, 22, 18, 18);
        rowLayout.spacing = 18f;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;

        TextMeshProUGUI iconText = CreateText(row.transform, unlocked ? "OK" : "GO", 24, FontStyles.Bold);
        iconText.color = unlocked ? new Color(0.54f, 0.95f, 0.62f, 1f) : new Color(0.78f, 0.84f, 0.94f, 1f);
        LayoutElement iconElement = iconText.gameObject.AddComponent<LayoutElement>();
        iconElement.preferredWidth = 72f;

        GameObject info = CreateUIObject("Info", row.transform);
        LayoutElement infoElement = info.AddComponent<LayoutElement>();
        infoElement.preferredWidth = 420f;
        infoElement.flexibleWidth = 1f;

        VerticalLayoutGroup infoLayout = info.AddComponent<VerticalLayoutGroup>();
        infoLayout.spacing = 6f;
        infoLayout.childAlignment = TextAnchor.MiddleLeft;
        infoLayout.childControlWidth = true;
        infoLayout.childControlHeight = true;
        infoLayout.childForceExpandHeight = false;

        TextMeshProUGUI titleText = CreateText(info.transform, achievement.Title, 25, FontStyles.Bold);
        titleText.alignment = TextAlignmentOptions.Left;
        titleText.color = unlocked ? new Color(1f, 0.93f, 0.64f, 1f) : Color.white;

        TextMeshProUGUI descriptionText = CreateText(info.transform, achievement.Description, 19, FontStyles.Normal);
        descriptionText.alignment = TextAlignmentOptions.Left;
        descriptionText.color = new Color(0.78f, 0.84f, 0.9f, 1f);

        GameObject progressRoot = CreateColoredObject(info.transform, "Progress", new Color(0.04f, 0.05f, 0.07f, 1f));
        LayoutElement progressElement = progressRoot.AddComponent<LayoutElement>();
        progressElement.preferredHeight = 12f;
        progressElement.flexibleWidth = 1f;

        Image progressFill = CreateColoredObject(progressRoot.transform, "Fill", unlocked
            ? new Color(0.35f, 0.82f, 0.43f, 1f)
            : new Color(0.25f, 0.55f, 0.82f, 1f)).GetComponent<Image>();
        progressFill.type = Image.Type.Filled;
        progressFill.fillMethod = Image.FillMethod.Horizontal;
        progressFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        progressFill.fillAmount = progress;
        Stretch(progressFill.GetComponent<RectTransform>());

        TextMeshProUGUI rewardText = CreateText(row.transform, $"+{achievement.GoldReward}g\n+{achievement.ExperienceReward}xp\n{Mathf.Min(current, target)}/{target}", 20, FontStyles.Bold);
        rewardText.color = unlocked ? new Color(0.55f, 0.95f, 0.62f, 1f) : new Color(0.94f, 0.82f, 0.38f, 1f);
        LayoutElement rewardElement = rewardText.gameObject.AddComponent<LayoutElement>();
        rewardElement.preferredWidth = 124f;
    }

    private int GetCurrentProgress(AchievementDefinition achievement)
    {
        if (achievement.RequiredRealTasks > 0)
            return achievementProgress.TotalRealTasks;

        if (achievement.RequiredStreak > 0)
            return routineProgress.CurrentStreak;

        return 1;
    }

    private int GetTargetProgress(AchievementDefinition achievement)
    {
        if (achievement.RequiredRealTasks > 0)
            return achievement.RequiredRealTasks;

        if (achievement.RequiredStreak > 0)
            return achievement.RequiredStreak;

        return 1;
    }

    private void Build()
    {
        canvas = CreateCanvas();
        CreateOpenButton(canvas.transform);
        CreatePanel(canvas.transform);
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("AchievementRuntimeOverlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas overlayCanvas = canvasObject.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 852;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        return overlayCanvas;
    }

    private void CreateOpenButton(Transform parent)
    {
        Button openButton = CreateButton(parent, "AchievementsButton", "Goals", new Color(0.25f, 0.18f, 0.42f, 0.94f), out _);
        RectTransform rect = openButton.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(170f, 64f);
        rect.anchoredPosition = new Vector2(-30f, -194f);
        openButton.onClick.AddListener(ShowPanel);
    }

    private void CreatePanel(Transform parent)
    {
        GameObject blocker = CreateUIObject("AchievementsBlocker", parent);
        Stretch(blocker.GetComponent<RectTransform>());
        Image blockerImage = blocker.AddComponent<Image>();
        blockerImage.color = new Color(0f, 0f, 0f, 0.46f);

        Button blockerButton = blocker.AddComponent<Button>();
        blockerButton.targetGraphic = blockerImage;
        blockerButton.onClick.AddListener(HidePanel);

        panelGroup = blocker.AddComponent<CanvasGroup>();

        GameObject panel = CreateColoredObject(blocker.transform, "AchievementsPanel", new Color(0.07f, 0.08f, 0.11f, 0.98f));
        panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(820f, 1010f);

        Button panelButton = panel.AddComponent<Button>();
        panelButton.transition = Selectable.Transition.None;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(34, 34, 34, 34);
        layout.spacing = 18f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        TextMeshProUGUI titleText = CreateText(panel.transform, "Achievements", 38, FontStyles.Bold);
        titleText.color = new Color(1f, 0.9f, 0.58f, 1f);

        summaryText = CreateText(panel.transform, "0/0 unlocked", 23, FontStyles.Bold);
        summaryText.color = new Color(0.82f, 0.88f, 0.95f, 1f);

        CreateDivider(panel.transform);

        GameObject list = CreateUIObject("AchievementList", panel.transform);
        listRoot = list.transform;
        VerticalLayoutGroup listLayout = list.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 12f;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = true;
        listLayout.childForceExpandHeight = false;

        LayoutElement listElement = list.AddComponent<LayoutElement>();
        listElement.flexibleHeight = 1f;

        CreateDivider(panel.transform);

        Button closeButton = CreateButton(panel.transform, "CloseButton", "Close", new Color(0.19f, 0.2f, 0.24f, 1f), out _);
        LayoutElement closeElement = closeButton.gameObject.AddComponent<LayoutElement>();
        closeElement.preferredHeight = 64f;
        closeButton.onClick.AddListener(HidePanel);
    }

    private Button CreateButton(Transform parent, string name, string label, Color color, out TextMeshProUGUI labelText)
    {
        GameObject buttonObject = CreateColoredObject(parent, name, color);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();

        labelText = CreateText(buttonObject.transform, label, 25, FontStyles.Bold);
        labelText.raycastTarget = false;
        Stretch(labelText.GetComponent<RectTransform>());

        return button;
    }

    private void CreateDivider(Transform parent)
    {
        GameObject divider = CreateColoredObject(parent, "Divider", new Color(1f, 1f, 1f, 0.12f));
        LayoutElement element = divider.AddComponent<LayoutElement>();
        element.preferredHeight = 2f;
    }

    private GameObject CreateColoredObject(Transform parent, string name, Color color)
    {
        GameObject uiObject = CreateUIObject(name, parent);
        Image image = uiObject.AddComponent<Image>();
        image.color = color;
        return uiObject;
    }

    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private TextMeshProUGUI CreateText(Transform parent, string text, int fontSize, FontStyles style)
    {
        GameObject textObject = CreateUIObject("Text", parent);
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = TextAlignmentOptions.Center;
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
}
