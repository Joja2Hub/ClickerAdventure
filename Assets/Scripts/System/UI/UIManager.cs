using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject questsPanel;
    public GameObject inventoryPanel;
    public GameObject townPanel;
    public GameObject levelPanel;
    public GameObject blocker;

    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI levelText;

    private void Start()
    {
        MapController.Instance.gameObject.SetActive(true);
        HideAllPanels();
        blocker.SetActive(false);

        // Подписка на события
        var stats = PlayerStats.Instance;
        stats.OnMoneyChanged += UpdateMoney;
        stats.OnLevelChanged += UpdateLevel;


        // Инициализация начального значения
        UpdateMoney(stats.money);
        UpdateLevel(stats.level);

    }

    private void UpdateMoney(int newMoney)
    {
        moneyText.text = $" {newMoney}";
    }

    private void UpdateLevel(int newLevel)
    {
        levelText.text = $"{newLevel}";
    }


    public void ShowSettingsPanel()
    {
        ShowPanel(settingsPanel);
    }

    public void ShowQuestsPanel()
    {
        ShowPanel(questsPanel);
    }

    public void ShowInventoryPanel()
    {
        ShowPanel(inventoryPanel);
    }

    public void ShowTownPanel()
    {
        ShowPanel(townPanel);
    }

    public void ShowLevelPanel()
    {
        ShowPanel(levelPanel);
    }

    private void ShowPanel(GameObject panel)
    {
        HideAllPanels();
        panel.SetActive(true);
        blocker.SetActive(true);

        // Отключаем перемещение карты через синглтон MapController
        MapController.Instance.EnableMovement(false);
    }

    public void HideAllPanels()
    {
        settingsPanel.SetActive(false);
        questsPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        townPanel.SetActive(false);
        levelPanel.SetActive(false);
        blocker.SetActive(false);

        // Включаем перемещение карты через синглтон MapController
        MapController.Instance.EnableMovement(true);
    }
}