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
        ShowResults(totalGold, totalExp, default);
    }

    public void ShowResults(int totalGold, int totalExp, BattlePerformanceResult performance)
    {
        goldText.text = "Gold: " + totalGold;
        expText.text = "Exp: " + totalExp;
        panel.SetActive(true);

        if (rewardsApplied)
            return;

        rewardsApplied = true;
        PlayerStats.Instance.AddMoney(totalGold);
        PlayerStats.Instance.AddExperience(totalExp);

        if (totalGold <= 0 && totalExp <= 0)
            return;

        if (performance.HasBonus)
        {
            RewardPopup.ShowMessage(
                $"Battle reward  Rank {performance.Rank}",
                $"+{totalGold} gold\n+{totalExp} XP\nBonus: +{performance.BonusGold} gold / +{performance.BonusExperience} XP\nBest combo x{performance.MaxCombo}  Crits {performance.CriticalHits}");
            return;
        }

        RewardPopup.ShowReward("Battle reward", totalGold, totalExp);
    }

    public void ReturnToMap()
    {
        SceneManager.LoadScene("Map");
    }
}
