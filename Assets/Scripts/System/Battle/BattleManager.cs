using UnityEngine;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private Transform enemyParent;
    [SerializeField] private GameObject resultPanelObject;
    [SerializeField] private GameObject bgImage;

    private LocationData locationData;
    private int currentWaveIndex = 0;
    private int totalGold = 0;
    private int totalExp = 0;

    private ResultPanel resultPanel;

    


    private void Start()
    {
        var stats = PlayerStats.Instance;
        stats.OnHealthChanged += OnPlayerHealthChanged;

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

    private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
    {
        if (currentHealth <= 0)
        {
            Defeat();
        }
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
        if (currentWaveIndex >= locationData.waveCount)
        {
            EndBattle();
            yield break;
        }

        yield return new WaitForSeconds(2f);

        GameObject enemyGO = Instantiate(
            locationData.enemyPrefabs[Random.Range(0, locationData.enemyPrefabs.Length)],
            enemyParent
        );

        Enemy enemy = enemyGO.GetComponent<Enemy>();
        enemy.Initialize();
        enemy.OnDefeated += OnEnemyDefeated;
    }

    private void OnEnemyDefeated(EnemyData data)
    {
        totalGold += data.GetRandomMoneyReward();
        totalExp += data.GetRandomExpReward();

        currentWaveIndex++;
        StartCoroutine(SpawnNextEnemyWithDelay());
    }

    private void EndBattle()
    {
        Debug.Log("Battle complete!");
        resultPanelObject.SetActive(true);
        resultPanel.ShowResults(totalGold, totalExp);
    }

    public void Defeat()
    {
        currentWaveIndex = locationData.waveCount;

        Debug.Log("Battle complete!");
        resultPanelObject.SetActive(true);
        totalExp = 0;
        totalGold = 0;
        resultPanel.ShowResults(totalGold, totalExp);
        PlayerStats.Instance.currentHealth = 10;
    }

}
