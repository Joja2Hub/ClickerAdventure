using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealWorldTaskNotificationToast : MonoBehaviour
{
    private static RealWorldTaskNotificationToast instance;

    private CanvasGroup canvasGroup;
    private RectTransform panelRect;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI messageText;
    private Image panelImage;
    private Coroutine animationRoutine;

    public static void Show(string title, string message, Color accentColor)
    {
        RealWorldTaskNotificationToast toast = GetOrCreate();
        toast.ShowInternal(title, message, accentColor);
    }

    private static RealWorldTaskNotificationToast GetOrCreate()
    {
        if (instance != null)
            return instance;

        GameObject toastObject = new GameObject("RealWorldTaskNotificationToast");
        instance = toastObject.AddComponent<RealWorldTaskNotificationToast>();
        DontDestroyOnLoad(toastObject);
        instance.Build();
        return instance;
    }

    private void ShowInternal(string title, string message, Color accentColor)
    {
        titleText.text = title;
        messageText.text = message;
        panelImage.color = accentColor;

        gameObject.SetActive(true);
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        canvasGroup.alpha = 0f;
        panelRect.anchoredPosition = new Vector2(0f, 120f);

        yield return AnimatePhase(0f, 1f, 120f, -32f, 0.22f);
        yield return new WaitForSeconds(2.35f);
        yield return AnimatePhase(1f, 0f, -32f, -112f, 0.24f);

        gameObject.SetActive(false);
    }

    private IEnumerator AnimatePhase(float startAlpha, float endAlpha, float startY, float endY, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, eased);
            panelRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(startY, endY, eased));
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        panelRect.anchoredPosition = new Vector2(0f, endY);
    }

    private void Build()
    {
        GameObject canvasObject = new GameObject("RealWorldTaskNotificationCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1180;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("ToastPanel", typeof(RectTransform), typeof(CanvasGroup));
        panel.transform.SetParent(canvasObject.transform, false);
        panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.sizeDelta = new Vector2(740f, 132f);
        panelRect.anchoredPosition = new Vector2(0f, 120f);
        canvasGroup = panel.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;

        panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.2f, 0.28f, 0.96f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(28, 28, 18, 18);
        layout.spacing = 4f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        titleText = CreateText(panel.transform, "Task update", 27, FontStyles.Bold, new Color(1f, 0.94f, 0.68f, 1f));
        titleText.alignment = TextAlignmentOptions.Left;

        messageText = CreateText(panel.transform, "Message", 21, FontStyles.Normal, Color.white);
        messageText.alignment = TextAlignmentOptions.Left;

        gameObject.SetActive(false);
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
}
