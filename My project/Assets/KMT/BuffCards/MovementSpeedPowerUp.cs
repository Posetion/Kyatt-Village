using UnityEngine;

public class MovementSpeedPowerUp : BasePowerUp
{
    [Header("Buff Settings")]
    [Tooltip("Percentage increase for player movement speed (e.g., 10 for 10%).")]
    [SerializeField] private float movementSpeedIncreasePercent = 10f;

    protected override void ApplyBuff()
    {
        // Apply the Stat Buff via GameManager so it carries over maps
        if (GameManager.Instance != null)
        {
            float multiplier = 1f + (movementSpeedIncreasePercent / 100f);
            GameManager.Instance.playerMovementSpeed *= multiplier;

            Debug.Log($"Movement speed increased by {movementSpeedIncreasePercent}%! New Movement Speed: {GameManager.Instance.playerMovementSpeed}");
        }
        else
        {
            Debug.LogError("GameManager instance not found! Cannot apply movement speed buff.");
        }
    }
}