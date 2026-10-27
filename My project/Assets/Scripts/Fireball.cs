using UnityEngine;
using UnityEngine.UI;

public class Fireball : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Target Height")]
    public float targetHeight = 1.0f;

    [Header("Health")]
    public float maxHealth = 100f;
    public Slider healthBar;

    [Header("Attack Settings")]
    public float attackCooldown = 0.2f;

    [Header("Player Contact Damage")]
    public float contactDamage = 25f; // Damage dealt when touching the player directly

    [Header("Explosion on Death")]
    public float explosionDamage = 50f;
    public float explosionRadius = 4f;

    [Header("Sounds & Specific Volumes")]
    public AudioClip attackedSound;
    [Range(0f, 1f)] public float attackedVolume = 1.0f;

    public AudioClip deathSound;
    [Range(0f, 1f)] public float deathVolume = 1.0f;    

    [Header("VFX")]
    public GameObject attackedVFX;
    public GameObject deathVFX;
    public float vfxDestroyTime = 3f;

    [Header("Death Ground Circle VFX (Multiple)")]
    public GameObject deathGroundCircleVFXPrefab; 
    public int deathCircleSpawnCount = 5;         
    public float deathCircleRadius = 3f;          
    public float deathCircleDuration = 2.0f;      

    [Header("Loot / Coins")]
    public GameObject coinPrefab;
    public int minCoins = 3;
    public int maxCoins = 6;
    public float coinScatterRadius = 1.2f;
    public float coinPopForce = 4f;
    public Vector3 coinSpawnOffset = new Vector3(0f, 0.5f, 0f);

    private float currentHealth;
    private float lastAttackTime = -999f;
    private Transform player;
    private AudioSource audioSource;
    private bool isDead = false;

    void Start()
    {
        if (!CompareTag("Enemy"))
        {
            Debug.LogWarning(gameObject.name + " does not have the 'Enemy' tag! GameManager will not count it at start.");
        }

        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (isDead) return;

        // Move towards the player
        if (player != null)
        {
            Vector3 targetPosition = player.position + Vector3.up * targetHeight;
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(targetPosition);
        }
        
        // (Mouse click & touch detection removed completely!)
    }

    // Handles triggers entering the fireball (Player attacks or Player contact)
    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        // 1. Check if hit by the Player's Attack Zone / Weapon Hitbox
        // (Make sure your player's attack zone has the tag "PlayerAttack" or similar, 
        //  or check whichever tag/layer your other enemies use to detect player attacks)
        if (other.CompareTag("PlayerAttack") || other.name.Contains("Weapon") || other.name.Contains("AttackZone"))
        {
            // You can pull damage directly from the player's weapon script if they have one, 
            // or use a default damage value (e.g., 25f).
            float damageToTake = 25f; 
            
            // Optional: If your weapon script has a damage variable, you can fetch it:
            // WeaponScript weapon = other.GetComponent<WeaponScript>();
            // if (weapon != null) damageToTake = weapon.damage;

            TakeDamage(damageToTake);
        }

        // 2. Check if the player body itself touched the fireball
        if (other.CompareTag("Player") || other.transform.IsChildOf(player))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = other.GetComponentInParent<PlayerHealth>();
            }

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                Debug.Log("Player touched fireball and took contact damage!");
            }

            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;
        currentHealth -= damage;

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
            GameObject effect = Instantiate(attackedVFX, transform.position, Quaternion.identity);
            Destroy(effect, vfxDestroyTime);
        }

        Debug.Log("Fireball took damage! Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyDefeated();
        }

        Explode();
        DropCoins();

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        if (deathSound != null)
        {
            float globalSfxVolume = (AudioManager.Instance != null) ? AudioManager.Instance.GetSFXVolume() : 1.0f;
            float finalVolume = globalSfxVolume * deathVolume;
            AudioSource.PlayClipAtPoint(deathSound, transform.position, finalVolume);
        }

        if (deathVFX != null)
        {
            GameObject effect = Instantiate(deathVFX, transform.position, Quaternion.identity);
            Destroy(effect, vfxDestroyTime);
        }

        if (deathGroundCircleVFXPrefab != null)
        {
            for (int i = 0; i < deathCircleSpawnCount; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * deathCircleRadius;
                Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

                GameObject circleEffect = Instantiate(deathGroundCircleVFXPrefab, spawnPosition, Quaternion.identity);
                Destroy(circleEffect, deathCircleDuration);
            }
        }

        Destroy(gameObject, 0.2f);
    }

    void Explode()
    {
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerHealth.transform.position);
            if (distanceToPlayer <= explosionRadius)
            {
                playerHealth.TakeDamage(explosionDamage);
            }
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hitCol in hitColliders)
        {
            if (hitCol.gameObject == gameObject || hitCol.transform.IsChildOf(transform))
                continue;

            if (hitCol.CompareTag("Enemy"))
            {
                hitCol.SendMessage("TakeDamage", explosionDamage, SendMessageOptions.DontRequireReceiver);
            }
        }
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