using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParentTaskReviewPopup : MonoBehaviour
{
    private static ParentTaskReviewPopup instance;

    private CanvasGroup canvasGroup;
    private RectTransform panelRect;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI detailsText;
    private TextMeshProUGUI actionText;
    private TMP_InputField noteInput;
    private Image actionButtonImage;
    private Action<string> submitCallback;
    private Color actionColor;

    public static void Show(ExternalQuestData task, bool approve, Action<string> onSubmit)
    {
        if (task == null)
            return;

        ParentTaskReviewPopup popup = GetOrCreate();
        popup.ShowInternal(task, approve, onSubmit);
    }

    private static ParentTaskReviewPopup GetOrCreate()
    {
        if (instance != null)
            return instance;

        GameObject popupObject = new GameObject("ParentTaskReviewPopup");
        instance = popupObject.AddComponent<ParentTaskReviewPopup>();
        DontDestroyOnLoad(popupObject);
        instance.Build();
        return instance;
    }

    private void ShowInternal(ExternalQuestData task, bool approve, Action<string> onSubmit)
    {
        submitCallback = onSubmit;
        actionColor = approve ? new Color(0.12f, 0.42f, 0.22f, 1f) : new Color(0.52f, 0.16f, 0.14f, 1f);

        titleText.text = approve ? "Approve task" : "Send back";
        detailsText.text = BuildDetails(task);
        actionText.text = approve ? "Approve" : "Reject";
        actionButtonImage.color = actionColor;
        noteInput.text = string.Empty;

        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        panelRect.localScale = Vector3.one;
        noteInput.ActivateInputField();
    }

    private string BuildDetails(ExternalQuestData task)
    {
        string childNote = string.IsNullOrWhiteSpace(task.childNote) ? "No child note." : task.childNote;
        return $"{task.questName}\nReward: {task.rewardGold} gold / {task.rewardXP} XP\nChild note: {childNote}";
    }

    private void Submit()
    {
        string note = noteInput != null ? noteInput.text.Trim() : string.Empty;
        submitCallback?.Invoke(note);
        Hide();
    }

    private void Hide()
    {
        submitCallback = null;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        panelRect.localScale = Vector3.one * 0.96f;
        gameObject.SetActive(false);
    }

    private void Build()
    {
        GameObject canvasObject = new GameObject("ParentTaskReviewCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1160;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject blocker = CreateUIObject("ReviewBlocker", canvasObject.transform);
        Stretch(blocker.GetComponent<RectTransform>());
        Image blockerImage = blocker.AddComponent<Image>();
        blockerImage.color = new Color(0f, 0f, 0f, 0.5f);
        canvasGroup = blocker.AddComponent<CanvasGroup>();

        GameObject panel = CreatePanel(blocker.transform, "Panel", new Color(0.07f, 0.08f, 0.11f, 0.98f));
        panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(760f, 610f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(34, 34, 30, 30);
        layout.spacing = 16f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        titleText = CreateText(panel.transform, "Review task", 34, FontStyles.Bold, TextAlignmentOptions.Center);
        titleText.color = new Color(1f, 0.92f, 0.62f, 1f);
        AddLayoutElement(titleText.gameObject, 0f, 48f, 1f);

        detailsText = CreateText(panel.transform, "Task details", 22, FontStyles.Normal, TextAlignmentOptions.Center);
        detailsText.color = new Color(0.84f, 0.88f, 0.94f, 1f);
        AddLayoutElement(detailsText.gameObject, 0f, 118f, 1f);

        noteInput = CreateInput(panel.transform, "Optional note for the child", 0f, 190f);
        noteInput.lineType = TMP_InputField.LineType.MultiLineNewline;

        GameObject actions = CreateUIObject("Actions", panel.transform);
        HorizontalLayoutGroup actionsLayout = actions.AddComponent<HorizontalLayoutGroup>();
        actionsLayout.spacing = 18f;
        actionsLayout.childAlignment = TextAnchor.MiddleCenter;
        actionsLayout.childControlWidth = true;
        actionsLayout.childControlHeight = true;
        actionsLayout.childForceExpandWidth = false;
        AddLayoutElement(actions, 0f, 74f, 1f);

        Button cancelButton = CreateButton(actions.transform, "Cancel", new Color(0.22f, 0.23f, 0.28f, 1f), new Vector2(210f, 66f), out _);
        cancelButton.onClick.AddListener(Hide);

        Button actionButton = CreateButton(actions.transform, "Approve", new Color(0.12f, 0.42f, 0.22f, 1f), new Vector2(230f, 66f), out actionText);
        actionButtonImage = actionButton.GetComponent<Image>();
        actionButton.onClick.AddListener(Submit);

        Hide();
    }

    private TMP_InputField CreateInput(Transform parent, string placeholder, float preferredWidth, float preferredHeight)
    {
        GameObject inputObject = CreatePanel(parent, "ReviewNoteInput", new Color(0.04f, 0.05f, 0.08f, 1f));
        AddLayoutElement(inputObject, preferredWidth, preferredHeight, preferredWidth <= 0f ? 1f : 0f);

        TMP_InputField input = inputObject.AddComponent<TMP_InputField>();
        input.textViewport = inputObject.GetComponent<RectTransform>();

        TextMeshProUGUI text = CreateText(inputObject.transform, string.Empty, 23, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 12f);
        textRect.offsetMax = new Vector2(-18f, -12f);
        input.textComponent = text;

        TextMeshProUGUI placeholderText = CreateText(inputObject.transform, placeholder, 22, FontStyles.Italic, TextAlignmentOptions.TopLeft);
        placeholderText.color = new Color(0.52f, 0.56f, 0.64f, 1f);
        RectTransform placeholderRect = placeholderText.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(18f, 12f);
        placeholderRect.offsetMax = new Vector2(-18f, -12f);
        input.placeholder = placeholderText;

        return input;
    }

    private Button CreateButton(Transform parent, string label, Color color, Vector2 size, out TextMeshProUGUI labelText)
    {
        GameObject buttonObject = CreatePanel(parent, label + "Button", color);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();

        labelText = CreateText(buttonObject.transform, label, 24, FontStyles.Bold, TextAlignmentOptions.Center);
        labelText.raycastTarget = false;
        Stretch(labelText.GetComponent<RectTransform>());

        AddLayoutElement(buttonObject, size.x, size.y);
        return button;
    }

    private GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = CreateUIObject(name, parent);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private TextMeshProUGUI CreateText(Transform parent, string text, int fontSize, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject textObject = CreateUIObject("Text", parent);
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
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

    private void AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight, float flexibleWidth = 0f)
    {
        LayoutElement layoutElement = target.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = target.AddComponent<LayoutElement>();

        if (preferredWidth > 0f)
            layoutElement.preferredWidth = preferredWidth;

        if (preferredHeight > 0f)
            layoutElement.preferredHeight = preferredHeight;

        layoutElement.flexibleWidth = flexibleWidth;
    }
}
