using System.Collections.Generic;
using UnityEngine;

public class PowerUpSelectionManager : MonoBehaviour
{
    [Header("Available Power-Up Prefabs")]
    [Tooltip("Drag all your power-up card prefabs here (Damage, Attack Speed, Regen, Defense, Luck, Health, etc.).")]
    [SerializeField] private List<GameObject> allPowerUpPrefabs = new List<GameObject>();

    [Header("Card Spawn Slots")]
    [Tooltip("Drag the 3 UI Transforms/Slots where the cards should appear on the panel.")]
    [SerializeField] private Transform[] cardSlots;

    private void OnEnable()
    {
        // 1. Freeze the game background when the panel appears
        Time.timeScale = 0f;

        // 2. Generate 3 random power-ups
        GenerateRandomPowerUps();
    }

private void OnDisable()
{
    // Unfreeze the game background when the panel closes (as long as the pause menu isn't active)
    if (Time.timeScale == 0f)
    {
        Time.timeScale = 1f;
    }
}

    public void GenerateRandomPowerUps()
    {
        if (allPowerUpPrefabs == null || allPowerUpPrefabs.Count == 0)
        {
            Debug.LogError("[PowerUpSelectionManager] No power-up prefabs assigned in the list!");
            return;
        }

        if (cardSlots == null || cardSlots.Length == 0)
        {
            Debug.LogError("[PowerUpSelectionManager] No card slots assigned!");
            return;
        }

        // 1. Clear any old cards currently sitting in the slots
        foreach (Transform slot in cardSlots)
        {
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }

        // 2. Create a temporary copy of the list so we can pick unique power-ups without repeating
        List<GameObject> availablePrefabs = new List<GameObject>(allPowerUpPrefabs);

        // 3. Loop through each slot and instantiate a random power-up
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (availablePrefabs.Count == 0) break; // Stop if we ran out of unique prefabs

            // Pick a random index from our available pool
            int randomIndex = Random.Range(0, availablePrefabs.Count);
            GameObject chosenPrefab = availablePrefabs[randomIndex];

            // Remove it from the pool so it doesn't repeat on another card slot
            availablePrefabs.RemoveAt(randomIndex);

            // Instantiate the card as a child of the designated slot transform
            GameObject spawnedCard = Instantiate(chosenPrefab, cardSlots[i]);

            // Reset its local rect transform positions so it snaps nicely into the slot layout
            RectTransform rectTransform = spawnedCard.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = Vector3.one;
                rectTransform.localRotation = Quaternion.identity;
            }
        }
    }
}