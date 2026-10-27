using UnityEngine;

public class DamagePowerUp : BasePowerUp
{
    [Header("Buff Settings")]
    [Tooltip("Percentage increase for player damage (e.g., 5 for 5%).")]
    [SerializeField] private float damageIncreasePercent = 5f;

    protected override void ApplyBuff()
    {
        if (GameManager.Instance != null)
        {
            // Increase player damage by the specified percentage
            float multiplier = 1f + (damageIncreasePercent / 100f);
            GameManager.Instance.playerDamage *= multiplier;

            Debug.Log($"Damage increased by {damageIncreasePercent}%! New Damage: {GameManager.Instance.playerDamage}");
        }
        else
        {
            Debug.LogError("GameManager instance not found! Cannot apply damage buff.");
        }
    }
}