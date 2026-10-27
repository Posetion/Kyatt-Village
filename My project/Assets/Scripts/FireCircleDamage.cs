using UnityEngine;

public class FireCircleDamage : MonoBehaviour
{
    [Header("Fire Circle Attack")]
    public float damageAmount = 15f;
    public float attackCooldown = 1f; // Same cooldown style as EnemyAI

    private float lastAttackTime = -999f;
    private bool touchingPlayer;
    private Transform playerTransform;

    void Update()
    {
        // If the player is standing inside, continuously attempt to deal damage based on cooldown
        if (touchingPlayer)
        {
            TryDamagePlayer();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            touchingPlayer = true;
            playerTransform = other.transform;
            TryDamagePlayer();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            touchingPlayer = true;
            playerTransform = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            touchingPlayer = false;
            playerTransform = null;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            touchingPlayer = true;
            playerTransform = collision.transform;
            TryDamagePlayer();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            touchingPlayer = true;
            playerTransform = collision.transform;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            touchingPlayer = false;
            playerTransform = null;
        }
    }

    void TryDamagePlayer()
    {
        if (!touchingPlayer || Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;

        if (playerTransform != null)
        {
            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = playerTransform.GetComponentInParent<PlayerHealth>();
            }

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log("Player touched fire circle and took damage!");
            }
        }
    }
}