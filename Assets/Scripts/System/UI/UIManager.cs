using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject acctiveQuestsPanel;
    public GameObject inventoryPanel;
    public GameObject levelPanel;
    public GameObject questPanel;
    public GameObject blocker;

    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI levelText;

    private PlayerStats subscribedStats;
    private UpgradeShopRuntimeOverlay shopOverlay;
    private AchievementRuntimeOverlay achievementOverlay;

    private void Start()
    {
        if (MapController.Instance != null)
            MapController.Instance.gameObject.SetActive(true);

        HideAllPanels();
        blocker.SetActive(false);
        SubscribeToStats();
        SetupRuntimeShop();
        SetupAchievementOverlay();
        ApplyUnifiedSceneStyle();
    }

    private void OnDestroy()
    {
        UnsubscribeFromStats();
    }

    public void ShowSettingsPanel()
    {
        ShowPanel(settingsPanel);
    }

    public void ShowActiveQuestsPanel()
    {
        ShowPanel(acctiveQuestsPanel);
    }

    public void ShowInventoryPanel()
    {
        ShowPanel(inventoryPanel);
    }

    public void ShowLevelPanel()
    {
        ShowPanel(levelPanel);
    }

    public void ShowQuestPanel()
    {
        ShowPanel(questPanel);
    }

    public void HideAllPanels()
    {
        settingsPanel.SetActive(false);
        acctiveQuestsPanel.SetActive(false);
        questPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        levelPanel.SetActive(false);
        blocker.SetActive(false);

        if (MapController.Instance != null)
            MapController.Instance.EnableMovement(true);
    }

    private void ShowPanel(GameObject panel)
    {
        HideAllPanels();
        panel.SetActive(true);
        blocker.SetActive(true);

        if (MapController.Instance != null)
            MapController.Instance.EnableMovement(false);
    }

    private void SubscribeToStats()
    {
        subscribedStats = PlayerStats.Instance;
        if (subscribedStats == null)
            return;

        subscribedStats.OnMoneyChanged += UpdateMoney;
        subscribedStats.OnLevelChanged += UpdateLevel;

        UpdateMoney(subscribedStats.money);
        UpdateLevel(subscribedStats.level);
    }

    private void SetupRuntimeShop()
    {
        shopOverlay = gameObject.AddComponent<UpgradeShopRuntimeOverlay>();
        shopOverlay.Initialize();
    }

    private void SetupAchievementOverlay()
    {
        achievementOverlay = gameObject.AddComponent<AchievementRuntimeOverlay>();
        achievementOverlay.Initialize();
    }

    private void ApplyUnifiedSceneStyle()
    {
        Canvas canvas = RuntimeUiHost.GetCanvas(transform);
        Transform buttonRoot = RuntimeUiHost.GetButtonRoot(canvas);

        foreach (Button button in buttonRoot.GetComponentsInChildren<Button>(true))
        {
            RuntimeUiStyle.ApplyButton(button, GetButtonColor(button), RuntimeUiStyle.MainButtonWidth, RuntimeUiStyle.MainButtonHeight);

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
                RuntimeUiStyle.ApplyText(label, RuntimeUiStyle.ButtonTextSize, FontStyles.Bold, RuntimeUiStyle.Text, TextAlignmentOptions.Center);
        }

        ApplyPanelBase(settingsPanel);
        ApplyPanelBase(acctiveQuestsPanel);
        ApplyPanelBase(inventoryPanel);
        ApplyPanelBase(levelPanel);
        ApplyPanelBase(questPanel);
    }

    private Color GetButtonColor(Button button)
    {
        Image image = button.GetComponent<Image>();
        if (image == null)
            return RuntimeUiStyle.NeutralButton;

        if (image.sprite != null)
            return image.color;

        return RuntimeUiStyle.NeutralButton;
    }

    private void ApplyPanelBase(GameObject panel)
    {
        if (panel == null)
            return;

        Image image = panel.GetComponent<Image>();
        if (image != null && image.sprite == null)
            image.color = RuntimeUiStyle.Panel;

        foreach (TextMeshProUGUI label in panel.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (label.fontSize < RuntimeUiStyle.CaptionSize)
                RuntimeUiStyle.ApplyText(label, RuntimeUiStyle.BodySize, label.fontStyle, RuntimeUiStyle.Text, label.alignment);
        }
    }

    private void UnsubscribeFromStats()
    {
        if (subscribedStats == null)
            return;

        subscribedStats.OnMoneyChanged -= UpdateMoney;
        subscribedStats.OnLevelChanged -= UpdateLevel;
        subscribedStats = null;
    }

    private void UpdateMoney(int newMoney)
    {
        moneyText.text = $" {newMoney}";
    }

    private void UpdateLevel(int newLevel)
    {
        levelText.text = $"{newLevel}";
    }
}
