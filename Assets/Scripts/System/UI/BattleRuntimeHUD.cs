using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleRuntimeHUD : MonoBehaviour
{
    private BattleManager battleManager;
    private TextMeshProUGUI waveText;
    private TextMeshProUGUI lootText;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI comboText;
    private TextMeshProUGUI rageText;
    private TextMeshProUGUI powerText;
    private TextMeshProUGUI healText;
    private TextMeshProUGUI rageButtonText;
    private Image healthFill;
    private Image rageFill;
    private Button powerButton;
    private Button healButton;
    private Button rageButton;
    private float powerCooldownRemaining;
    private float healCooldownRemaining;

    private const float PowerCooldown = 8f;
    private const float HealCooldown = 12f;

    public void Initialize(BattleManager manager)
    {
        battleManager = manager;
        Build();
        UpdateWave(manager.CurrentWave, manager.TotalWaves);
        RefreshStats();
    }

    private void Update()
    {
        if (battleManager == null)
            return;

        TickCooldown(ref powerCooldownRemaining, powerButton, powerText, "Power", battleManager.CanCastPowerStrike);
        TickCooldown(ref healCooldownRemaining, healButton, healText, "Heal", battleManager.CanCastHeal);
        UpdateRageButton();
        RefreshStats();
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

        RefreshStats();
    }

    public void RefreshStats()
    {
        if (battleManager == null)
            return;

        int currentHealth = battleManager.PlayerHealth;
        int maxHealth = Mathf.Max(1, battleManager.PlayerMaxHealth);
        float healthRatio = Mathf.Clamp01((float)currentHealth / maxHealth);
        float rageRatio = Mathf.Clamp01(battleManager.RageCharge / 100f);

        if (healthText != null)
            healthText.text = $"HP {currentHealth}/{maxHealth}";

        if (healthFill != null)
            healthFill.fillAmount = healthRatio;

        if (comboText != null)
            comboText.text = battleManager.ComboCount > 1 ? $"Combo x{battleManager.ComboCount}" : "Combo ready";

        if (rageText != null)
            rageText.text = $"Rage {battleManager.RageCharge}%";

        if (rageFill != null)
            rageFill.fillAmount = rageRatio;

        if (lootText != null)
            lootText.text = $"{battleManager.TotalGold} gold  |  {battleManager.TotalExp} XP";
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

    private void CastRage()
    {
        if (battleManager == null)
            return;

        battleManager.CastRageBurst();
    }

    private void TickCooldown(ref float cooldown, Button button, TextMeshProUGUI label, string readyText, bool canUse)
    {
        if (cooldown > 0f)
            cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);

        bool isReady = cooldown <= 0f && canUse;
        if (button != null)
            button.interactable = isReady;

        if (label == null)
            return;

        label.text = cooldown > 0f ? $"{Mathf.CeilToInt(cooldown)}s" : readyText;
    }

    private void UpdateRageButton()
    {
        if (rageButton != null)
            rageButton.interactable = battleManager.CanCastRageBurst;

        if (rageButtonText != null)
            rageButtonText.text = battleManager.RageCharge >= 100 ? "Rage" : $"{battleManager.RageCharge}%";
    }

    private void Build()
    {
        Canvas canvas = CreateCanvas();

        GameObject topPanel = CreatePanel(canvas.transform, "BattleTopPanel", new Color(0.05f, 0.06f, 0.08f, 0.84f));
        RectTransform topRect = topPanel.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0.5f, 1f);
        topRect.anchorMax = new Vector2(0.5f, 1f);
        topRect.pivot = new Vector2(0.5f, 1f);
        topRect.sizeDelta = new Vector2(820f, 112f);
        topRect.anchoredPosition = new Vector2(0f, -24f);

        HorizontalLayoutGroup topLayout = topPanel.AddComponent<HorizontalLayoutGroup>();
        topLayout.padding = new RectOffset(24, 24, 18, 18);
        topLayout.spacing = 18f;
        topLayout.childAlignment = TextAnchor.MiddleCenter;
        topLayout.childControlHeight = true;
        topLayout.childControlWidth = true;

        waveText = CreateText(topPanel.transform, "Wave 1/1", 30, FontStyles.Bold);
        lootText = CreateText(topPanel.transform, "0 gold  |  0 XP", 26, FontStyles.Bold);
        lootText.color = new Color(1f, 0.88f, 0.48f, 1f);

        GameObject statsPanel = CreatePanel(canvas.transform, "PlayerStatsPanel", new Color(0.06f, 0.07f, 0.1f, 0.88f));
        RectTransform statsRect = statsPanel.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0f, 1f);
        statsRect.anchorMax = new Vector2(0f, 1f);
        statsRect.pivot = new Vector2(0f, 1f);
        statsRect.sizeDelta = new Vector2(400f, 210f);
        statsRect.anchoredPosition = new Vector2(28f, -156f);

        VerticalLayoutGroup statsLayout = statsPanel.AddComponent<VerticalLayoutGroup>();
        statsLayout.padding = new RectOffset(22, 22, 18, 18);
        statsLayout.spacing = 12f;
        statsLayout.childControlWidth = true;
        statsLayout.childControlHeight = true;
        statsLayout.childForceExpandHeight = false;

        healthText = CreateText(statsPanel.transform, "HP 0/0", 26, FontStyles.Bold);
        healthText.alignment = TextAlignmentOptions.Left;
        healthFill = CreateBar(statsPanel.transform, "HealthBar", new Color(0.12f, 0.16f, 0.18f, 1f), new Color(0.22f, 0.86f, 0.34f, 1f));

        comboText = CreateText(statsPanel.transform, "Combo ready", 24, FontStyles.Bold);
        comboText.alignment = TextAlignmentOptions.Left;
        comboText.color = new Color(0.7f, 0.86f, 1f, 1f);

        rageText = CreateText(statsPanel.transform, "Rage 0%", 22, FontStyles.Bold);
        rageText.alignment = TextAlignmentOptions.Left;
        rageFill = CreateBar(statsPanel.transform, "RageBar", new Color(0.16f, 0.1f, 0.12f, 1f), new Color(1f, 0.32f, 0.18f, 1f));

        GameObject abilityPanel = CreatePanel(canvas.transform, "AbilityPanel", new Color(0f, 0f, 0f, 0f));
        RectTransform abilityRect = abilityPanel.GetComponent<RectTransform>();
        abilityRect.anchorMin = new Vector2(0.5f, 0f);
        abilityRect.anchorMax = new Vector2(0.5f, 0f);
        abilityRect.pivot = new Vector2(0.5f, 0f);
        abilityRect.sizeDelta = new Vector2(760f, 130f);
        abilityRect.anchoredPosition = new Vector2(0f, 42f);

        HorizontalLayoutGroup layout = abilityPanel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        powerButton = CreateAbilityButton(abilityPanel.transform, "Power", new Color(0.55f, 0.14f, 0.16f, 0.95f), out powerText);
        powerButton.onClick.AddListener(CastPower);

        healButton = CreateAbilityButton(abilityPanel.transform, "Heal", new Color(0.13f, 0.42f, 0.24f, 0.95f), out healText);
        healButton.onClick.AddListener(CastHeal);

        rageButton = CreateAbilityButton(abilityPanel.transform, "Rage", new Color(0.62f, 0.2f, 0.08f, 0.95f), out rageButtonText);
        rageButton.onClick.AddListener(CastRage);
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

    private Image CreateBar(Transform parent, string name, Color backgroundColor, Color fillColor)
    {
        GameObject root = CreatePanel(parent, name, backgroundColor);
        LayoutElement rootElement = root.AddComponent<LayoutElement>();
        rootElement.preferredHeight = 18f;

        Image fill = CreatePanel(root.transform, "Fill", fillColor).GetComponent<Image>();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 1f;
        Stretch(fill.GetComponent<RectTransform>());

        return fill;
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
