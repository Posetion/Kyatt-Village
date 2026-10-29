using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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

    private readonly HashSet<Collider> playerCollidersInZone = new HashSet<Collider>();
    private bool isPlayerInZone;

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
        if (!isPlayerInZone) return;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleShop();
            return;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            UnityEngine.InputSystem.Controls.TouchControl touch = Touchscreen.current.primaryTouch;
            int touchId = (int)touch.touchId.ReadValue();
            if (!IsPointerOverUI(touchId) && IsTapOnKeeper(touch.position.ReadValue()))
            {
                ToggleShop();
            }

            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
            !IsPointerOverUI(-1) && IsTapOnKeeper(Mouse.current.position.ReadValue()))
        {
            ToggleShop();
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleShop();
            return;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            UnityEngine.Touch touch = Input.GetTouch(0);
            if (!IsPointerOverUI(touch.fingerId) && IsTapOnKeeper(touch.position))
            {
                ToggleShop();
            }
        }
        else if (Input.GetMouseButtonDown(0) && !IsPointerOverUI(-1) && IsTapOnKeeper(Input.mousePosition))
        {
            ToggleShop();
        }
#endif
    }

    private bool IsPointerOverUI(int pointerId)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
    }

    private bool IsTapOnKeeper(Vector2 screenPosition)
    {
        Camera mainCamera = Camera.main;
        SpriteRenderer keeperRenderer = GetComponentInChildren<SpriteRenderer>();
        if (mainCamera == null || keeperRenderer == null) return false;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Plane keeperPlane = new Plane(mainCamera.transform.forward, keeperRenderer.bounds.center);
        if (!keeperPlane.Raycast(ray, out float distance)) return false;

        return keeperRenderer.bounds.Contains(ray.GetPoint(distance));
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
            SetShopOpen(!shopPanel.activeSelf);
        }
        else
        {
            Debug.LogError("ShopPanel reference is missing on the Shopkeeper script! Please drag your Shop UI Panel into the Shopkeeper Inspector slot.");
        }
    }

    private void SetShopOpen(bool open)
    {
        if (shopPanel == null)
        {
            InitializeReferences();
        }

        if (shopPanel == null || shopPanel.activeSelf == open) return;

        shopPanel.SetActive(open);
        if (!open) return;

        if (gameManager != null)
        {
            Debug.Log($"Shop opened! Current Player Coins: {gameManager.totalCoins}");
        }
        else
        {
            Debug.LogWarning("Shop opened, but GameManager reference is missing!");
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
        if (!other.CompareTag("Player") || !playerCollidersInZone.Add(other)) return;

        bool firstEntry = playerCollidersInZone.Count == 1;
        isPlayerInZone = true;

        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(true);
        }

        if (firstEntry) SetShopOpen(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || !playerCollidersInZone.Remove(other)) return;
        if (playerCollidersInZone.Count > 0) return;

        isPlayerInZone = false;

        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }

        SetShopOpen(false);
    }
}