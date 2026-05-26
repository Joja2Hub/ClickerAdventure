using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParentAccessGatePopup : MonoBehaviour
{
    private static ParentAccessGatePopup instance;
    private const string PinKey = "ParentAccessPin";

    private CanvasGroup canvasGroup;
    private RectTransform panelRect;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI messageText;
    private TextMeshProUGUI errorText;
    private TextMeshProUGUI actionText;
    private TMP_InputField pinInput;
    private TMP_InputField confirmInput;
    private Action unlockCallback;
    private bool isSetupMode;

    public static bool IsUnlockedThisSession { get; private set; }

    public static void RequestAccess(Action onUnlocked)
    {
        if (IsUnlockedThisSession)
        {
            onUnlocked?.Invoke();
            return;
        }

        ParentAccessGatePopup popup = GetOrCreate();
        popup.ShowInternal(onUnlocked);
    }

    public static void LockSession()
    {
        IsUnlockedThisSession = false;
    }

    private static ParentAccessGatePopup GetOrCreate()
    {
        if (instance != null)
            return instance;

        GameObject popupObject = new GameObject("ParentAccessGatePopup");
        instance = popupObject.AddComponent<ParentAccessGatePopup>();
        instance.Build();
        return instance;
    }

    private void ShowInternal(Action onUnlocked)
    {
        unlockCallback = onUnlocked;
        isSetupMode = !PlayerPrefs.HasKey(PinKey);

        titleText.text = isSetupMode ? "Set parent PIN" : "Parent lock";
        messageText.text = isSetupMode
            ? "Create a PIN before opening parent controls."
            : "Enter the parent PIN to continue.";
        actionText.text = isSetupMode ? "Save PIN" : "Unlock";
        errorText.text = string.Empty;

        pinInput.text = string.Empty;
        confirmInput.text = string.Empty;
        confirmInput.gameObject.SetActive(isSetupMode);

        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        panelRect.localScale = Vector3.one;
        pinInput.ActivateInputField();
    }

    private void Submit()
    {
        string pin = pinInput.text.Trim();
        if (pin.Length < 4)
        {
            ShowError("PIN must be at least 4 digits.");
            return;
        }

        if (isSetupMode)
        {
            string confirmation = confirmInput.text.Trim();
            if (pin != confirmation)
            {
                ShowError("PINs do not match.");
                return;
            }

            PlayerPrefs.SetString(PinKey, pin);
            PlayerPrefs.Save();
            Unlock();
            return;
        }

        if (pin != PlayerPrefs.GetString(PinKey, string.Empty))
        {
            ShowError("Incorrect PIN.");
            pinInput.text = string.Empty;
            pinInput.ActivateInputField();
            return;
        }

        Unlock();
    }

    private void Unlock()
    {
        IsUnlockedThisSession = true;
        Action callback = unlockCallback;
        Hide();
        callback?.Invoke();
    }

    private void ShowError(string message)
    {
        errorText.text = message;
    }

    private void Hide()
    {
        unlockCallback = null;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        panelRect.localScale = Vector3.one * 0.96f;
        gameObject.SetActive(false);
    }

    private void Build()
    {
        Canvas canvas = RuntimeUiHost.GetCanvas(transform, 1190);
        GameObject blocker = CreateUIObject("ParentAccessBlocker", RuntimeUiHost.GetPopupRoot(canvas));
        blocker.transform.SetAsLastSibling();
        Stretch(blocker.GetComponent<RectTransform>());
        Image blockerImage = blocker.AddComponent<Image>();
        blockerImage.color = new Color(0f, 0f, 0f, 0.52f);
        canvasGroup = blocker.AddComponent<CanvasGroup>();

        GameObject panel = CreatePanel(blocker.transform, "Panel", new Color(0.07f, 0.08f, 0.11f, 0.98f));
        panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(640f, 520f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(34, 34, 30, 30);
        layout.spacing = 16f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        titleText = CreateText(panel.transform, "Parent lock", 34, FontStyles.Bold, TextAlignmentOptions.Center);
        titleText.color = new Color(1f, 0.92f, 0.62f, 1f);
        AddLayoutElement(titleText.gameObject, 0f, 48f, 1f);

        messageText = CreateText(panel.transform, "Enter parent PIN.", 22, FontStyles.Normal, TextAlignmentOptions.Center);
        messageText.color = new Color(0.84f, 0.88f, 0.94f, 1f);
        AddLayoutElement(messageText.gameObject, 0f, 58f, 1f);

        pinInput = CreateInput(panel.transform, "PIN", 0f, 58f);
        pinInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        pinInput.characterLimit = 8;

        confirmInput = CreateInput(panel.transform, "Repeat PIN", 0f, 58f);
        confirmInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        confirmInput.characterLimit = 8;

        errorText = CreateText(panel.transform, string.Empty, 20, FontStyles.Bold, TextAlignmentOptions.Center);
        errorText.color = new Color(1f, 0.42f, 0.36f, 1f);
        AddLayoutElement(errorText.gameObject, 0f, 34f, 1f);

        GameObject actions = CreateUIObject("Actions", panel.transform);
        HorizontalLayoutGroup actionsLayout = actions.AddComponent<HorizontalLayoutGroup>();
        actionsLayout.spacing = 18f;
        actionsLayout.childAlignment = TextAnchor.MiddleCenter;
        actionsLayout.childControlWidth = true;
        actionsLayout.childControlHeight = true;
        actionsLayout.childForceExpandWidth = false;
        AddLayoutElement(actions, 0f, 70f, 1f);

        Button cancelButton = CreateButton(actions.transform, "Cancel", new Color(0.22f, 0.23f, 0.28f, 1f), new Vector2(200f, 62f), out _);
        cancelButton.onClick.AddListener(Hide);

        Button actionButton = CreateButton(actions.transform, "Unlock", new Color(0.12f, 0.38f, 0.28f, 1f), new Vector2(220f, 62f), out actionText);
        actionButton.onClick.AddListener(Submit);

        Hide();
    }

    private TMP_InputField CreateInput(Transform parent, string placeholder, float preferredWidth, float preferredHeight)
    {
        GameObject inputObject = CreatePanel(parent, placeholder + "Input", new Color(0.04f, 0.05f, 0.08f, 1f));
        AddLayoutElement(inputObject, preferredWidth, preferredHeight, preferredWidth <= 0f ? 1f : 0f);

        TMP_InputField input = inputObject.AddComponent<TMP_InputField>();
        input.textViewport = inputObject.GetComponent<RectTransform>();

        TextMeshProUGUI text = CreateText(inputObject.transform, string.Empty, 25, FontStyles.Bold, TextAlignmentOptions.Center);
        StretchWithPadding(text.GetComponent<RectTransform>(), 18f, 8f);
        input.textComponent = text;

        TextMeshProUGUI placeholderText = CreateText(inputObject.transform, placeholder, 22, FontStyles.Italic, TextAlignmentOptions.Center);
        placeholderText.color = new Color(0.52f, 0.56f, 0.64f, 1f);
        StretchWithPadding(placeholderText.GetComponent<RectTransform>(), 18f, 8f);
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

        labelText = CreateText(buttonObject.transform, label, 23, FontStyles.Bold, TextAlignmentOptions.Center);
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

    private void StretchWithPadding(RectTransform rect, float horizontal, float vertical)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(horizontal, vertical);
        rect.offsetMax = new Vector2(-horizontal, -vertical);
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
