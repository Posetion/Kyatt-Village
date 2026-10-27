using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class BasePowerUp : MonoBehaviour
{
    [Header("Icon Settings")]
    [Tooltip("The icon sprite representing this power-up.")]
    [SerializeField] public Sprite powerUpIcon;

    [Header("Shop & Cost Settings")]
    [Tooltip("The coin cost to purchase this power-up.")]
    [SerializeField] protected int powerUpCost = 75;

    [Header("UI Display")]
    [SerializeField] private TextMeshProUGUI costTextUI;

    public void SetCostText(int cost)
    {
        if (costTextUI != null) costTextUI.text = cost.ToString() + " Coins";
    }

    public int PowerUpCost => powerUpCost;

    protected Button button;

    protected virtual void Awake()
    {
        if (GetType().Name.Contains("Regen") || GetType().Name.Contains("Regeneration"))
        {
            powerUpCost = 100;
        }

        button = GetComponent<Button>();
        
    }

    protected virtual void Start()
    {
        ShopManager shopManager = GetComponentInParent<ShopManager>();
        
        if (costTextUI != null)
        {
            if (shopManager != null)
            {
                costTextUI.gameObject.SetActive(true);
                SetCostText(powerUpCost);
            }
            else
            {
                costTextUI.gameObject.SetActive(false);
            }
        }
    }

    public void OnPowerUpClicked()
    {
        ShopManager shopManager = GetComponentInParent<ShopManager>();

        if (shopManager != null)
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.totalCoins < powerUpCost) return;

                GameManager.Instance.totalCoins -= powerUpCost;

                if (UiManager.instance != null)
                {
                    UiManager.instance.UpdateMoney(GameManager.Instance.totalCoins);
                }

                shopManager.UpdateShopUI();
            }

            ApplyBuffAndSaveIcon();
            Destroy(gameObject);
        }
        else
        {
            ApplyBuffAndSaveIcon();

            PowerUpSelectionManager selectionManager = GetComponentInParent<PowerUpSelectionManager>();
            if (selectionManager != null)
            {
                selectionManager.gameObject.SetActive(false);
            }
            else
            {
                GameObject powerUpPanel = GameObject.Find("PowerUpChoose");
                if (powerUpPanel != null) powerUpPanel.SetActive(false);
            }

            Time.timeScale = 1f;
        }
    }
private void ApplyBuffAndSaveIcon()
{
    ApplyBuff();

    if (GameManager.Instance != null && powerUpIcon != null)
    {
        Debug.Log("Saving power-up icon: " + powerUpIcon.name); // <-- Add this line
        GameManager.Instance.AddCollectedPowerUpIcon(powerUpIcon);
    }
    else
    {
        Debug.LogWarning("Could not save power-up icon! GameManager is null or powerUpIcon is missing."); // <-- And this line
    }
}

    protected abstract void ApplyBuff();
}