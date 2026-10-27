using UnityEngine;

public class Coin : MonoBehaviour
{
    private Animator animator;
    private Collider coinCollider;
    
    [SerializeField] private int coinValue = 1; 
    
    [Header("Audio")]
    [SerializeField] private AudioClip coinPickupSound; 
    [Range(0f, 1f)] [SerializeField] private float coinPickupVolume = 1.0f; 

    void Start()
    {
        animator = GetComponent<Animator>();
        coinCollider = GetComponent<Collider>();

        // DEBUG CHECK: Helps find if the clip is missing in Map1
        if (coinPickupSound == null)
        {
            Debug.LogWarning("[Coin] Missing 'coinPickupSound' on GameObject: " + gameObject.name, this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[Coin] Collected by player in scene: " + gameObject.scene.name);

            // --- PLAY COIN PICKUP SOUND VIA AUDIO MANAGER ---
            if (AudioManager.Instance != null)
            {
                if (coinPickupSound != null)
                {
                    AudioManager.Instance.PlaySFX(coinPickupSound, coinPickupVolume);
                    Debug.Log("[Coin] Successfully triggered PlaySFX.");
                }
                else
                {
                    Debug.LogError("[Coin] Failed to play sound: coinPickupSound is NULL on " + gameObject.name, this);
                }
            }
            else
            {
                Debug.LogError("[Coin] Failed to play sound: AudioManager.Instance is NULL!", this);
            }

            // --- ADD COINS TO GAME MANAGER ---
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(coinValue);
            }
            else
            {
                Debug.LogWarning("[Coin] GameManager instance not found!");
            }

            // Trigger the collection animation
            if (animator != null)
            {
                animator.SetTrigger("Collect");
            }

            // Disable the collider so it can't be collected twice
            if (coinCollider != null)
            {
                coinCollider.enabled = false;
            }

            // Stop movement physics when collected
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            // Destroy the GameObject after the collection animation finishes 
            Destroy(gameObject, 0.4f);
        }
    }
}