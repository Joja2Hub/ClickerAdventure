using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI expText;
    public Button returnButton;
    public GameObject panel;

    private bool rewardsApplied;

    private void Awake()
    {
        panel.SetActive(false);
        returnButton.onClick.AddListener(ReturnToMap);
    }

    private void OnDestroy()
    {
        returnButton.onClick.RemoveListener(ReturnToMap);
    }

    public void ShowResults(int totalGold, int totalExp)
    {
        goldText.text = "Gold: " + totalGold;
        expText.text = "Exp: " + totalExp;
        panel.SetActive(true);

        if (rewardsApplied)
            return;

        rewardsApplied = true;
        PlayerStats.Instance.AddMoney(totalGold);
        PlayerStats.Instance.AddExperience(totalExp);
    }

    public void ReturnToMap()
    {
        SceneManager.LoadScene("Map");
    }
}
