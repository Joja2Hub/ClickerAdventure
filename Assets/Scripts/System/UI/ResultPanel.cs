using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ResultPanel : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI expText;
    public Button returnButton;
    public GameObject panel;

    private void Awake()
    {
        panel.SetActive(false); // скрыть по умолчанию
        returnButton.onClick.AddListener(ReturnToMap);
    }

    public void ShowResults(int totalGold, int totalExp)
    {
        goldText.text = "Gold: " + totalGold;
        expText.text = "Exp: " + totalExp;
        panel.SetActive(true);
        PlayerStats.Instance.AddMoney(totalGold);
        PlayerStats.Instance.AddExperience(totalExp);
    }

    public void ReturnToMap()
    {
        SceneManager.LoadScene("Map");
    }
}
