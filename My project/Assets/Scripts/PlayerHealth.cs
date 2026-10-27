using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private GameObject player; 

    [Header("Game Manager Reference")]
    [SerializeField] private GameManager gameManager; 

    [Header("UI Integration")]
    [SerializeField] private UiManager uiManager; 

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
                // Debug check if health unexpectedly refills
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

        UpdateUIHealth();
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f) isInvincible = false;
        }

        if (regenAmountPerSecond > 0f && currentHealth < maxHealth && !isDead)
        {
            currentHealth += regenAmountPerSecond * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth, maxHealth); 
            UpdateUIHealth();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible) return;

        float defense = (gameManager != null) ? gameManager.playerDefense : 0f;
        float finalDamage = Mathf.Max(1f, damage - defense); 

        currentHealth -= finalDamage;
        if (currentHealth < 0f) currentHealth = 0f;
        
        Debug.Log($"[Damage Taken] Took {finalDamage} damage. Current Health is now: {currentHealth}");
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

    // 1. Freeze time completely so nothing moves
    Time.timeScale = 0f;

    // 2. Save and check best time record
    if (GameManager.Instance != null)
    {
        GameManager.Instance.EndRunAndCheckRecord();
    }

    // 3. Find and show the Lose Panel via UiManager
    if (uiManager != null)
    {
        uiManager.ShowLosePanel();
        Debug.Log("[PlayerHealth] Successfully triggered UiManager Lose Panel.");
    }
    else if (UiManager.instance != null)
    {
        UiManager.instance.ShowLosePanel();
        Debug.Log("[PlayerHealth] Successfully triggered UiManager.instance Lose Panel.");
    }
    else
    {
        Debug.LogError("[PlayerHealth] Could not find UiManager reference to show lose panel!");
    }

    // 4. Play death sound
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

        if (healToFull)
        {
            currentHealth = maxHealth; 
        }
        else
        {
            currentHealth += bonusHealth; 
        }

        UpdateUIHealth();
        Debug.Log($"Max Health increased by {percentIncrease}%! New Max Health: {maxHealth}");
    }

    public void AddRegeneration(float amountPerSecond)
    {
        regenAmountPerSecond += amountPerSecond;
        Debug.Log($"Regeneration increased! Current Regen: {regenAmountPerSecond} HP/sec");
    }
}