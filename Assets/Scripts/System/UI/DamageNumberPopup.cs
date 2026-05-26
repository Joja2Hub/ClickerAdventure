using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageNumberPopup : MonoBehaviour
{
    private static Canvas canvas;

    private RectTransform rectTransform;
    private TextMeshProUGUI damageText;
    private CanvasGroup canvasGroup;

    public static void Show(Vector3 worldPosition, int damage)
    {
        ShowDamage(worldPosition, damage, false, null);
    }

    public static void ShowDamage(Vector3 worldPosition, int damage, bool isCritical, string prefix)
    {
        string text = string.IsNullOrEmpty(prefix) ? $"-{damage}" : $"{prefix}\n-{damage}";
        Color color = isCritical ? new Color(1f, 0.22f, 0.16f, 1f) : new Color(1f, 0.82f, 0.2f, 1f);
        float size = isCritical ? 54f : 42f;
        ShowText(worldPosition, text, color, size);
    }

    public static void ShowText(Vector3 worldPosition, string text, Color color, float fontSize)
    {
        Canvas popupCanvas = GetCanvas();
        GameObject popupObject = new GameObject("DamageNumber", typeof(RectTransform), typeof(CanvasGroup));
        popupObject.transform.SetParent(popupCanvas.transform, false);

        DamageNumberPopup popup = popupObject.AddComponent<DamageNumberPopup>();
        popup.Initialize(worldPosition, text, color, fontSize);
    }

    private static Canvas GetCanvas()
    {
        if (canvas != null)
            return canvas;

        canvas = RuntimeUiHost.GetCanvas(null, 1100);
        return canvas;
    }

    private void Initialize(Vector3 worldPosition, string text, Color color, float fontSize)
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        damageText = gameObject.AddComponent<TextMeshProUGUI>();
        damageText.text = text;
        damageText.fontSize = fontSize;
        damageText.fontStyle = FontStyles.Bold;
        damageText.alignment = TextAlignmentOptions.Center;
        damageText.color = color;
        damageText.raycastTarget = false;

        rectTransform.sizeDelta = new Vector2(220f, 110f);

        Vector3 screenPosition = Camera.main != null
            ? Camera.main.WorldToScreenPoint(worldPosition)
            : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

        screenPosition += new Vector3(Random.Range(-28f, 28f), Random.Range(20f, 48f), 0f);
        rectTransform.position = screenPosition;

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        const float duration = 0.72f;
        float elapsed = 0f;
        Vector2 start = rectTransform.anchoredPosition;
        Vector2 end = start + new Vector2(0f, 88f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            rectTransform.anchoredPosition = Vector2.Lerp(start, end, eased);
            rectTransform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.08f, Mathf.Sin(t * Mathf.PI));
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        Destroy(gameObject);
    }
}
