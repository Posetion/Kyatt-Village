using UnityEngine;
using System.Collections.Generic;

public class PlayerAutoAttack : MonoBehaviour
{
    [Header("Game Manager Reference")]
    [SerializeField] private GameManager gameManager;

    [Header("Attack Settings")]
    [SerializeField] private float baseAttackInterval = 1.0f; 

    private List<Collider> enemiesInRange = new List<Collider>();
    private float attackTimer = 0f;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc == null)
        {
            Debug.LogError("[PlayerAutoAttack] CRITICAL: No Sphere Collider found on the Player object!");
        }
        else if (!sc.isTrigger)
        {
            sc.isTrigger = true;
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        attackTimer += Time.deltaTime;

        float currentAttackSpeed = (gameManager != null) ? gameManager.playerAttackSpeed : 1f;
        currentAttackSpeed = Mathf.Max(0.1f, currentAttackSpeed);
        float effectiveInterval = baseAttackInterval / currentAttackSpeed;

        if (attackTimer >= effectiveInterval)
        {
            attackTimer = 0f;
            PerformAutomaticAttack();
        }
    }

    private void PerformAutomaticAttack()
    {
        // Clean up null/destroyed objects
        enemiesInRange.RemoveAll(col => col == null || col.gameObject == null);

        if (enemiesInRange.Count == 0) return;

        Collider targetCollider = null;
        Fireball targetFireball = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        // --- STEP 1: PRIORITIZE FIREBALLS (FLATTEN Y-AXIS DISTANCE) ---
        foreach (Collider col in enemiesInRange)
        {
            if (col == null) continue;

            Fireball fireball = col.GetComponent<Fireball>() ?? col.GetComponentInParent<Fireball>();
            if (fireball != null)
            {
                // Measure ground distance only (ignore height differences)
                Vector3 posDiff = col.transform.position - currentPosition;
                posDiff.y = 0f; 

                float distanceSqr = posDiff.sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    targetFireball = fireball;
                    targetCollider = col;
                }
            }
        }

        // --- STEP 2: IF NO FIREBALL FOUND, TARGET CLOSEST REGULAR ENEMY ---
        if (targetFireball == null)
        {
            foreach (Collider col in enemiesInRange)
            {
                if (col == null) continue;

                EnemyAI enemy = col.GetComponent<EnemyAI>() ?? col.GetComponentInParent<EnemyAI>();
                if (enemy != null)
                {
                    Vector3 posDiff = col.transform.position - currentPosition;
                    posDiff.y = 0f;

                    float distanceSqr = posDiff.sqrMagnitude;
                    if (distanceSqr < closestDistanceSqr)
                    {
                        closestDistanceSqr = distanceSqr;
                        targetCollider = col;
                    }
                }
            }
        }

        // --- STEP 3: DEAL DAMAGE TO TARGET ---
        if (targetCollider != null)
        {
            float damageToDeal = (gameManager != null) ? gameManager.playerDamage : 15f;

            Fireball fb = targetCollider.GetComponent<Fireball>() ?? targetCollider.GetComponentInParent<Fireball>();
            if (fb != null)
            {
                fb.TakeDamage(damageToDeal);
                Debug.Log($"[Auto-Attack] Prioritized & damaged floating Fireball: {fb.name} for {damageToDeal} damage!");
                return;
            }

            EnemyAI en = targetCollider.GetComponent<EnemyAI>() ?? targetCollider.GetComponentInParent<EnemyAI>();
            if (en != null)
            {
                en.TakeDamage(damageToDeal);
                Debug.Log($"[Auto-Attack] Attacked ground enemy: {en.name} for {damageToDeal} damage.");
            }
        }
    }

    // --- SPHERE COLLIDER ZONE DETECTION ---

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!enemiesInRange.Contains(other))
            {
                enemiesInRange.Add(other);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (enemiesInRange.Contains(other))
            {
                enemiesInRange.Remove(other);
            }
        }
    }
}