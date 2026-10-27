using UnityEngine;

public class DefensePowerUp : BasePowerUp
{
    [Header("Buff Settings")]
    [Tooltip("Flat amount to increase player defense by (e.g., 5 for +5 defense).")]
    [SerializeField] private float defenseIncreaseAmount = 5f;

    protected override void ApplyBuff()
    {
        if (GameManager.Instance != null)
        {
            // Increase player defense by the specified flat amount
            GameManager.Instance.playerDefense += defenseIncreaseAmount;

            Debug.Log($"Defense increased by +{defenseIncreaseAmount}! New Defense: {GameManager.Instance.playerDefense}");
        }
        else
        {
            Debug.LogError("GameManager instance not found! Cannot apply defense buff.");
        }
    }
}