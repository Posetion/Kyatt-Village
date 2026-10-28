using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private GameObject player; 

    private bool hasSpunOnAppear = false;
    private float spinTimer = 0f;
    [Header("Arrow Spin Settings")]
    [SerializeField] private float spinDuration = 1.0f; // How long the spin lasts in seconds
    [SerializeField] private float spinSpeed = 720f;   // Degrees per second (720 = 2 full rotations)


    [Header("Game Manager Reference")]
    [SerializeField] private GameManager gameManager; 

    [Header("UI Integration")]
    [SerializeField] private UiManager uiManager; 

    [Header("Exit Objective & Arrow")]
    [Tooltip("Drag the 3D arrow GameObject (nested in armature) here.")]
    [SerializeField] private GameObject playerArrow;
    [Tooltip("Height offset if needed (leave 0,0,0 if already positioned on head bone).")]
    [SerializeField] private Vector3 arrowOffset = Vector3.zero;
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

        // Keep it hidden at start. Respects your armature setup!
        if (playerArrow != null)
        {
            playerArrow.SetActive(false);
        }

        // Randomly select one exit zone script out of the available ones
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

        // Check GameManager's enemy defeat status using your property
        bool allEnemiesCleared = gameManager.AreAllEnemiesDefeated;

        // If enemies are NOT cleared, reset the spin trigger so it spins again next time
        if (!allEnemiesCleared)
        {
            hasSpunOnAppear = false;
            spinTimer = 0f;
            playerArrow.SetActive(false);
            return;
        }

        // If they ARE cleared and the arrow wasn't active yet, turn it on and start the spin timer
        if (!playerArrow.activeSelf)
        {
            playerArrow.SetActive(true);
            hasSpunOnAppear = false;
            spinTimer = spinDuration;
        }

        // Handle the spin effect when it first appears
        if (!hasSpunOnAppear)
        {
            spinTimer -= Time.deltaTime;
            
            // Spin around the Y-axis (or whichever local axis you prefer)
            playerArrow.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);

            if (spinTimer <= 0f)
            {
                hasSpunOnAppear = true; // Spin finished, move to pointing mode
            }
        }
        else
        {
            // Once the spin is done, continuously point toward the active exit zone
            Vector3 direction = selectedExitZone.transform.position - playerArrow.transform.position;

            // OPTIONAL: Uncomment if you want the arrow to only rotate horizontally (flat compass style)
            // direction.y = 0f;

            if (direction != Vector3.zero)
            {
                playerArrow.transform.rotation = Quaternion.LookRotation(direction);
            }
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