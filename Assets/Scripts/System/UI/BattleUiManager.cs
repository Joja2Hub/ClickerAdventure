using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;

    [Header("UI Elements")]
    public Slider healthSlider;

    private PlayerStats playerStats;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerStats = PlayerStats.Instance;
        if (playerStats == null)
            return;

        healthSlider.maxValue = playerStats.maxHealth;
        playerStats.OnHealthChanged += UpdateHealthUI;
        UpdateHealthUI(playerStats.currentHealth, playerStats.maxHealth);
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnHealthChanged -= UpdateHealthUI;

        if (Instance == this)
            Instance = null;
    }

    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthSlider == null)
            return;

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }
}
