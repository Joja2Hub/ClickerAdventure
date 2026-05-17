using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeShopRuntimeOverlay : MonoBehaviour
{
    private PlayerStats playerStats;
    private Canvas canvas;
    private CanvasGroup panelGroup;
    private RectTransform panelRect;
    private TextMeshProUGUI moneyText;
    private TextMeshProUGUI damageText;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI critText;
    private TextMeshProUGUI rageText;
    private TextMeshProUGUI lootText;
    private TextMeshProUGUI damageCostText;
    private TextMeshProUGUI healthCostText;
    private TextMeshProUGUI critCostText;
    private TextMeshProUGUI rageCostText;
    private TextMeshProUGUI lootCostText;
    private Button damageButton;
    private Button healthButton;
    private Button critButton;
    private Button rageButton;
    private Button lootButton;

    public void Initialize()
    {
        if (canvas != null)
            return;

        playerStats = PlayerStats.Instance;
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
        if (playerStats == null)
            return;

        playerStats.OnMoneyChanged += OnMoneyChanged;
        playerStats.OnHealthChanged += OnHealthChanged;
        playerStats.OnStatsChanged += Refresh;
    }

    private void Unsubscribe()
    {
        if (playerStats == null)
            return;

        playerStats.OnMoneyChanged -= OnMoneyChanged;
        playerStats.OnHealthChanged -= OnHealthChanged;
        playerStats.OnStatsChanged -= Refresh;
        playerStats = null;
    }

    private void OnMoneyChanged(int money)
    {
        Refresh();
    }

    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        Refresh();
    }

    private void BuyDamage()
    {
        if (playerStats == null)
            return;

        if (playerStats.UpgradeDamage())
        {
            RewardPopup.ShowMessage("Upgrade purchased", $"+ Damage\nDamage: {playerStats.currentDmg}");
            Refresh();
        }
        else
        {
            RewardPopup.ShowMessage("Need more gold", $"Damage upgrade costs {playerStats.GetDamageUpgradeCost()}");
        }
    }

    private void BuyHealth()
    {
        if (playerStats == null)
            return;

        if (playerStats.UpgradeMaxHealth())
        {
            RewardPopup.ShowMessage("Upgrade purchased", $"+ Max HP\nHP: {playerStats.maxHealth}");
            Refresh();
        }
        else
        {
            RewardPopup.ShowMessage("Need more gold", $"Health upgrade costs {playerStats.GetHealthUpgradeCost()}");
        }
    }

    private void BuyCrit()
    {
        if (playerStats == null)
            return;

        if (playerStats.UpgradeCrit())
        {
            RewardPopup.ShowMessage("Upgrade purchased", $"+ Critical chance\nCrit bonus: {FormatPercent(playerStats.CriticalChanceBonus)}");
            Refresh();
        }
        else
        {
            RewardPopup.ShowMessage("Need more gold", $"Precision costs {playerStats.GetCritUpgradeCost()}");
        }
    }

    private void BuyRage()
    {
        if (playerStats == null)
            return;

        if (playerStats.UpgradeRageGain())
        {
            RewardPopup.ShowMessage("Upgrade purchased", $"+ Rage gain\nRage: {FormatPercent(playerStats.RageGainMultiplier - 1f)} faster");
            Refresh();
        }
        else
        {
            RewardPopup.ShowMessage("Need more gold", $"Focus costs {playerStats.GetRageUpgradeCost()}");
        }
    }

    private void BuyLoot()
    {
        if (playerStats == null)
            return;

        if (playerStats.UpgradeLoot())
        {
            RewardPopup.ShowMessage("Upgrade purchased", $"+ Battle gold\nLoot: {FormatPercent(playerStats.BattleGoldMultiplier - 1f)} bonus");
            Refresh();
        }
        else
        {
            RewardPopup.ShowMessage("Need more gold", $"Treasure sense costs {playerStats.GetLootUpgradeCost()}");
        }
    }

    private void Refresh()
    {
        if (playerStats == null)
            playerStats = PlayerStats.Instance;

        if (playerStats == null)
            return;

        int damageCost = playerStats.GetDamageUpgradeCost();
        int healthCost = playerStats.GetHealthUpgradeCost();
        int critCost = playerStats.GetCritUpgradeCost();
        int rageCost = playerStats.GetRageUpgradeCost();
        int lootCost = playerStats.GetLootUpgradeCost();

        moneyText.text = $"{playerStats.money} gold";
        damageText.text = $"Damage {playerStats.currentDmg}  Lv.{playerStats.damageUpgradeLevel}";
        healthText.text = $"HP {playerStats.currentHealth}/{playerStats.maxHealth}  Lv.{playerStats.healthUpgradeLevel}";
        critText.text = $"Crit +{FormatPercent(playerStats.CriticalChanceBonus)}  Lv.{playerStats.critUpgradeLevel}";
        rageText.text = $"Rage +{FormatPercent(playerStats.RageGainMultiplier - 1f)}  Lv.{playerStats.rageUpgradeLevel}";
        lootText.text = $"Battle gold +{FormatPercent(playerStats.BattleGoldMultiplier - 1f)}  Lv.{playerStats.lootUpgradeLevel}";
        damageCostText.text = $"{damageCost} gold";
        healthCostText.text = $"{healthCost} gold";
        critCostText.text = $"{critCost} gold";
        rageCostText.text = $"{rageCost} gold";
        lootCostText.text = $"{lootCost} gold";

        damageButton.interactable = playerStats.money >= damageCost;
        healthButton.interactable = playerStats.money >= healthCost;
        critButton.interactable = playerStats.money >= critCost;
        rageButton.interactable = playerStats.money >= rageCost;
        lootButton.interactable = playerStats.money >= lootCost;
    }

    private void ShowPanel()
    {
        panelGroup.alpha = 1f;
        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;
        panelRect.localScale = Vector3.one;
        Refresh();
    }

    private void HidePanel()
    {
        panelGroup.alpha = 0f;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;
        panelRect.localScale = Vector3.one * 0.96f;
    }

    private void Build()
    {
        canvas = CreateCanvas();
        CreateOpenButton(canvas.transform);
        CreatePanel(canvas.transform);
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("UpgradeShopRuntimeOverlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas shopCanvas = canvasObject.GetComponent<Canvas>();
        shopCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        shopCanvas.sortingOrder = 850;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        return shopCanvas;
    }

    private void CreateOpenButton(Transform parent)
    {
        Button openButton = CreateButton(parent, "ShopButton", "Shop", new Color(0.11f, 0.28f, 0.43f, 0.94f), out _);
        RectTransform rect = openButton.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(170f, 72f);
        rect.anchoredPosition = new Vector2(-30f, -110f);
        openButton.onClick.AddListener(ShowPanel);
    }

    private void CreatePanel(Transform parent)
    {
        GameObject blocker = CreateUIObject("ShopBlocker", parent);
        Stretch(blocker.GetComponent<RectTransform>());
        Image blockerImage = blocker.AddComponent<Image>();
        blockerImage.color = new Color(0f, 0f, 0f, 0.42f);

        Button blockerButton = blocker.AddComponent<Button>();
        blockerButton.targetGraphic = blockerImage;
        blockerButton.onClick.AddListener(HidePanel);

        panelGroup = blocker.AddComponent<CanvasGroup>();

        GameObject panel = CreateColoredObject(blocker.transform, "ShopPanel", new Color(0.07f, 0.08f, 0.11f, 0.98f));
        panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(780f, 980f);

        Button panelButton = panel.AddComponent<Button>();
        panelButton.transition = Selectable.Transition.None;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(36, 36, 34, 34);
        layout.spacing = 20f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        TextMeshProUGUI titleText = CreateText(panel.transform, "Hero upgrades", 38, FontStyles.Bold);
        titleText.color = new Color(1f, 0.9f, 0.58f, 1f);

        moneyText = CreateText(panel.transform, "0 gold", 30, FontStyles.Bold);
        moneyText.color = new Color(0.94f, 0.82f, 0.38f, 1f);

        CreateDivider(panel.transform);
        CreateUpgradeRow(panel.transform, "DamageTraining", "Train attack", new Color(0.48f, 0.11f, 0.15f, 0.95f), out damageText, out damageCostText, out damageButton);
        damageButton.onClick.AddListener(BuyDamage);

        CreateUpgradeRow(panel.transform, "HealthTraining", "Fortify health", new Color(0.12f, 0.34f, 0.21f, 0.95f), out healthText, out healthCostText, out healthButton);
        healthButton.onClick.AddListener(BuyHealth);

        CreateUpgradeRow(panel.transform, "PrecisionTraining", "Precision", new Color(0.42f, 0.2f, 0.5f, 0.95f), out critText, out critCostText, out critButton);
        critButton.onClick.AddListener(BuyCrit);

        CreateUpgradeRow(panel.transform, "FocusTraining", "Battle focus", new Color(0.62f, 0.22f, 0.08f, 0.95f), out rageText, out rageCostText, out rageButton);
        rageButton.onClick.AddListener(BuyRage);

        CreateUpgradeRow(panel.transform, "LootTraining", "Treasure sense", new Color(0.36f, 0.29f, 0.1f, 0.95f), out lootText, out lootCostText, out lootButton);
        lootButton.onClick.AddListener(BuyLoot);

        CreateDivider(panel.transform);

        Button closeButton = CreateButton(panel.transform, "CloseButton", "Close", new Color(0.19f, 0.2f, 0.24f, 1f), out _);
        closeButton.onClick.AddListener(HidePanel);
    }

    private void CreateUpgradeRow(Transform parent, string name, string title, Color buttonColor, out TextMeshProUGUI statText, out TextMeshProUGUI costText, out Button buyButton)
    {
        GameObject row = CreateColoredObject(parent, name, new Color(0.12f, 0.13f, 0.16f, 1f));
        LayoutElement rowElement = row.AddComponent<LayoutElement>();
        rowElement.preferredHeight = 118f;

        HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.padding = new RectOffset(26, 20, 18, 18);
        rowLayout.spacing = 18f;
        rowLayout.childControlHeight = true;
        rowLayout.childControlWidth = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;

        GameObject info = CreateUIObject("Info", row.transform);
        LayoutElement infoElement = info.AddComponent<LayoutElement>();
        infoElement.preferredWidth = 390f;

        VerticalLayoutGroup infoLayout = info.AddComponent<VerticalLayoutGroup>();
        infoLayout.spacing = 8f;
        infoLayout.childAlignment = TextAnchor.MiddleLeft;
        infoLayout.childControlWidth = true;
        infoLayout.childControlHeight = true;
        infoLayout.childForceExpandHeight = false;

        TextMeshProUGUI titleText = CreateText(info.transform, title, 28, FontStyles.Bold);
        titleText.alignment = TextAlignmentOptions.Left;

        statText = CreateText(info.transform, "Stat", 24, FontStyles.Normal);
        statText.alignment = TextAlignmentOptions.Left;
        statText.color = new Color(0.82f, 0.86f, 0.92f, 1f);

        buyButton = CreateButton(row.transform, "BuyButton", "Buy", buttonColor, out costText);
        LayoutElement buttonElement = buyButton.gameObject.AddComponent<LayoutElement>();
        buttonElement.preferredWidth = 210f;
        buttonElement.preferredHeight = 78f;
    }

    private Button CreateButton(Transform parent, string name, string label, Color color, out TextMeshProUGUI labelText)
    {
        GameObject buttonObject = CreateColoredObject(parent, name, color);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();

        labelText = CreateText(buttonObject.transform, label, 27, FontStyles.Bold);
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

    private string FormatPercent(float value)
    {
        return $"{Mathf.RoundToInt(value * 100f)}%";
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
