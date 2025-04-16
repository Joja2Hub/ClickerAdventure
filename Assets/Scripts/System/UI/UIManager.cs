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

    private void Awake()
    {
        
    }

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
        acctiveQuestsPanel.SetActive(false);
        questPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        levelPanel.SetActive(false);
        blocker.SetActive(false);

        // Включаем перемещение карты через синглтон MapController
        MapController.Instance.EnableMovement(true);
    }
}