using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

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
    private BattleRuntimeHUD runtimeHUD;
    private Enemy currentEnemy;
    private int comboCount;
    private int rageCharge;
    private float lastAttackTime;
    private int maxComboCount;
    private int criticalHits;
    private int defeatedEnemies;
    private int damageTaken;
    private int powerStrikes;
    private int rageBursts;

    public int CurrentWave => Mathf.Min(currentWaveIndex + 1, locationData != null ? locationData.waveCount : 1);
    public int TotalWaves => locationData != null ? locationData.waveCount : 1;
    public int TotalGold => totalGold;
    public int TotalExp => totalExp;
    public int ComboCount => comboCount;
    public int MaxComboCount => maxComboCount;
    public int CriticalHits => criticalHits;
    public int DamageTaken => damageTaken;
    public int RageCharge => rageCharge;
    public int PlayerHealth => playerStats != null ? playerStats.currentHealth : 0;
    public int PlayerMaxHealth => playerStats != null ? playerStats.maxHealth : 1;
    public bool HasActiveEnemy => currentEnemy != null && !battleEnded;
    public bool CanCastPowerStrike => HasActiveEnemy && playerStats != null;
    public bool CanCastHeal => !battleEnded && playerStats != null && playerStats.currentHealth < playerStats.maxHealth;
    public bool CanCastRageBurst => HasActiveEnemy && playerStats != null && rageCharge >= 100;

    private void Awake()
    {
        Instance = this;
    }

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
        runtimeHUD = gameObject.AddComponent<BattleRuntimeHUD>();
        runtimeHUD.Initialize(this);
        SetupBattle();
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnHealthChanged -= OnPlayerHealthChanged;

        if (Instance == this)
            Instance = null;
    }

    private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
    {
        runtimeHUD?.RefreshStats();

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

        runtimeHUD?.SetWaitingForEnemy();

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
        currentEnemy = enemy;
        enemy.Initialize();
        enemy.OnDefeated += OnEnemyDefeated;
        runtimeHUD?.UpdateWave(CurrentWave, TotalWaves);
        runtimeHUD?.RefreshStats();
    }

    private void OnEnemyDefeated(EnemyData data)
    {
        if (battleEnded)
            return;

        int goldReward = data.GetRandomMoneyReward();
        if (playerStats != null)
            goldReward = Mathf.RoundToInt(goldReward * playerStats.BattleGoldMultiplier);

        totalGold += goldReward;
        totalExp += data.GetRandomExpReward();
        currentEnemy = null;
        defeatedEnemies++;
        AddRage(10);

        currentWaveIndex++;
        runtimeHUD?.UpdateWave(CurrentWave, TotalWaves);
        runtimeHUD?.RefreshStats();
        StartCoroutine(SpawnNextEnemyWithDelay());
    }

    private void EndBattle()
    {
        if (battleEnded)
            return;

        battleEnded = true;
        Debug.Log("Battle complete!");
        BattlePerformanceResult performance = BuildPerformanceResult();
        totalGold += performance.BonusGold;
        totalExp += performance.BonusExperience;
        resultPanelObject.SetActive(true);
        resultPanel.ShowResults(totalGold, totalExp, performance);
    }

    public void HandlePlayerTap(Enemy enemy)
    {
        if (battleEnded || enemy == null || enemy != currentEnemy || playerStats == null)
            return;

        if (Time.time - lastAttackTime > 1.25f)
            comboCount = 0;

        comboCount++;
        maxComboCount = Mathf.Max(maxComboCount, comboCount);
        lastAttackTime = Time.time;

        bool isCritical = Random.value < GetCriticalChance();
        int comboBonus = Mathf.FloorToInt(comboCount / 5f);
        int damage = Mathf.Max(1, playerStats.currentDmg + comboBonus);

        if (isCritical)
        {
            damage = Mathf.RoundToInt(damage * 2.1f);
            criticalHits++;
        }

        enemy.TakeDamage(damage, isCritical, isCritical ? "CRIT" : null);
        AddRage(isCritical ? 14 : 7);
        runtimeHUD?.RefreshStats();
    }

    public bool CastPowerStrike()
    {
        if (!CanCastPowerStrike)
            return false;

        int damage = Mathf.Max(1, playerStats.currentDmg * 5 + comboCount);
        currentEnemy.TakeDamage(damage, true, "POWER");
        powerStrikes++;
        AddRage(12);
        RewardPopup.ShowMessage("Power strike", $"-{damage}");
        runtimeHUD?.RefreshStats();
        return true;
    }

    public bool CastHeal()
    {
        if (!CanCastHeal)
            return false;

        int healAmount = Mathf.Max(20, Mathf.RoundToInt(playerStats.maxHealth * 0.35f));
        playerStats.SetHealth(playerStats.currentHealth + healAmount);
        RewardPopup.ShowMessage("Heal", $"+{healAmount} HP");
        runtimeHUD?.RefreshStats();
        return true;
    }

    public bool CastRageBurst()
    {
        if (!CanCastRageBurst)
            return false;

        int damage = Mathf.Max(10, playerStats.currentDmg * 12 + comboCount * 2);
        rageCharge = 0;
        currentEnemy.TakeDamage(damage, true, "RAGE");
        rageBursts++;
        RewardPopup.ShowMessage("Rage burst", $"-{damage}");
        runtimeHUD?.RefreshStats();
        return true;
    }

    public int ResolveIncomingDamage(int baseDamage)
    {
        if (comboCount >= 12)
            comboCount = Mathf.Max(0, comboCount - 4);
        else
            comboCount = 0;

        int resolvedDamage = Mathf.Max(1, baseDamage);
        damageTaken += resolvedDamage;
        runtimeHUD?.RefreshStats();
        return resolvedDamage;
    }

    public void Defeat()
    {
        if (battleEnded)
            return;

        battleEnded = true;
        currentEnemy = null;
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

    private void AddRage(int amount)
    {
        if (playerStats != null)
            amount = Mathf.Max(1, Mathf.RoundToInt(amount * playerStats.RageGainMultiplier));

        rageCharge = Mathf.Clamp(rageCharge + amount, 0, 100);
    }

    private float GetCriticalChance()
    {
        float comboBonus = Mathf.Min(0.18f, comboCount * 0.006f);
        float upgradeBonus = playerStats != null ? playerStats.CriticalChanceBonus : 0f;
        return Mathf.Clamp(0.08f + comboBonus + upgradeBonus, 0.02f, 0.5f);
    }

    private BattlePerformanceResult BuildPerformanceResult()
    {
        int score = defeatedEnemies * 6;

        if (maxComboCount >= 25)
            score += 40;
        else if (maxComboCount >= 15)
            score += 28;
        else if (maxComboCount >= 8)
            score += 16;

        score += Mathf.Min(20, criticalHits * 4);
        score += Mathf.Min(16, rageBursts * 8);
        score += Mathf.Min(10, powerStrikes * 3);

        int maxHealth = playerStats != null ? Mathf.Max(1, playerStats.maxHealth) : 100;
        if (damageTaken == 0)
            score += 30;
        else if (damageTaken <= maxHealth / 4)
            score += 18;
        else if (damageTaken <= maxHealth / 2)
            score += 8;

        string rank = GetPerformanceRank(score);
        float multiplier = GetPerformanceMultiplier(rank);
        int bonusGold = Mathf.RoundToInt(totalGold * multiplier);
        int bonusExperience = Mathf.RoundToInt(totalExp * multiplier);

        if (maxComboCount >= 10)
        {
            bonusGold += Mathf.Min(60, maxComboCount * 2);
            bonusExperience += Mathf.Min(45, maxComboCount);
        }

        return new BattlePerformanceResult(rank, score, bonusGold, bonusExperience, maxComboCount, criticalHits, rageBursts, damageTaken);
    }

    private string GetPerformanceRank(int score)
    {
        if (score >= 95)
            return "S";

        if (score >= 72)
            return "A";

        if (score >= 48)
            return "B";

        return "C";
    }

    private float GetPerformanceMultiplier(string rank)
    {
        switch (rank)
        {
            case "S":
                return 0.28f;
            case "A":
                return 0.18f;
            case "B":
                return 0.1f;
            default:
                return 0.04f;
        }
    }
}

public readonly struct BattlePerformanceResult
{
    public BattlePerformanceResult(string rank, int score, int bonusGold, int bonusExperience, int maxCombo, int criticalHits, int rageBursts, int damageTaken)
    {
        Rank = rank;
        Score = score;
        BonusGold = bonusGold;
        BonusExperience = bonusExperience;
        MaxCombo = maxCombo;
        CriticalHits = criticalHits;
        RageBursts = rageBursts;
        DamageTaken = damageTaken;
    }

    public string Rank { get; }
    public int Score { get; }
    public int BonusGold { get; }
    public int BonusExperience { get; }
    public int MaxCombo { get; }
    public int CriticalHits { get; }
    public int RageBursts { get; }
    public int DamageTaken { get; }
    public bool HasBonus => BonusGold > 0 || BonusExperience > 0;
}
