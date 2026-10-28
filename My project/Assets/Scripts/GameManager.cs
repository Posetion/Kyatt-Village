using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

        if (currentSceneName == shopSceneName)
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
    }

    // ==========================================
    // RECORD & TIMER SAVE SYSTEM
    // ==========================================
    public void EndRunAndCheckRecord()
    {
        // Check if current run time is higher than the saved best time (longest survival)
        if (levelElapsedTime > BestRunTime)
        {
            BestRunTime = levelElapsedTime;
            PlayerPrefs.SetFloat(BestTimeKey, BestRunTime);
            PlayerPrefs.Save();
            Debug.Log($"[GameManager] New Record Time Saved: {GetFormattedBestTime()}");
        }
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

        if (finishZones.Length == 0 && SceneManager.GetActiveScene().name != shopSceneName)
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

        if (SceneManager.GetActiveScene().name == shopSceneName)
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

// Add this public property to GameManager to check status easily
public bool AreAllEnemiesDefeated => (totalEnemies > 0 && defeatedEnemies >= totalEnemies) || (expectedSpawnersCount == 0);

}