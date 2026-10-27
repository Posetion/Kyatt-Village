using UnityEngine;

public class LuckPowerUp : BasePowerUp
{
    [Header("Buff Settings")]
    [Tooltip("Flat amount to increase player luck by (e.g., 5 for +5 luck).")]
    [SerializeField] private float luckIncreaseAmount = 5f;

    protected override void ApplyBuff()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerLuck += luckIncreaseAmount;
            Debug.Log($"Luck increased by +{luckIncreaseAmount}! New Luck: {GameManager.Instance.playerLuck}");
        }
        else
        {
            Debug.LogError("GameManager instance not found! Cannot apply luck buff.");
        }
    }
}