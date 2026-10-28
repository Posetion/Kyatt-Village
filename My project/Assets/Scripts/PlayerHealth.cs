using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private GameObject player; 

    [Header("Game Manager Reference")]
    [SerializeField] private GameManager gameManager; 

    [Header("UI Integration")]
    [SerializeField] private UiManager uiManager; 

    [Header("Exit Objective & Arrow")]
    [Tooltip("Drag the 2D arrow sprite GameObject here.")]
    [SerializeField] private GameObject playerArrow;
    [Tooltip("Height offset above the player's head.")]
    [SerializeField] private Vector3 arrowOffset = new Vector3(0f, 1.5f, 0f);
    private FinishZone selectedExitZone;

    [Header("Regeneration")]
    public float regenAmountPerSecond 
    {
        get { return (gameManager != null) ? gameManager.playerRegenAmount : 0f; }
        set { if (gameManager != null) gameManager.playerRegenAmount = value; }
    }

    public float maxHealth 
    {
        get { return (gameManager != null) ? gameManager.playerMaxHealth : 1000f; }
        set { if (gameManager != null) gameManager.playerMaxHealth = value; }
    }

    public float currentHealth 
    {
        get { return (gameManager != null) ? gameManager.playerCurrentHealth : 1000f; }
        set 
        { 
            if (gameManager != null) 
            {
                if (value > gameManager.playerCurrentHealth && value == maxHealth)
                {
                    Debug.LogWarning($"[Health Debug] Health suddenly refilled to max! Stack trace:\n{System.Environment.StackTrace}");
                }

                gameManager.playerCurrentHealth = value; 
            }
        }
    }

    public float attackDamage 
    {
        get 
        {
            if (gameManager != null) return gameManager.playerDamage;
            return baseAttackDamage;
        }
        set { baseAttackDamage = value; }
    }
    [SerializeField] private float baseAttackDamage = 15f;

    public float invincibilityDuration = 0.5f; 

    [Header("Sounds & Specific Volumes")]
    public AudioClip attackedSound;
    [Range(0f, 1f)] public float attackedVolume = 1.0f;
    
    public AudioClip deathSound;
    [Range(0f, 1f)] public float deathVolume = 1.0f;     

    private float lastHurtSoundTime = -999f; 
    private float hurtSoundCooldown = 2.5f; 

    [Header("VFX Spawn Points")]
    public Transform vfxSpawnPoint; 
    public GameObject attackedVFX;
    public GameObject deathVFX;
    public float vfxDestroyTime = 3f;

    private AudioSource audioSource;
    private bool isDead;
    private bool isInvincible;
    private float invincibilityTimer;

    void Start()
    {
        if (player == null) player = gameObject; 
        if (gameManager == null) gameManager = GameManager.Instance;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (vfxSpawnPoint == null) vfxSpawnPoint = transform;
        if (uiManager == null) uiManager = UiManager.instance;

        // Unparent the arrow so player flipping/rotation doesn't break its tracking
        if (playerArrow != null)
        {
            playerArrow.transform.SetParent(null);
            playerArrow.SetActive(false);
        }

        // Randomly select one exit zone script out of the available ones (2 to 4)
        SelectRandomExitZone();

        UpdateUIHealth();
    }

    void Update()
    {
        if (isDead) return;

        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f) isInvincible = false;
        }

        if (regenAmountPerSecond > 0f && currentHealth < maxHealth)
        {
            currentHealth += regenAmountPerSecond * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth, maxHealth); 
            UpdateUIHealth();
        }

        CheckEnemiesAndHandleArrow();
    }

    private void SelectRandomExitZone()
    {
        GameObject[] exitZones = GameObject.FindGameObjectsWithTag("ExitZone");
        if (exitZones.Length > 0)
        {
            int randomIndex = Random.Range(0, exitZones.Length);
            selectedExitZone = exitZones[randomIndex].GetComponent<FinishZone>();
            Debug.Log($"[PlayerHealth] Selected active exit zone: {selectedExitZone.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] No GameObjects with tag 'ExitZone' found in the scene!");
        }
    }

    private void CheckEnemiesAndHandleArrow()
    {
        if (gameManager == null || selectedExitZone == null || playerArrow == null) return;

        // Check GameManager's enemy defeat status
        bool allEnemiesCleared = gameManager.AreAllEnemiesDefeated;

        // Toggle arrow visibility based on GameManager
        playerArrow.SetActive(allEnemiesCleared);

        // If all enemies are defeated, keep the arrow locked to the player and continuously pointing at the exit
        if (allEnemiesCleared)
        {
            // Keep arrow positioned right above the player
            playerArrow.transform.position = transform.position + arrowOffset;

            // Continuously calculate angle toward the exit zone
            Vector3 direction = selectedExitZone.transform.position - playerArrow.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Apply rotation (+ 180f accounts for your left-pointing arrow sprite)
            playerArrow.transform.rotation = Quaternion.Euler(0, 0, angle + 180f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible) return;

        float defense = (gameManager != null) ? gameManager.playerDefense : 0f;
        float finalDamage = Mathf.Max(1f, damage - defense); 

        currentHealth -= finalDamage;
        if (currentHealth < 0f) currentHealth = 0f;
        
        UpdateUIHealth();

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        if (attackedSound != null && Time.time >= lastHurtSoundTime + hurtSoundCooldown)
        {
            lastHurtSoundTime = Time.time;
            float globalSfxVolume = (AudioManager.Instance != null) ? AudioManager.Instance.GetSFXVolume() : 1.0f;
            audioSource.PlayOneShot(attackedSound, globalSfxVolume * attackedVolume);
        }

        if (attackedVFX != null)
        {
            GameObject effect = Instantiate(attackedVFX, vfxSpawnPoint.position, vfxSpawnPoint.rotation);
            Destroy(effect, vfxDestroyTime);
        }

        if (currentHealth <= 0f) Die();
    }

    private void UpdateUIHealth()
    {
        if (uiManager != null) 
        {
            uiManager.UpdateHealthPoints(currentHealth, maxHealth);
        }
        else if (UiManager.instance != null)
        {
            UiManager.instance.UpdateHealthPoints(currentHealth, maxHealth);
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (playerArrow != null) playerArrow.SetActive(false);

        Time.timeScale = 0f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndRunAndCheckRecord();
        }

        if (uiManager != null)
        {
            uiManager.ShowLosePanel();
        }
        else if (UiManager.instance != null)
        {
            UiManager.instance.ShowLosePanel();
        }

        if (deathSound != null)
        {
            float globalSfxVolume = (AudioManager.Instance != null) ? AudioManager.Instance.GetSFXVolume() : 1.0f;
            audioSource.PlayOneShot(deathSound, globalSfxVolume * deathVolume);
        }
    }

    public void IncreaseMaxHealth(float percentIncrease, bool healToFull = true)
    {
        float bonusHealth = maxHealth * (percentIncrease / 100f);
        maxHealth += bonusHealth;

        if (healToFull) currentHealth = maxHealth; 
        else currentHealth += bonusHealth; 

        UpdateUIHealth();
    }

    public void AddRegeneration(float amountPerSecond)
    {
        regenAmountPerSecond += amountPerSecond;
    }
}