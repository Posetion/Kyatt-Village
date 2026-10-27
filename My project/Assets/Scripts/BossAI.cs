using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CatBossAI : MonoBehaviour
{
    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundCheckHeight = 10f;
    public float groundCheckDistance = 30f;
    public float groundOffset = 0.02f;

    [Header("Movement")]
    public float moveSpeed = 3.5f;

    [Header("Health")]
    public float maxHealth = 100f;
    public Slider healthBar;

    [Header("Combat Settings")]
    public float shootCooldown = 45f;      
    private float lastShootTime = -999f;
    
    public float meleeAttackDamage = 20f;
    public float meleeAttackCooldown = 1.5f;
    public float meleeAttackDuration = 1.0f;
    public float clickDamage = 10f;

    public float shootAttackDuration = 0.8f;
    public float reloadAttackDuration = 1.2f;
    public float hurtAnimationDuration = 0.4f;

    [Header("Distance / Alignment Settings")]
    public float lineAlignmentMargin = 0.5f; 

    [Header("Sounds & FX")]
    public AudioClip attackedSound;
    public AudioClip attackSound;
    public AudioClip deathSound;
    public GameObject attackedVFX;
    public GameObject attackVFX;
    public GameObject deathVFX;
    public float vfxDestroyTime = 3f;

    [Header("Immunity Settings")]
    public float iFramesDuration = 1.0f;
    private bool isInvincible = false;

    [Header("Loot / Coins")]
    public GameObject coinPrefab;
    public int minCoins = 5;
    public int maxCoins = 10;
    public float coinScatterRadius = 1.5f;
    public float coinPopForce = 4f;
    public Vector3 coinSpawnOffset = new Vector3(0f, 0.5f, 0f);

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private AudioSource audioSource;
    private Rigidbody rb;

    private float currentHealth;
    private float lastMeleeTime = -999f;
    private float actionEndTime = 0f;

    private bool isDead;
    private bool touchingPlayer;
    private bool walking;
    private bool isBusy; 

    void Start()
    {
        if (!CompareTag("Enemy"))
        {
            Debug.LogWarning(gameObject.name + " does not have the Enemy tag!");
        }

        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.freezeRotation = true;
        }

        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        FindPlayer();
        PlaceOnGround();
    }

    void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            playerObject = GameObject.Find("PlayerArmature");
        }

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        // Manage action/animation locks (shooting, reloading, hurt stun)
        if (isBusy)
        {
            if (Time.time >= actionEndTime)
            {
                isBusy = false;
            }
            else
            {
                // Ensures zero movement while busy (shooting/reloading/hurt)
                SetWalking(false);
                return;
            }
        }

        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        bool inSameLine = Mathf.Abs(transform.position.z - player.position.z) <= lineAlignmentMargin;

        // 1. MELEE PRIORITY
        if (touchingPlayer)
        {
            SetWalking(false);
            animator.SetBool("isClose", true);

            if (Time.time >= lastMeleeTime + meleeAttackCooldown)
            {
                PerformMeleeAttack();
            }
        }
        else
        {
            animator.SetBool("isClose", false);

            // 2. SHOOT & RELOAD SEQUENCE
            if (inSameLine && Time.time >= lastShootTime + shootCooldown)
            {
                SetWalking(false);
                StartCoroutine(ShootAndReloadSequence());
            }
        }
    }

    void FixedUpdate()
    {
        // Added `isBusy` check here so it never moves while reloading, shooting, or hurt
        if (isDead || player == null || isBusy || touchingPlayer)
        {
            return;
        }

        MoveTowardPlayer();
    }

    void MoveTowardPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            SetWalking(false);
            return;
        }

        direction.Normalize();

        Vector3 newPosition = transform.position + direction * moveSpeed * Time.fixedDeltaTime;
        RaycastHit hit;
        Vector3 rayStart = newPosition + Vector3.up * groundCheckHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            float spriteBottomOffset = 0f;
            if (spriteRenderer != null)
            {
                spriteBottomOffset = transform.position.y - spriteRenderer.bounds.min.y;
            }
            newPosition.y = hit.point.y + spriteBottomOffset + groundOffset;
        }
        else
        {
            newPosition.y = transform.position.y;
        }

        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            transform.position = newPosition;
        }

        if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.05f)
        {
            spriteRenderer.flipX = direction.x < 0f;
        }

        SetWalking(true);
    }

    void PlaceOnGround()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * groundCheckHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            float spriteBottomOffset = 0f;
            if (spriteRenderer != null)
            {
                spriteBottomOffset = transform.position.y - spriteRenderer.bounds.min.y;
            }

            Vector3 newPosition = transform.position;
            newPosition.y = hit.point.y + spriteBottomOffset + groundOffset;
            transform.position = newPosition;
        }
    }

    void SetWalking(bool shouldWalk)
    {
        if (animator == null || shouldWalk == walking) return;

        walking = shouldWalk;
        animator.SetBool("IsWalking", shouldWalk);
    }

    // --- MELEE ATTACK ---

    void PerformMeleeAttack()
    {
        lastMeleeTime = Time.time;
        isBusy = true;
        actionEndTime = Time.time + meleeAttackDuration;
        walking = false;

        animator.SetBool("isClose", true);

        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    // --- SHOOT & RELOAD 45s SEQUENCE ---

    IEnumerator ShootAndReloadSequence()
    {
        isBusy = true;
        lastShootTime = Time.time; 
        walking = false;
        SetWalking(false);

        // 1. Play Shoot Animation
        animator.SetTrigger("shoot");
        yield return new WaitForSeconds(shootAttackDuration);

        // 2. Play Reload Animation (Boss remains completely still here)
        animator.SetTrigger("reload");
        yield return new WaitForSeconds(reloadAttackDuration);

        isBusy = false;
    }

    // --- COLLIDER & TRIGGER DETECTION ---

    void OnCollisionEnter(Collision collision)
    {
        if (isDead || !collision.gameObject.CompareTag("Player")) return;
        touchingPlayer = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            touchingPlayer = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead || !other.CompareTag("Player")) return;
        touchingPlayer = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            touchingPlayer = false;
        }
    }

    // --- HEALTH, HURT & IMMUNITY ---

    public void TakeDamage(float damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        if (currentHealth < 0f) currentHealth = 0f;

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        animator.SetTrigger("hurt");
        StartCoroutine(ImmunityRoutine());

        if (attackedSound != null)
        {
            audioSource.PlayOneShot(attackedSound);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    IEnumerator ImmunityRoutine()
    {
        isInvincible = true;
        isBusy = true;
        actionEndTime = Time.time + hurtAnimationDuration;
        walking = false;
        SetWalking(false);

        yield return new WaitForSeconds(hurtAnimationDuration);
        isBusy = false;

        yield return new WaitForSeconds(iFramesDuration - hurtAnimationDuration);
        isInvincible = false;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        touchingPlayer = false;

        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Notify Game Manager that an enemy/boss was defeated
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyDefeated();
        }

        // Drop the coins upon defeat
        DropCoins();

        if (animator != null)
        {
            animator.Play("Death", 0, 0f);
        }

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        Destroy(gameObject, 1.2f);
    }

    void DropCoins()
    {
        if (coinPrefab == null) return;

        int coinCount = Random.Range(minCoins, maxCoins + 1);
        Vector3 baseSpawnPos = transform.position + coinSpawnOffset;

        for (int i = 0; i < coinCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * coinScatterRadius;
            Vector3 spawnPosition = baseSpawnPos + new Vector3(randomCircle.x, 0f, randomCircle.y);

            GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);

            Rigidbody coinRb = coin.GetComponent<Rigidbody>();
            if (coinRb != null)
            {
                Vector3 burstForce = new Vector3(randomCircle.x * 2f, coinPopForce, randomCircle.y * 2f);
                coinRb.AddForce(burstForce, ForceMode.Impulse);
            }
        }
    }

    // --- CLICK TO ATTACK (Powered by GameManager) ---

    void OnMouseDown()
    {
        if (isDead) return;

        float finalDamage = clickDamage; // Fallback default

        // Pull damage dynamically from GameManager if available
        if (GameManager.Instance != null)
        {
            finalDamage = GameManager.Instance.playerDamage;
        }

        TakeDamage(finalDamage);
    }
}