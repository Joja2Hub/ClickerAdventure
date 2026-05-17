using TMPro;
using UnityEngine;

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

    private void Start()
    {
        if (MapController.Instance != null)
            MapController.Instance.gameObject.SetActive(true);

        HideAllPanels();
        blocker.SetActive(false);
        SubscribeToStats();
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
