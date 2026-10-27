using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class UiManager : MonoBehaviour
{
    [Header("Game HUD & Core References")]
    [SerializeField] private GameObject GameHUD;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text secondTimerText;            // Main text for the second timer
    [SerializeField] private TMP_Text secondTimerShadowText; // Shadow text for the second timer
    [SerializeField] private GameManager gameManager;

    [Header("Power-Up UI")]
    [Tooltip("Drag your Power-Up Selection Panel GameObject here.")]
    [SerializeField] private GameObject powerUpPanel;

    [Header("Lose / Game Over UI")]
    [SerializeField] private GameObject losePanel;
    [SerializeField] private TMP_Text bestRecordText;
    [SerializeField] private TMP_Text finalTimeText;
    [SerializeField] private Transform losePanelPowerUpContainer; // Parent container on the Lose Panel for power-up icons
    [SerializeField] private GameObject losePanelIconPrefab;       // UI Image prefab to display each power-up icon
    private List<GameObject> spawnedLosePanelIcons = new List<GameObject>();

    [Header("Player Stats HUD")]
    [SerializeField] private TMP_Text moneyText;      
    
    [Header("Health Hearts System")]
    [SerializeField] private Transform heartsContainer;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private int maxHearts = 10; 
    [SerializeField] private string heartAnimationClipName = "Heart"; 

    private List<GameObject> heartIcons = new List<GameObject>();
    private int displayedMoney = 0;
    private Coroutine moneyAnimationCoroutine;

    [Header("Pause & Settings Menu")]
    [SerializeField] public GameObject pauseMenu;
    [SerializeField] private GameObject settingsPanel;

    public static UiManager instance;
    [SerializeField] public GameObject player;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // <-- Keep this UI persistent across scenes!
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-detect timer text if not assigned
        if (timerText == null)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (timerText == null && texts[i].name.Contains("Timer"))
                    timerText = texts[i];
            }
        }
    }

    void OnEnable()
    {
        // Subscribe to scene load events to re-link scene-specific components automatically
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-find references that belong to the newly loaded map
        FindSceneReferences();
    }

    void Start()
    {
        InitializeUI();
        InitializeHearts();
        FindSceneReferences();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);

        // --- BULLETPROOF COIN SYNC FOR NEW MAPS ---
        StartCoroutine(SyncCoinsOnStart());

        // --- SYNC HEARTS TO FULL HEALTH ---
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            UpdateHealthPoints(playerHealth.maxHealth, playerHealth.maxHealth);
        }
        else
        {
            UpdateHealthPoints(1000f, 1000f);
        }
    }

    private void FindSceneReferences()
    {
        // Re-find the new player in the new scene
        player = GameObject.FindGameObjectWithTag("Player");

        // Re-find the power-up panel if it lives inside the scene instead of the persistent canvas
        if (powerUpPanel == null)
        {
            Transform foundPanel = transform.Find("PowerUpPanel") ?? transform.Find("PowerUpSelectionPanel");
            if (foundPanel != null)
            {
                powerUpPanel = foundPanel.gameObject;
            }
            else
            {
                GameObject panelObj = GameObject.FindWithTag("PowerUpPanel");
                if (panelObj != null) powerUpPanel = panelObj;
            }
        }

        if (powerUpPanel != null)
        {
            powerUpPanel.SetActive(false);
        }
    }

    private IEnumerator SyncCoinsOnStart()
    {
        yield return null;
        gameManager = GameManager.Instance;

        if (gameManager != null)
        {
            displayedMoney = gameManager.totalCoins;
            if (moneyText != null)
            {
                moneyText.text = $"{displayedMoney:N0}";
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (player != null)
        {
            StarterAssetsInputs inputs = player.GetComponent<StarterAssetsInputs>();
            if (inputs != null)
            {
                if (Time.timeScale == 0)
                {
                    inputs.cursorInputForLook = false;
                    inputs.look = new Vector2(0, 0);
                }
                else
                {
                    inputs.cursorInputForLook = true;
                }
            }
        }
    }

    private void InitializeUI()
    {
        if (GameHUD != null)
            GameHUD.SetActive(true);

        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void InitializeHearts()
    {
        if (heartsContainer == null) return;
        heartIcons.Clear();
        foreach (Transform child in heartsContainer)
        {
            heartIcons.Add(child.gameObject);
        }
    }

    // --- POWER-UP PANEL CONTROLS ---

    public void ShowPowerUpPanel()
    {
        Debug.Log("[UiManager] ShowPowerUpPanel() was called by GameManager!");

        if (powerUpPanel != null)
        {
            powerUpPanel.SetActive(true);
            Debug.Log("[UiManager] powerUpPanel.SetActive(true) executed successfully. Panel active state is now: " + powerUpPanel.activeInHierarchy);
        }
        else
        {
            Debug.LogError("[UiManager] CRITICAL: powerUpPanel reference is NULL! Drag it into the UiManager Inspector.");
        }
    }

    public void HidePowerUpPanel()
    {
        if (powerUpPanel != null)
        {
            powerUpPanel.SetActive(false);
        }
    }

    // --- LOSE / GAME OVER PANEL CONTROLS ---

    public void ShowLosePanel()
    {
        Debug.Log("[UiManager] Showing Lose Panel.");

        if (GameHUD != null)
        {
            GameHUD.SetActive(false); // Hide standard gameplay HUD when dead
        }

        if (losePanel != null)
        {
            losePanel.SetActive(true);

            // Populate run times and best record
            if (GameManager.Instance != null)
            {
                if (finalTimeText != null)
                {
                    finalTimeText.text = $"Time: {GameManager.Instance.GetFormattedCurrentTime()}";
                }

                if (bestRecordText != null)
                {
                    bestRecordText.text = $"Best Record: {GameManager.Instance.GetFormattedBestTime()}";
                }

                // --- POPULATE COLLECTED POWER-UP ICONS ---
                PopulateLosePanelPowerUps(GameManager.Instance.collectedPowerUpIcons);
            }
        }
        else
        {
            Debug.LogWarning("[UiManager] LosePanel reference is missing in UiManager inspector!");
        }
    }

    private void PopulateLosePanelPowerUps(List<Sprite> icons)
    {
        // Clear previously spawned icons if any
        foreach (GameObject iconObj in spawnedLosePanelIcons)
        {
            if (iconObj != null) Destroy(iconObj);
        }
        spawnedLosePanelIcons.Clear();

        if (losePanelPowerUpContainer == null || losePanelIconPrefab == null) return;

        foreach (Sprite iconSprite in icons)
        {
            if (iconSprite == null) continue;

            // Instantiate the icon prefab as a child of the container
            GameObject newIconObj = Instantiate(losePanelIconPrefab, losePanelPowerUpContainer);
            
            // Set the sprite on the Image component
            Image img = newIconObj.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = iconSprite;
            }

            spawnedLosePanelIcons.Add(newIconObj);
        }
    }

    public void UpdateHealthPoints(float currentHealth, float maxHealth)
    {
        for (int i = 0; i < heartIcons.Count; i++)
        {
            if (heartIcons[i] == null) continue;
            Animator heartAnimator = heartIcons[i].GetComponent<Animator>();
            heartIcons[i].SetActive(true);
            float heartStartPoints = i * 100f;
            float pointsInThisHeart = Mathf.Clamp(currentHealth - heartStartPoints, 0f, 100f);
            float normalizedTime = (100f - pointsInThisHeart) / 100f;

            if (heartAnimator != null)
            {
                heartAnimator.speed = 0f;
                AnimatorStateInfo stateInfo = heartAnimator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.length > 0)
                {
                    heartAnimator.Play(stateInfo.fullPathHash, 0, normalizedTime);
                }
                else
                {
                    heartAnimator.Play(heartAnimationClipName, 0, normalizedTime);
                }
            }
        }
    }

    public void UpdateMoney(int currentMoney)
    {
        if (moneyText == null) return;

        if (moneyAnimationCoroutine != null)
        {
            StopCoroutine(moneyAnimationCoroutine);
        }

        moneyAnimationCoroutine = StartCoroutine(AnimateCoinCounter(displayedMoney, currentMoney));
    }

    private IEnumerator AnimateCoinCounter(int startValue, int endValue)
    {
        float duration = 0.4f; 
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int stepValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
            moneyText.text = $"{stepValue:N0}";
            yield return null;
        }

        displayedMoney = endValue;
        moneyText.text = $"{displayedMoney:N0}";
    }

    public void UpdateTimeUI(float timeElapsed)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeElapsed / 60f);
        int seconds = Mathf.FloorToInt(timeElapsed % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void UpdateSecondTimeUI(float timeElapsed)
    {
        int minutes = Mathf.FloorToInt(timeElapsed / 60f);
        int seconds = Mathf.FloorToInt(timeElapsed % 60f);
        string timeString = $"Time: {minutes:00}:{seconds:00}";

        if (secondTimerText != null)
        {
            secondTimerText.text = timeString;
        }

        if (secondTimerShadowText != null)
        {
            secondTimerShadowText.text = timeString;
        }
    }

    public void PauseButton()
    {
        Time.timeScale = 0f;
        if (pauseMenu != null) pauseMenu.SetActive(true);
    }

    public void TogglePause()
    {
        bool isPaused = false;
        if (pauseMenu != null) 
        {
            isPaused = !pauseMenu.activeSelf;
            pauseMenu.SetActive(isPaused);
        }

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            bool isOpen = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isOpen);
        }
    }

  public void RestartButton()
    {
        Time.timeScale = 1f;

        // --- FIX: Hide all active UI panels and re-enable HUD ---
        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false); // Close settings if open
        }

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false); // Close pause menu if open
        }

        if (GameHUD != null)
        {
            GameHUD.SetActive(true);
        }

        // --- Reset local coin display ---
        displayedMoney = 0;
        if (moneyText != null)
        {
            moneyText.text = "0";
        }

        // Trigger a completely fresh new run via SceneController (resets rooms, random map, stats, timer, and coins)
        if (SceneController.Instance != null)
        {
            SceneController.Instance.StartNewRun();
        }
        else
        {
            // Fallback just in case SceneController is missing
            if (GameManager.Instance != null)
            {
                GameManager.Instance.levelElapsedTime = 0f;
                GameManager.Instance.collectedPowerUpIcons.Clear();
                GameManager.Instance.totalCoins = 0;
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

   public void HomeButton()
    {
        Time.timeScale = 1f;

        // Clean up persistent managers if returning to the title screen
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        if (SceneController.Instance != null)
        {
            Destroy(SceneController.Instance.gameObject);
        }

        // Load your correct title scene name
        SceneManager.LoadScene("Title");

        // Destroy the UiManager itself so it doesn't persist over the title menu
        Destroy(gameObject);
    }

    public void ResumeButton()
    {
        Time.timeScale = 1f;
        if (GameHUD != null) GameHUD.SetActive(true);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void HideHUDForCutscene()
    {
        if (GameHUD != null) GameHUD.SetActive(false);
    }
}