using UnityEngine;

public class RegenPowerUp : BasePowerUp
{
    [Header("Buff Settings")]
    [Tooltip("Amount of health restored per second (e.g., 5 for 5 HP per second).")]
    [SerializeField] private float regenIncreaseAmount = 5f;

    protected override void ApplyBuff()
    {
        // Directly update the GameManager if available so it persists across maps
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerRegenAmount += regenIncreaseAmount;
            Debug.Log($"Regeneration increased by {regenIncreaseAmount}! Current Regen: {GameManager.Instance.playerRegenAmount} HP/sec");
            
            // Also update the active player if they are currently in the scene
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                // This triggers any local UI or updates if necessary
                playerHealth.AddRegeneration(0f); // Just refreshes/syncs if needed
            }
        }
        else
        {
            Debug.LogError("GameManager instance not found! Cannot apply regen buff across maps.");
        }
    }
}