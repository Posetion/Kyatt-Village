using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Available Power-Up Prefabs")]
    [Tooltip("Drag all your power-up card prefabs here (Damage, Attack Speed, Regen, Defense, Luck, Health, etc.).")]
    [SerializeField] private List<GameObject> allPowerUpPrefabs = new List<GameObject>();

    [Header("Card Spawn Slots")]
    [Tooltip("Drag the UI Transforms/Slots where the cards should appear in the shop panel.")]
    [SerializeField] private Transform[] cardSlots;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI shopPlayerCoinsText; // To display current player coins inside the shop UI

    [Header("Refresh Settings")]
    [Tooltip("Button used to refresh/re-roll the shop stock.")]
    [SerializeField] private Button refreshButton;
    [Tooltip("(Optional) Text on the refresh button to show the changing cost.")]
    [SerializeField] private TextMeshProUGUI refreshButtonText;

    private int currentRefreshCost = 75; // Set starting/fixed refresh cost to 75
    private const int baseRefreshCost = 75;
    private const int costIncrement = 0; // Set to 0 if you want it to always remain 75 coins. Change back if you want it to scale.

    private void Start()
    {
        // Initial generation when game starts if needed
        currentRefreshCost = baseRefreshCost;
        GenerateShopStock();
    }

    private void OnEnable()
    {
        // Freeze game background when shopping
        Time.timeScale = 0f;

        // FIXED: Every time you open the shop (or re-enter the shop map/scene), 
        // it will completely refresh the stock so old or bought items get replaced!
        currentRefreshCost = baseRefreshCost;
        GenerateShopStock();
    }

    private void OnDisable()
    {
        // Unfreeze game when closing the shop panel
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }

    // Call this method from your Shopkeeper NPC interaction script to open the shop!
    public void OpenShop()
    {
        gameObject.SetActive(true);
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
    }

    // Hook this up to your Close Button's OnClick() event in the Inspector
    public void CloseShopButton()
    {
        CloseShop();
    }

    // Hook this up to your Refresh Button's OnClick() event in the Inspector
    public void RefreshShop()
    {
        if (GameManager.Instance == null) return;

        // Check if player has enough coins to refresh
        if (GameManager.Instance.totalCoins >= currentRefreshCost)
        {
            // Deduct coins
            GameManager.Instance.totalCoins -= currentRefreshCost;

            // Increase the cost for the next refresh (if costIncrement > 0)
            currentRefreshCost += costIncrement;

            // Generate new stock and update cards
            GenerateShopStock();
        }
        else
        {
            Debug.Log("[ShopManager] Not enough coins to refresh the shop!");
        }
    }

    public void GenerateShopStock()
    {
        if (allPowerUpPrefabs == null || allPowerUpPrefabs.Count == 0)
        {
            Debug.LogError("[ShopManager] No power-up prefabs assigned in the shop list!");
            return;
        }

        if (cardSlots == null || cardSlots.Length == 0)
        {
            Debug.LogError("[ShopManager] No card slots assigned in the shop!");
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

        // 2. Create a temporary copy so we can pick unique power-ups without repeating
        List<GameObject> availablePrefabs = new List<GameObject>(allPowerUpPrefabs);

        // 3. Loop through each slot and instantiate a random power-up card
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (availablePrefabs.Count == 0) break; // Stop if we ran out of unique prefabs

            int randomIndex = Random.Range(0, availablePrefabs.Count);
            GameObject chosenPrefab = availablePrefabs[randomIndex];
            availablePrefabs.RemoveAt(randomIndex);

            // Instantiate the card as a child of the designated slot transform
            GameObject spawnedCard = Instantiate(chosenPrefab, cardSlots[i]);

            // Reset its local rect transform positions so it snaps nicely into layout
            RectTransform rectTransform = spawnedCard.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = Vector3.one;
                rectTransform.localRotation = Quaternion.identity;
            }

            // 4. GET COMPONENTS FOR COST AND CARD BUTTON INTERACTIVITY
            BasePowerUp powerUpComponent = spawnedCard.GetComponent<BasePowerUp>();
            Button cardButton = spawnedCard.GetComponentInChildren<Button>();

            if (powerUpComponent != null)
            {
                powerUpComponent.SetCostText(powerUpComponent.PowerUpCost);

                if (cardButton != null && GameManager.Instance != null)
                {
                    cardButton.interactable = (GameManager.Instance.totalCoins >= powerUpComponent.PowerUpCost);
                }
            }
        }

        // Update UI states after generating stock
        UpdateShopUI();
    }

    public void UpdateShopUI()
    {
        if (GameManager.Instance == null) return;

        // Update player coin display text
        if (shopPlayerCoinsText != null)
        {
            shopPlayerCoinsText.text = "Coins: " + GameManager.Instance.totalCoins;
        }

        // Update Refresh Button state and text
        if (refreshButton != null)
        {
            refreshButton.interactable = (GameManager.Instance.totalCoins >= currentRefreshCost);
        }

        if (refreshButtonText != null)
        {
            refreshButtonText.text = $"Refresh ({currentRefreshCost} Coins)";
        }
    }
}