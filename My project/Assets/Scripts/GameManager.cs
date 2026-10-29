using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PermanentUpgradeType
{
    MovementSpeed,
    AttackDamage,
    MaximumHealth,
    Defense
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private const int maxPermanentUpgradeLevel = 10;
    private const string permanentUpgradePointsKey = "PermanentUpgradePoints";
    private const string movementSpeedUpgradeKey = "PermanentUpgrade_MovementSpeed";
    private const string attackDamageUpgradeKey = "PermanentUpgrade_AttackDamage";
    private const string maximumHealthUpgradeKey = "PermanentUpgrade_MaximumHealth";
    private const string defenseUpgradeKey = "PermanentUpgrade_Defense";

    [Header("Scene Settings")]
    [Tooltip("Name of your shop scene where power-up selection and enemy spawns should be skipped.")]
    [SerializeField] private string shopSceneName = "ShopScene";

    [Header("Finish / Exit Zones")]
    [Tooltip("Leave empty to auto-detect by 'ExitZone' tag in each new scene.")]
    public FinishZone[] finishZones;

    [Header("Player Economy")]
    public int totalCoins = 0;

    [Header("Player Stats & Buffs")]
    public float playerMaxHealth = 1000f;
    public float playerCurrentHealth = 1000f;
    public float playerDamage = 15f;
    public float playerAttackSpeed = 1f;
    public float playerMovementSpeed = 5f;
    public float playerDefense = 0f;
    public float playerLuck = 0f;
    public float playerRegenAmount = 0f;

    [Header("Power-Up Icon History")]
    public List<Sprite> collectedPowerUpIcons = new List<Sprite>();

    [Header("Game Stopwatch Timer & Records")]
    public float levelElapsedTime = 0f;
    private const string BestTimeKey = "BestRunTime";
    public float BestRunTime { get; private set; }

    // Spawner Tracking (Dynamic)
    private int expectedSpawnersCount = 0;
    private int registeredSpawnersCount = 0;

    private int totalEnemies = 0;
    private int defeatedEnemies = 0;
    private bool spawnerFinished = false;
    private bool powerUpTriggered = false;
    private bool isInitialized = false;
    private string currentSceneName = "";
    private float startingPlayerMaxHealth;
    private float startingPlayerDamage;
    private float startingPlayerAttackSpeed;
    private float startingPlayerMovementSpeed;
    private float startingPlayerDefense;
    private float startingPlayerLuck;
    private float startingPlayerRegenAmount;
    private int permanentUpgradePoints;
    private int movementSpeedUpgradeLevel;
    private int attackDamageUpgradeLevel;
    private int maximumHealthUpgradeLevel;
    private int defenseUpgradeLevel;
    private ThirdPersonController movementController;
    private float appliedMovementSpeedMultiplier = float.NaN;

    public int PermanentUpgradePoints => permanentUpgradePoints;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            startingPlayerMaxHealth = playerMaxHealth;
            startingPlayerDamage = playerDamage;
            startingPlayerAttackSpeed = playerAttackSpeed;
            startingPlayerMovementSpeed = playerMovementSpeed;
            startingPlayerDefense = playerDefense;
            startingPlayerLuck = playerLuck;
            startingPlayerRegenAmount = playerRegenAmount;

            LoadPermanentUpgradeProgress();
            ApplyPermanentUpgrades();
            playerCurrentHealth = playerMaxHealth;

            // Load saved best record time on startup
            BestRunTime = PlayerPrefs.GetFloat(BestTimeKey, 0f);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeScene();
    }

    private void Start()
    {
        InitializeScene();
    }

    private void InitializeScene()
    {
        if (isInitialized && SceneManager.GetActiveScene().name == currentSceneName) return;
        
        currentSceneName = SceneManager.GetActiveScene().name;
        isInitialized = true;

        FindFinishZones();
        
        EnemySpawner[] allSpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        expectedSpawnersCount = allSpawners != null ? allSpawners.Length : 0;

        spawnerFinished = false;
        powerUpTriggered = false;
        defeatedEnemies = 0;
        totalEnemies = 0;
        registeredSpawnersCount = 0;

        Debug.Log($"[GameManager] Initialized scene. Found {expectedSpawnersCount} enemy spawner(s).");

        if (IsShopScene(currentSceneName))
        {
            spawnerFinished = true;
            return; 
        }

        if (expectedSpawnersCount == 0)
        {
            spawnerFinished = true;
            AllEnemiesDefeated();
        }
    }

    private void Update()
    {
        if (Time.timeScale > 0f)
        {
            levelElapsedTime += Time.deltaTime;

            if (UiManager.instance != null)
            {
                UiManager.instance.UpdateTimeUI(levelElapsedTime);
                UiManager.instance.UpdateSecondTimeUI(levelElapsedTime);
            }
        }

        ApplyMovementSpeedToPlayer();
    }

    private void ApplyMovementSpeedToPlayer()
    {
        if (movementController == null)
        {
            movementController = FindFirstObjectByType<ThirdPersonController>();
            if (movementController == null) return;
            appliedMovementSpeedMultiplier = float.NaN;
        }

        float multiplier = GetMovementSpeedMultiplier();
        if (Mathf.Approximately(appliedMovementSpeedMultiplier, multiplier)) return;

        movementController.SetMovementSpeedMultiplier(multiplier);
        appliedMovementSpeedMultiplier = multiplier;
    }

    // ==========================================
    // RECORD & TIMER SAVE SYSTEM
    // ==========================================
    public void EndRunAndCheckRecord()
    {
        float savedBestRunTime = PlayerPrefs.GetFloat(BestTimeKey, 0f);
        BestRunTime = Mathf.Max(BestRunTime, savedBestRunTime);

        // The record is the longest survival time across all runs.
        if (levelElapsedTime > BestRunTime)
        {
            BestRunTime = levelElapsedTime;
            Debug.Log($"[GameManager] New Record Time Saved: {GetFormattedBestTime()}");
        }

        PlayerPrefs.SetFloat(BestTimeKey, BestRunTime);
        PlayerPrefs.Save();
    }

    public string GetFormattedCurrentTime()
    {
        return FormatTime(levelElapsedTime);
    }

    public string GetFormattedBestTime()
    {
        return FormatTime(BestRunTime);
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void FindFinishZones()
    {
        GameObject[] exitObjects = GameObject.FindGameObjectsWithTag("ExitZone");
        List<FinishZone> foundZones = new List<FinishZone>();

        foreach (GameObject obj in exitObjects)
        {
            FinishZone zone = obj.GetComponent<FinishZone>();
            if (zone != null) foundZones.Add(zone);
        }

        finishZones = foundZones.ToArray();

        if (finishZones.Length == 0 && !IsShopScene(SceneManager.GetActiveScene().name))
        {
            Debug.LogWarning("[GameManager] No standard FinishZones found in this scene.");
        }
    }

    public void AddCollectedPowerUpIcon(Sprite icon)
    {
        if (icon == null) return;
        collectedPowerUpIcons.Add(icon);

        PowerUpDisplayContainer displayContainer = FindFirstObjectByType<PowerUpDisplayContainer>(FindObjectsInactive.Include);
        if (displayContainer != null)
        {
            displayContainer.RefreshDisplay();
        }
    }

    public void RegisterWaveEnemies(int enemyCount)
    {
        totalEnemies += enemyCount;
        registeredSpawnersCount++;

        if (registeredSpawnersCount >= expectedSpawnersCount)
        {
            spawnerFinished = true;
        }

        // REMOVED UiManager.instance.UpdateEnemiesLeft(...) call here

        if (totalEnemies <= 0 && spawnerFinished) 
        {
            AllEnemiesDefeated();
        }
    }

    public void EnemyDefeated()
    {
        spawnerFinished = true;

        defeatedEnemies++;
        int remainingEnemies = Mathf.Max(0, totalEnemies - defeatedEnemies);

        // REMOVED UiManager.instance.UpdateEnemiesLeft(...) call here

        if (defeatedEnemies >= totalEnemies) 
        {
            AllEnemiesDefeated();
        }
    }

    void AllEnemiesDefeated()
    {
        if (powerUpTriggered) return;
        powerUpTriggered = true;

        if (IsShopScene(SceneManager.GetActiveScene().name))
        {
            return;
        }

        if (UiManager.instance != null)
        {
            UiManager.instance.ShowPowerUpPanel();
        }

        if (finishZones != null)
        {
            foreach (FinishZone zone in finishZones)
            {
                if (zone != null) zone.UnlockZone();
            }
        }
    }

    public void AddCoins(int amount)
    {
        totalCoins += amount;
        if (UiManager.instance != null)
        {
            UiManager.instance.UpdateMoney(totalCoins);
        }
    }

    public void AwardPermanentUpgradePointForCompletedMap()
    {
        permanentUpgradePoints++;
        PlayerPrefs.SetInt(permanentUpgradePointsKey, permanentUpgradePoints);
        PlayerPrefs.Save();
        Debug.Log($"[GameManager] Map completed. Permanent upgrade points: {permanentUpgradePoints}");
    }

    public int GetPermanentUpgradeLevel(PermanentUpgradeType upgrade)
    {
        switch (upgrade)
        {
            case PermanentUpgradeType.MovementSpeed:
                return movementSpeedUpgradeLevel;
            case PermanentUpgradeType.AttackDamage:
                return attackDamageUpgradeLevel;
            case PermanentUpgradeType.MaximumHealth:
                return maximumHealthUpgradeLevel;
            case PermanentUpgradeType.Defense:
                return defenseUpgradeLevel;
            default:
                Debug.LogError($"[GameManager] Unknown permanent upgrade: {upgrade}");
                return 0;
        }
    }

    public bool CanPurchasePermanentUpgrade(PermanentUpgradeType upgrade)
    {
        if (!System.Enum.IsDefined(typeof(PermanentUpgradeType), upgrade))
        {
            Debug.LogError($"[GameManager] Unknown permanent upgrade: {upgrade}");
            return false;
        }

        return GetPermanentUpgradeLevel(upgrade) < maxPermanentUpgradeLevel &&
               permanentUpgradePoints >= GetPermanentUpgradeCost(upgrade);
    }

    public int GetPermanentUpgradeCost(PermanentUpgradeType upgrade)
    {
        if (!System.Enum.IsDefined(typeof(PermanentUpgradeType), upgrade))
        {
            Debug.LogError($"[GameManager] Unknown permanent upgrade: {upgrade}");
            return 0;
        }

        return GetPermanentUpgradeLevel(upgrade) + 1;
    }

    public bool TryPurchasePermanentUpgrade(PermanentUpgradeType upgrade)
    {
        if (!CanPurchasePermanentUpgrade(upgrade)) return false;

        permanentUpgradePoints -= GetPermanentUpgradeCost(upgrade);

        switch (upgrade)
        {
            case PermanentUpgradeType.MovementSpeed:
                movementSpeedUpgradeLevel++;
                PlayerPrefs.SetInt(movementSpeedUpgradeKey, movementSpeedUpgradeLevel);
                break;
            case PermanentUpgradeType.AttackDamage:
                attackDamageUpgradeLevel++;
                PlayerPrefs.SetInt(attackDamageUpgradeKey, attackDamageUpgradeLevel);
                break;
            case PermanentUpgradeType.MaximumHealth:
                maximumHealthUpgradeLevel++;
                PlayerPrefs.SetInt(maximumHealthUpgradeKey, maximumHealthUpgradeLevel);
                break;
            case PermanentUpgradeType.Defense:
                defenseUpgradeLevel++;
                PlayerPrefs.SetInt(defenseUpgradeKey, defenseUpgradeLevel);
                break;
        }

        PlayerPrefs.SetInt(permanentUpgradePointsKey, permanentUpgradePoints);
        PlayerPrefs.Save();
        return true;
    }

    public void ResetPlayerStatsForNewRun()
    {
        permanentUpgradePoints = 0;
        movementSpeedUpgradeLevel = 0;
        attackDamageUpgradeLevel = 0;
        maximumHealthUpgradeLevel = 0;
        defenseUpgradeLevel = 0;

        PlayerPrefs.SetInt(permanentUpgradePointsKey, permanentUpgradePoints);
        PlayerPrefs.SetInt(movementSpeedUpgradeKey, movementSpeedUpgradeLevel);
        PlayerPrefs.SetInt(attackDamageUpgradeKey, attackDamageUpgradeLevel);
        PlayerPrefs.SetInt(maximumHealthUpgradeKey, maximumHealthUpgradeLevel);
        PlayerPrefs.SetInt(defenseUpgradeKey, defenseUpgradeLevel);
        PlayerPrefs.Save();

        ApplyPermanentUpgrades();
        playerCurrentHealth = playerMaxHealth;
        playerAttackSpeed = startingPlayerAttackSpeed;
        playerLuck = startingPlayerLuck;
        playerRegenAmount = startingPlayerRegenAmount;
        totalCoins = 0;
        levelElapsedTime = 0f;
        collectedPowerUpIcons.Clear();
        expectedSpawnersCount = 0;
        registeredSpawnersCount = 0;
        totalEnemies = 0;
        defeatedEnemies = 0;
        spawnerFinished = false;
        powerUpTriggered = false;
        currentSceneName = "";
        isInitialized = false;
        ApplyMovementSpeedToPlayer();
    }

    private void LoadPermanentUpgradeProgress()
    {
        permanentUpgradePoints = Mathf.Max(0, PlayerPrefs.GetInt(permanentUpgradePointsKey, 0));
        movementSpeedUpgradeLevel = Mathf.Clamp(PlayerPrefs.GetInt(movementSpeedUpgradeKey, 0), 0, maxPermanentUpgradeLevel);
        attackDamageUpgradeLevel = Mathf.Clamp(PlayerPrefs.GetInt(attackDamageUpgradeKey, 0), 0, maxPermanentUpgradeLevel);
        maximumHealthUpgradeLevel = Mathf.Clamp(PlayerPrefs.GetInt(maximumHealthUpgradeKey, 0), 0, maxPermanentUpgradeLevel);
        defenseUpgradeLevel = Mathf.Clamp(PlayerPrefs.GetInt(defenseUpgradeKey, 0), 0, maxPermanentUpgradeLevel);
    }

    private void ApplyPermanentUpgrades()
    {
        playerMovementSpeed = startingPlayerMovementSpeed * (1f + movementSpeedUpgradeLevel * 0.05f);
        playerDamage = startingPlayerDamage * (1f + attackDamageUpgradeLevel * 0.1f);
        playerMaxHealth = startingPlayerMaxHealth * (1f + maximumHealthUpgradeLevel * 0.1f);
        playerDefense = startingPlayerDefense + defenseUpgradeLevel;
    }

    public float GetMovementSpeedMultiplier()
    {
        return startingPlayerMovementSpeed > 0f
            ? playerMovementSpeed / startingPlayerMovementSpeed
            : 1f;
    }

// Add this public property to GameManager to check status easily
public bool AreAllEnemiesDefeated => (totalEnemies > 0 && defeatedEnemies >= totalEnemies) || (expectedSpawnersCount == 0);

private bool IsShopScene(string sceneName)
{
    return sceneName == shopSceneName || sceneName == "Shop";
}
}