using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement & Separation")]
    public float moveSpeed = 3.5f;
    public float separationRadius = 1.5f; 
    public float separationWeight = 1.2f; 




    [Header("Health")]
    public float maxHealth = 100f;
    public Slider healthBar;

    [Header("Enemy Attack")]
    public float attackDamage = 20f;
    public float attackCooldown = 1f;
    public float attackAnimationDuration = 0.5f;

    [Header("Room Difficulty Scaling")]
    [Tooltip("How much speed is added per room completed.")]
    [SerializeField] private float speedScalingPerRoom = 0.15f;
    [Tooltip("Percentage increase of health per room completed (e.g., 0.10 = +10% per room).")]
    [SerializeField] private float healthScalingPerRoom = 0.12f;
    [Tooltip("How much attack damage is added per room completed.")]
    [SerializeField] private float damageScalingPerRoom = 2f;

    [Header("Sounds & Specific Volumes")]
    public AudioClip attackedSound;
    [Range(0f, 1f)] public float attackedVolume = 1.0f; // Volume slider for hit sound

    public AudioClip attackSound;
    [Range(0f, 1f)] public float attackVolume = 1.0f;   // Volume slider for attack sound

    public AudioClip deathSound;
    [Range(0f, 1f)] public float deathVolume = 1.0f;    // Volume slider for death sound

    [Header("VFX")]
    public GameObject attackedVFX;
    public Vector3 attackedVFXOffset = new Vector3(0f, 1f, 0f);

    public GameObject attackVFX;
    public Vector3 attackVFXOffset = new Vector3(0f, 1f, 0f);

    public GameObject deathVFX;
    public Vector3 deathVFXOffset = new Vector3(0f, 0.5f, 0f);

    public float vfxDestroyTime = 3f;

    [Header("Loot / Coins")]
    public GameObject coinPrefab;
    public int minCoins = 3;
    public int maxCoins = 6;
    public float coinScatterRadius = 1.2f;
    public float coinPopForce = 4f;
    public Vector3 coinSpawnOffset = new Vector3(0f, 0.5f, 0f);

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private AudioSource audioSource;
    private NavMeshAgent agent;

    private float currentHealth;
    private float lastAttackTime = -999f;
    private float attackEndTime = 0f;

    private bool isDead;
    private bool touchingPlayer;
    private bool walking;
    private bool isAttacking;

    void Start()
    {
        if (!CompareTag("Enemy"))
        {
            Debug.LogWarning(gameObject.name + " does not have the Enemy tag!");
        }

        // Calculate scaling based on current room count from SceneController
        int roomCount = (SceneController.Instance != null) ? SceneController.Instance.GetRoomCount() : 0;

        // Scale Stats
        moveSpeed += roomCount * speedScalingPerRoom;
        maxHealth += maxHealth * (roomCount * healthScalingPerRoom);
        attackDamage += roomCount * damageScalingPerRoom;

        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.updateRotation = false; 
            agent.updateUpAxis = false;   
            agent.stoppingDistance = 0.2f; 
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance; 
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        FindPlayer();
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

        if (isAttacking)
        {
            if (Time.time >= attackEndTime)
            {
                isAttacking = false;
            }
            else
            {
                if (agent != null && agent.enabled) agent.isStopped = true;
                return;
            }
        }

        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        if (touchingPlayer)
        {
            if (agent != null && agent.enabled) agent.isStopped = true;
            SetWalking(false);
            return;
        }

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            MoveTowardPlayer();
        }
    }

    void MoveTowardPlayer()
    {
        if (player == null || agent == null) return;

        agent.SetDestination(player.position);

        Vector3 horizontalVelocity = new Vector3(agent.velocity.x, 0f, agent.velocity.z);
        bool isMoving = horizontalVelocity.sqrMagnitude > 0.01f;
        
        SetWalking(isMoving);

        if (isMoving && spriteRenderer != null)
        {
            if (Mathf.Abs(agent.velocity.x) > 0.05f)
            {
                spriteRenderer.flipX = agent.velocity.x < 0f;
            }
        }
    }

    void SetWalking(bool shouldWalk)
    {
        if (animator == null || shouldWalk == walking) return;

        walking = shouldWalk;

        if (shouldWalk)
        {
            animator.Play("Walk", 0, 0f);
        }
        else
        {
            animator.Play("Idle", 0, 0f);
        }
    }

    void OnMouseDown()
    {
        if (isDead) return;

        float damageToDeal = 15f; 

        if (GameManager.Instance != null)
        {
            damageToDeal = GameManager.Instance.playerDamage;
        }

        TakeDamage(damageToDeal);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead || !collision.gameObject.CompareTag("Player")) return;
        touchingPlayer = true;
        AttackPlayer();
    }

    void OnCollisionStay(Collision collision)
    {
        if (isDead || !collision.gameObject.CompareTag("Player")) return;
        touchingPlayer = true;
        AttackPlayer();
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
        AttackPlayer();
    }

    void OnTriggerStay(Collider other)
    {
        if (isDead || !other.CompareTag("Player")) return;
        touchingPlayer = true;
        AttackPlayer();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            touchingPlayer = false;
        }
    }

    void AttackPlayer()
    {
        if (isDead || !touchingPlayer || Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;
        isAttacking = true;
        attackEndTime = Time.time + attackAnimationDuration;
        walking = false;

        if (animator != null)
        {
            animator.Play("Attack01", 0, 0f);
        }

        if (attackSound != null)
        {
            float globalSfxVolume = (AudioManager.Instance != null) ? AudioManager.Instance.GetSFXVolume() : 1.0f;
            audioSource.PlayOneShot(attackSound, globalSfxVolume * attackVolume);
        }

        if (attackVFX != null)
        {
            Vector3 spawnPosition = transform.position + attackVFXOffset;
            GameObject effect = Instantiate(attackVFX, spawnPosition, Quaternion.identity);
            Destroy(effect, vfxDestroyTime);
        }

        PlayerHealth playerHealth = null;

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = player.GetComponentInParent<PlayerHealth>();
            }
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth < 0f)
        {
            currentHealth = 0f;
        }

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (attackedSound != null)
        {
            float globalSfxVolume = (AudioManager.Instance != null) ? AudioManager.Instance.GetSFXVolume() : 1.0f;
            audioSource.PlayOneShot(attackedSound, globalSfxVolume * attackedVolume);
        }

        if (attackedVFX != null)
        {
            Vector3 spawnPosition = transform.position + attackedVFXOffset;
            GameObject effect = Instantiate(attackedVFX, spawnPosition, Quaternion.identity);
            Destroy(effect, vfxDestroyTime);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        touchingPlayer = false;

        if (agent != null)
        {
            agent.enabled = false;
        }

        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyDefeated();
        }

        DropCoins();

        if (animator != null)
        {
            animator.Play("Death", 0, 0f);
        }

        if (AudioManager.Instance != null && deathSound != null)
        {
            AudioManager.Instance.PlaySFX(deathSound, deathVolume);
        }

        if (deathVFX != null)
        {
            Vector3 spawnPosition = transform.position + deathVFXOffset;
            GameObject effect = Instantiate(deathVFX, spawnPosition, Quaternion.identity);
            Destroy(effect, vfxDestroyTime);
        }

        Destroy(gameObject, 1.2f);
    }

    void DropCoins()
    {
        if (coinPrefab == null) return;

        float playerLuck = (GameManager.Instance != null) ? GameManager.Instance.playerLuck : 0f;
        float luckMultiplier = 1f + (playerLuck * 0.05f); 

        int adjustedMin = Mathf.RoundToInt(minCoins * luckMultiplier);
        int adjustedMax = Mathf.RoundToInt(maxCoins * luckMultiplier);

        int coinCount = Random.Range(adjustedMin, adjustedMax + 1);
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
}