using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class RuntimeUiStyle
{
    public const int TitleSize = 40;
    public const int SectionTitleSize = 34;
    public const int CardTitleSize = 30;
    public const int BodySize = 24;
    public const int CaptionSize = 21;
    public const int ButtonTextSize = 30;
    public const float MainButtonWidth = 300f;
    public const float MainButtonHeight = 300f;
    public const float PanelButtonHeight = 72f;
    public const float QuestCardHeight = 260f;

    public static readonly Color Panel = new Color(0.07f, 0.08f, 0.11f, 0.98f);
    public static readonly Color Card = new Color(0.12f, 0.14f, 0.18f, 0.98f);
    public static readonly Color CardAlt = new Color(0.09f, 0.13f, 0.16f, 0.96f);
    public static readonly Color Text = new Color(0.94f, 0.96f, 1f, 1f);
    public static readonly Color MutedText = new Color(0.78f, 0.84f, 0.9f, 1f);
    public static readonly Color Gold = new Color(1f, 0.9f, 0.58f, 1f);
    public static readonly Color Green = new Color(0.22f, 0.78f, 0.38f, 1f);
    public static readonly Color Blue = new Color(0.22f, 0.48f, 0.78f, 1f);
    public static readonly Color Red = new Color(0.72f, 0.25f, 0.22f, 1f);
    public static readonly Color NeutralButton = new Color(0.2f, 0.21f, 0.27f, 1f);

    public static void ApplyText(TextMeshProUGUI label, int size, FontStyles style, Color color, TextAlignmentOptions alignment)
    {
        if (label == null)
            return;

        label.fontSize = size;
        label.fontStyle = style;
        label.color = color;
        label.alignment = alignment;
        label.enableAutoSizing = false;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;
    }

    public static void ApplyButton(Button button, Color color, float width = MainButtonWidth, float height = MainButtonHeight)
    {
        if (button == null)
            return;

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
            button.targetGraphic = image;
        }

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect != null)
            rect.sizeDelta = new Vector2(width, height);

        LayoutElement element = button.GetComponent<LayoutElement>();
        if (element == null)
            element = button.gameObject.AddComponent<LayoutElement>();

        element.preferredWidth = width;
        element.preferredHeight = height;
        element.minWidth = width;
        element.minHeight = height;
    }

    public static void ApplyLayoutElement(GameObject target, float preferredHeight, float flexibleWidth = 1f)
    {
        LayoutElement element = target.GetComponent<LayoutElement>();
        if (element == null)
            element = target.AddComponent<LayoutElement>();

        element.preferredHeight = preferredHeight;
        element.minHeight = preferredHeight;
        element.flexibleWidth = flexibleWidth;
    }

    public static TextMeshProUGUI GetOrCreateButtonLabel(Button button, string defaultText)
    {
        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            return label;

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(button.transform, false);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        RuntimeUiHost.Stretch(rect);

        label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = defaultText;
        return label;
    }
}
