using UnityEngine;

public class AttackSpeedPowerUp : BasePowerUp
{
    [Header("Buff Settings")]
    [Tooltip("Percentage increase for player attack speed (e.g., 5 for 5%).")]
    [SerializeField] private float attackSpeedIncreasePercent = 5f;

    protected override void ApplyBuff()
    {
        // Apply the Stat Buff via GameManager so it carries over maps
        if (GameManager.Instance != null)
        {
            float multiplier = 1f + (attackSpeedIncreasePercent / 100f);
            GameManager.Instance.playerAttackSpeed *= multiplier;

            Debug.Log($"Attack speed increased by {attackSpeedIncreasePercent}%! New Attack Speed: {GameManager.Instance.playerAttackSpeed}");
        }
        else
        {
            Debug.LogError("GameManager instance not found! Cannot apply attack speed buff.");
        }
    }
}