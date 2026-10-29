using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement & Separation")]
    public float moveSpeed = 3.5f;

    [Header("Health")]
    public float maxHealth = 100f;
    public Slider healthBar;

    [Header("Enemy Attack")]
    public float attackDamage = 20f;
    public float attackCooldown = 1f;
    public float attackAnimationDuration = 0.5f;

    [Header("Room Difficulty Scaling")]
    [SerializeField] private float speedScalingPerRoom = 0.02f;
    [SerializeField] private float maxSpeedMultiplier = 1.25f;
    [SerializeField] private float healthScalingPerRoom = 0.05f;
    [SerializeField] private float damageScalingPerRoom = 5f;

    [Header("Sounds & Specific Volumes")]
    public AudioClip attackedSound;
    [Range(0f, 1f)] public float attackedVolume = 1.0f;
    public AudioClip attackSound;
    [Range(0f, 1f)] public float attackVolume = 1.0f;
    public AudioClip deathSound;
    [Range(0f, 1f)] public float deathVolume = 1.0f;

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
    private NavMeshAgent navAgent;

    private float currentHealth;
    private float lastAttackTime = -999f;
    private float attackEndTime = 0f;
    private float nextDestinationUpdateTime;
    private const float destinationUpdateInterval = 0.2f;
    private const float destinationUpdateDistance = 0.5f;

    private bool isDead;
    private bool touchingPlayer;
    private bool walking;
    private bool isAttacking;
    private bool warnedOffNavMesh;
    private bool warnedDestinationFailure;
    private bool warnedInvalidPath;

    void Start()
    {
        if (!CompareTag("Enemy"))
        {
            Debug.LogWarning(gameObject.name + " does not have the Enemy tag!");
        }

        int roomCount = (SceneController.Instance != null) ? SceneController.Instance.GetRoomCount() : 0;

        float speedMultiplier = Mathf.Min(1f + (roomCount * speedScalingPerRoom), maxSpeedMultiplier);
        moveSpeed *= speedMultiplier;
        maxHealth *= 1f + (roomCount * healthScalingPerRoom);
        attackDamage += roomCount * damageScalingPerRoom;

        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        navAgent = GetComponent<NavMeshAgent>();

        if (navAgent == null)
        {
            Debug.LogError(gameObject.name + " needs a NavMeshAgent to move.", this);
        }
        else
        {
            navAgent.speed = moveSpeed;
            navAgent.stoppingDistance = 0.3f;
            navAgent.updateRotation = false;
            navAgent.updateUpAxis = false;
            navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            navAgent.autoRepath = true;
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

        if (playerObject == null)
        {
            GameObject[] taggedPlayers = GameObject.FindGameObjectsWithTag("Player");
            if (taggedPlayers.Length > 0)
            {
                playerObject = taggedPlayers[0];
            }
        }

        if (playerObject != null)
        {
            Transform armature = playerObject.transform.Find("PlayerArmature");
            if (armature == null)
            {
                armature = playerObject.transform.Find("Armature");
            }

            player = armature != null ? armature : playerObject.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        // Continuously ensure we have a valid reference to the active player
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        if (isAttacking)
        {
            if (Time.time >= attackEndTime)
            {
                isAttacking = false;
            }
            else
            {
                SetAgentStopped(true);
                SetWalking(false);
                return;
            }
        }

        if (touchingPlayer)
        {
            SetAgentStopped(true);
            SetWalking(false);
            return;
        }

        if (navAgent == null || !navAgent.isActiveAndEnabled)
        {
            SetWalking(false);
            return;
        }

        if (!navAgent.isOnNavMesh)
        {
            SetWalking(false);
            if (!warnedOffNavMesh)
            {
                Debug.LogWarning(gameObject.name + " NavMeshAgent is not on a baked NavMesh. Check the room's NavMesh data and the enemy spawn position.", this);
                warnedOffNavMesh = true;
            }
            return;
        }

        warnedOffNavMesh = false;
        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = navAgent.agentTypeID,
            areaMask = navAgent.areaMask
        };
        Vector3 chaseTarget = player.position;
        if (player != null)
        {
            Transform targetRoot = player;
            if (targetRoot != null)
            {
                chaseTarget = targetRoot.position;
            }
        }

        float playerSampleDistance = Mathf.Max(navAgent.height * 2f, 20f);
        if (!NavMesh.SamplePosition(chaseTarget, out NavMeshHit playerNavMeshHit, playerSampleDistance, filter))
        {
            SetWalking(false);
            if (!warnedDestinationFailure)
            {
                Debug.LogWarning(gameObject.name + " could not find a NavMesh position for the player target.", this);
                warnedDestinationFailure = true;
            }
            return;
        }

        if (Time.time < nextDestinationUpdateTime &&
            (navAgent.pathPending ||
             (navAgent.hasPath && (playerNavMeshHit.position - navAgent.destination).sqrMagnitude < destinationUpdateDistance * destinationUpdateDistance)))
        {
            UpdateWalkingState();
            return;
        }

        nextDestinationUpdateTime = Time.time + destinationUpdateInterval;
        navAgent.isStopped = false;
        if (!navAgent.SetDestination(playerNavMeshHit.position))
        {
            SetWalking(false);
            if (!warnedDestinationFailure)
            {
                Debug.LogWarning(gameObject.name + " could not set a NavMesh destination for the player.", this);
                warnedDestinationFailure = true;
            }
            return;
        }

        warnedDestinationFailure = false;
        UpdateWalkingState();
    }

    void UpdateWalkingState()
    {
        if (navAgent == null || !navAgent.isActiveAndEnabled) return;

        if (!navAgent.pathPending && navAgent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            SetWalking(false);
            if (!warnedInvalidPath)
            {
                Debug.LogWarning(gameObject.name + " has no valid NavMesh path to the player. Check that both are on connected walkable NavMesh.", this);
                warnedInvalidPath = true;
            }
            return;
        }

        warnedInvalidPath = false;
        Vector3 direction = navAgent.velocity;
        direction.y = 0f;
        bool moving = direction.sqrMagnitude > 0.01f;

        SetWalking(moving);

        if (moving && spriteRenderer != null && direction.x != 0f)
        {
            spriteRenderer.flipX = direction.x < 0f;
        }
    }

    void SetAgentStopped(bool stopped)
    {
        if (navAgent != null && navAgent.isActiveAndEnabled && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = stopped;
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
        float damageToDeal = (GameManager.Instance != null) ? GameManager.Instance.playerDamage : 15f;
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
        if (collision.gameObject.CompareTag("Player")) touchingPlayer = false;
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
        if (other.CompareTag("Player")) touchingPlayer = false;
    }

    void AttackPlayer()
    {
        if (isDead || !touchingPlayer || Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        isAttacking = true;
        attackEndTime = Time.time + attackAnimationDuration;
        walking = false;
        SetAgentStopped(true);

        if (animator != null) animator.Play("Attack01", 0, 0f);

        if (attackSound != null)
        {
            float globalSfxVolume = (AudioManager.Instance != null) ? AudioManager.Instance.GetSFXVolume() : 1.0f;
            audioSource.PlayOneShot(attackSound, globalSfxVolume * attackVolume);
        }

        if (attackVFX != null)
        {
            GameObject effect = Instantiate(attackVFX, transform.position + attackVFXOffset, Quaternion.identity);
            Destroy(effect, vfxDestroyTime);
        }

        PlayerHealth playerHealth = player != null ? player.GetComponent<PlayerHealth>() : null;
        if (playerHealth == null && player != null) playerHealth = player.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null) playerHealth.TakeDamage(attackDamage);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        if (healthBar != null) healthBar.value = currentHealth;

        if (attackedSound != null)
        {
            float globalSfxVolume = (AudioManager.Instance != null) ? AudioManager.Instance.GetSFXVolume() : 1.0f;
            audioSource.PlayOneShot(attackedSound, globalSfxVolume * attackedVolume);
        }

        if (attackedVFX != null)
        {
            GameObject effect = Instantiate(attackedVFX, transform.position + attackedVFXOffset, Quaternion.identity);
            Destroy(effect, vfxDestroyTime);
        }

        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        touchingPlayer = false;
        SetAgentStopped(true);
        if (navAgent != null) navAgent.enabled = false;

        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = false;
        }

        if (GameManager.Instance != null) GameManager.Instance.EnemyDefeated();

        DropCoins();

        if (animator != null) animator.Play("Death", 0, 0f);

        if (AudioManager.Instance != null && deathSound != null)
        {
            AudioManager.Instance.PlaySFX(deathSound, deathVolume);
        }

        if (deathVFX != null)
        {
            GameObject effect = Instantiate(deathVFX, transform.position + deathVFXOffset, Quaternion.identity);
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