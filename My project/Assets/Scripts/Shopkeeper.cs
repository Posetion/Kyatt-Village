using UnityEngine;

public class Shopkeeper : MonoBehaviour
{
    [Header("Managers References")]
    [Tooltip("Drag your GameManager here (or leave blank to auto-find GameManager.Instance).")]
    [SerializeField] private GameManager gameManager;

    [Tooltip("Drag your UiManager here (or leave blank to auto-find UiManager.instance).")]
    [SerializeField] private UiManager uiManager;

    [Header("UI Settings")]
    [Tooltip("Drag your Shop UI Panel here. If left blank, it will auto-find it via ShopManager.")]
    [SerializeField] private GameObject shopPanel;

    [Tooltip("(Optional) Drag a prompt UI like 'Press E to Talk' or mobile touch icon here.")]
    [SerializeField] private GameObject interactionPromptUI;

    private bool isPlayerInZone = false;

    private void Start()
    {
        InitializeReferences();
    }

    // Call this whenever the scene loads or if references are missing
    private void InitializeReferences()
    {
        // Auto-assign GameManager if missing
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        // Auto-assign UiManager if missing
        if (uiManager == null)
        {
            uiManager = UiManager.instance;
        }

        // Safety check: If shopPanel reference is missing, find it automatically by searching for the ShopManager component in the scene!
        if (shopPanel == null)
        {
            ShopManager foundShopManager = FindFirstObjectByType<ShopManager>(FindObjectsInactive.Include);
            if (foundShopManager != null)
            {
                shopPanel = foundShopManager.gameObject;
                Debug.Log("[Shopkeeper] Automatically found and linked ShopPanel via ShopManager component.");
            }
        }

        // Fallback: If still null, search by exact name "ShopPanel"
        if (shopPanel == null)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "ShopPanel" && obj.scene.isLoaded)
                {
                    shopPanel = obj;
                    break;
                }
            }
        }

        // Ensure shop panel is hidden at start of the scene
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        // Hide the prompt initially
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }

    private void Update()
    {
        // If the player is in the zone and presses 'E' on PC, toggle the shop
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E))
        {
            ToggleShop();
        }
    }

    // Call this method from a mobile UI Button's OnClick() event
    public void OnMobileInteractButtonPressed()
    {
        if (isPlayerInZone)
        {
            ToggleShop();
        }
    }

    private void ToggleShop()
    {
        // Re-verify references dynamically just in case they were lost
        if (shopPanel == null)
        {
            InitializeReferences();
        }

        if (shopPanel != null)
        {
            bool isActive = shopPanel.activeSelf;
            shopPanel.SetActive(!isActive);

            // Optional: Print current coins to verify it's listening to GameManager
            if (gameManager != null)
            {
                Debug.Log($"Shop opened! Current Player Coins: {gameManager.totalCoins}");
            }
            else
            {
                Debug.LogWarning("Shop opened, but GameManager reference is missing!");
            }
        }
        else
        {
            Debug.LogError("ShopPanel reference is missing on the Shopkeeper script! Please drag your Shop UI Panel into the Shopkeeper Inspector slot.");
        }
    }

    // Example shop purchase function you can call from items inside your shop panel
    public bool TryBuyItem(int itemCost)
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        if (gameManager != null)
        {
            if (gameManager.totalCoins >= itemCost)
            {
                gameManager.AddCoins(-itemCost);
                Debug.Log($"Item bought successfully! Remaining coins: {gameManager.totalCoins}");
                return true;
            }
            else
            {
                Debug.Log("Not enough coins to buy this item!");
                return false;
            }
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;

            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;

            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(false);
            }

            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }
        }
    }
}