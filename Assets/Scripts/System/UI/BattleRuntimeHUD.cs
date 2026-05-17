using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleRuntimeHUD : MonoBehaviour
{
    private BattleManager battleManager;
    private TextMeshProUGUI waveText;
    private TextMeshProUGUI powerText;
    private TextMeshProUGUI healText;
    private Button powerButton;
    private Button healButton;
    private float powerCooldownRemaining;
    private float healCooldownRemaining;

    private const float PowerCooldown = 8f;
    private const float HealCooldown = 12f;

    public void Initialize(BattleManager manager)
    {
        battleManager = manager;
        Build();
        UpdateWave(manager.CurrentWave, manager.TotalWaves);
    }

    private void Update()
    {
        TickCooldown(ref powerCooldownRemaining, PowerCooldown, powerButton, powerText, "Power");
        TickCooldown(ref healCooldownRemaining, HealCooldown, healButton, healText, "Heal");
    }

    public void UpdateWave(int currentWave, int totalWaves)
    {
        if (waveText != null)
            waveText.text = $"Wave {currentWave}/{totalWaves}";
    }

    public void SetWaitingForEnemy()
    {
        if (waveText != null && battleManager != null)
            waveText.text = $"Wave {battleManager.CurrentWave}/{battleManager.TotalWaves}";
    }

    private void CastPower()
    {
        if (powerCooldownRemaining > 0f || battleManager == null)
            return;

        if (battleManager.CastPowerStrike())
            powerCooldownRemaining = PowerCooldown;
    }

    private void CastHeal()
    {
        if (healCooldownRemaining > 0f || battleManager == null)
            return;

        if (battleManager.CastHeal())
            healCooldownRemaining = HealCooldown;
    }

    private void TickCooldown(ref float cooldown, float duration, Button button, TextMeshProUGUI label, string readyText)
    {
        if (cooldown > 0f)
            cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);

        if (button != null)
            button.interactable = cooldown <= 0f;

        if (label == null)
            return;

        label.text = cooldown > 0f ? $"{Mathf.CeilToInt(cooldown)}s" : readyText;
    }

    private void Build()
    {
        Canvas canvas = CreateCanvas();

        GameObject topPanel = CreatePanel(canvas.transform, "WavePanel", new Color(0.05f, 0.06f, 0.08f, 0.78f));
        RectTransform topRect = topPanel.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0.5f, 1f);
        topRect.anchorMax = new Vector2(0.5f, 1f);
        topRect.pivot = new Vector2(0.5f, 1f);
        topRect.sizeDelta = new Vector2(360f, 70f);
        topRect.anchoredPosition = new Vector2(0f, -24f);

        waveText = CreateText(topPanel.transform, "Wave 1/1", 30, FontStyles.Bold);
        Stretch(waveText.GetComponent<RectTransform>());

        GameObject abilityPanel = CreatePanel(canvas.transform, "AbilityPanel", new Color(0f, 0f, 0f, 0f));
        RectTransform abilityRect = abilityPanel.GetComponent<RectTransform>();
        abilityRect.anchorMin = new Vector2(0.5f, 0f);
        abilityRect.anchorMax = new Vector2(0.5f, 0f);
        abilityRect.pivot = new Vector2(0.5f, 0f);
        abilityRect.sizeDelta = new Vector2(520f, 130f);
        abilityRect.anchoredPosition = new Vector2(0f, 42f);

        HorizontalLayoutGroup layout = abilityPanel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 22f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        powerButton = CreateAbilityButton(abilityPanel.transform, "Power", new Color(0.55f, 0.14f, 0.16f, 0.95f), out powerText);
        powerButton.onClick.AddListener(CastPower);

        healButton = CreateAbilityButton(abilityPanel.transform, "Heal", new Color(0.13f, 0.42f, 0.24f, 0.95f), out healText);
        healButton.onClick.AddListener(CastHeal);
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("BattleRuntimeHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private Button CreateAbilityButton(Transform parent, string label, Color color, out TextMeshProUGUI labelText)
    {
        GameObject buttonObject = CreatePanel(parent, label + "Button", color);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(210f, 98f);

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 210f;
        layoutElement.preferredHeight = 98f;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();

        labelText = CreateText(buttonObject.transform, label, 28, FontStyles.Bold);
        Stretch(labelText.GetComponent<RectTransform>());
        labelText.raycastTarget = false;

        return button;
    }

    private GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private TextMeshProUGUI CreateText(Transform parent, string text, int size, FontStyles style)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
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
