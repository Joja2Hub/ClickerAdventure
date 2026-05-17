using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private Transform enemyParent;
    [SerializeField] private GameObject resultPanelObject;
    [SerializeField] private GameObject bgImage;

    private LocationData locationData;
    private int currentWaveIndex = 0;
    private int totalGold = 0;
    private int totalExp = 0;
    private bool battleEnded;

    private PlayerStats playerStats;
    private ResultPanel resultPanel;

    private void Start()
    {
        playerStats = PlayerStats.Instance;
        if (playerStats != null)
            playerStats.OnHealthChanged += OnPlayerHealthChanged;

        if (MapController.Instance != null)
            MapController.Instance.gameObject.SetActive(false);

        locationData = DungeonTransferData.LocationData;
        if (locationData == null)
        {
            Debug.LogError("No location data found! Make sure it was set before loading this scene.");
            return;
        }

        resultPanel = resultPanelObject.GetComponent<ResultPanel>();
        SetupBattle();
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnHealthChanged -= OnPlayerHealthChanged;
    }

    private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
    {
        if (currentHealth <= 0)
            Defeat();
    }

    private void SetupBattle()
    {
        Debug.Log("Loaded dungeon: " + locationData.dungeonName);
        Debug.Log("Waves: " + locationData.waveCount);

        SpriteRenderer spriteRenderer = bgImage.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = locationData.background;

        StartCoroutine(SpawnNextEnemyWithDelay());
    }

    private IEnumerator SpawnNextEnemyWithDelay()
    {
        if (battleEnded)
            yield break;

        if (currentWaveIndex >= locationData.waveCount)
        {
            EndBattle();
            yield break;
        }

        yield return new WaitForSeconds(2f);

        if (battleEnded)
            yield break;

        var prefab = locationData.enemyPrefabs[Random.Range(0, locationData.enemyPrefabs.Length)];
        GameObject enemyGO = enemyParent != null ? Instantiate(prefab, enemyParent) : Instantiate(prefab);

        Enemy enemy = enemyGO.GetComponent<Enemy>();
        enemy.Initialize();
        enemy.OnDefeated += OnEnemyDefeated;
    }

    private void OnEnemyDefeated(EnemyData data)
    {
        if (battleEnded)
            return;

        totalGold += data.GetRandomMoneyReward();
        totalExp += data.GetRandomExpReward();

        currentWaveIndex++;
        StartCoroutine(SpawnNextEnemyWithDelay());
    }

    private void EndBattle()
    {
        if (battleEnded)
            return;

        battleEnded = true;
        Debug.Log("Battle complete!");
        resultPanelObject.SetActive(true);
        resultPanel.ShowResults(totalGold, totalExp);
    }

    public void Defeat()
    {
        if (battleEnded)
            return;

        battleEnded = true;
        currentWaveIndex = locationData.waveCount;
        totalExp = 0;
        totalGold = 0;

        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            Destroy(enemy.gameObject);

        Debug.Log("Battle failed.");
        resultPanelObject.SetActive(true);
        resultPanel.ShowResults(totalGold, totalExp);

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.SetHealth(10);
    }
}
