using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;

    [Header("UI Elements")]
    public Slider healthSlider;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }


        //Slider healthSlider = GetComponent<Slider>();
        healthSlider.maxValue = PlayerStats.Instance.maxHealth;
    }

    private void Start()
    {
        UpdateHealthUI();
    }

    private void Update()
    {
        // ����� ������ Update � �������� UpdateHealthUI ������� ��� ��������� �����
        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = PlayerStats.Instance.currentHealth;
        }
    }
}
