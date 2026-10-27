using UnityEngine;

public class HealthPowerUp : BasePowerUp
{
    [Header("Buff Settings")]
    [Tooltip("Percentage increase for player max health (e.g., 5 for 5%).")]
    [SerializeField] private float healthIncreasePercent = 5f;

    protected override void ApplyBuff()
    {
        if (GameManager.Instance != null)
        {
            // 1. Calculate health bonus
            float bonusHealth = GameManager.Instance.playerMaxHealth * (healthIncreasePercent / 100f);
            
            // 2. Increase max health and heal the player by the bonus amount in GameManager
            GameManager.Instance.playerMaxHealth += bonusHealth;
            GameManager.Instance.playerCurrentHealth += bonusHealth;

            Debug.Log($"Max Health increased by {healthIncreasePercent}%! New Max Health: {GameManager.Instance.playerMaxHealth}");

            // 3. If PlayerHealth exists in current scene, trigger UI update
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                // Calling IncreaseMaxHealth with 0% triggers health sync and UI update without doubling stats
                playerHealth.IncreaseMaxHealth(0f, healToFull: false);
            }
        }
        else
        {
            Debug.LogError("GameManager instance not found! Cannot apply health buff.");
        }
    }
}