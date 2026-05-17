using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardPopup : MonoBehaviour
{
    private static RewardPopup instance;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform panelRect;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI rewardText;
    private Coroutine animationRoutine;

    public static void ShowReward(string title, int gold, int experience)
    {
        string message = BuildRewardMessage(gold, experience);
        Show(title, message);
    }

    public static void ShowMessage(string title, string message)
    {
        Show(title, message);
    }

    private static void Show(string title, string message)
    {
        RewardPopup popup = GetOrCreate();
        popup.ShowInternal(title, message);
    }

    private static RewardPopup GetOrCreate()
    {
        if (instance != null)
            return instance;

        GameObject popupObject = new GameObject("RewardPopup");
        instance = popupObject.AddComponent<RewardPopup>();
        DontDestroyOnLoad(popupObject);
        instance.Build();
        return instance;
    }

    private static string BuildRewardMessage(int gold, int experience)
    {
        if (gold <= 0 && experience <= 0)
            return "No reward this time";

        if (gold > 0 && experience > 0)
            return $"+{gold} gold\n+{experience} XP";

        if (gold > 0)
            return $"+{gold} gold";

        return $"+{experience} XP";
    }

    private void Build()
    {
        canvas = CreateCanvas();

        GameObject blocker = CreateUIObject("RewardPopupBlocker", canvas.transform);
        Stretch(blocker.GetComponent<RectTransform>());
        canvasGroup = blocker.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        GameObject panel = CreatePanel(blocker.transform, "Panel", new Color(0.08f, 0.09f, 0.12f, 0.96f));
        panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(560f, 300f);
        panelRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(34, 34, 30, 30);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        titleText = CreateText(panel.transform, "Reward", 36, FontStyles.Bold);
        titleText.color = new Color(1f, 0.92f, 0.55f, 1f);

        rewardText = CreateText(panel.transform, "+0 gold", 30, FontStyles.Bold);
        rewardText.color = Color.white;

        gameObject.SetActive(false);
    }

    private void ShowInternal(string title, string message)
    {
        titleText.text = title;
        rewardText.text = message;

        gameObject.SetActive(true);
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        canvasGroup.blocksRaycasts = true;
        panelRect.localScale = Vector3.one * 0.72f;

        yield return AnimatePhase(0f, 1f, 0.72f, 1.06f, 0.18f);
        yield return AnimatePhase(1f, 1f, 1.06f, 1f, 0.12f);
        yield return new WaitForSeconds(1.15f);
        yield return AnimatePhase(1f, 0f, 1f, 0.92f, 0.22f);

        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private IEnumerator AnimatePhase(float startAlpha, float endAlpha, float startScale, float endScale, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, eased);
            panelRect.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, eased);

            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        panelRect.localScale = Vector3.one * endScale;
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("RewardPopupCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas popupCanvas = canvasObject.GetComponent<Canvas>();
        popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        popupCanvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        return popupCanvas;
    }

    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = CreateUIObject(name, parent);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private TextMeshProUGUI CreateText(Transform parent, string text, int fontSize, FontStyles style)
    {
        GameObject textObject = CreateUIObject("Text", parent);
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = TextAlignmentOptions.Center;
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
