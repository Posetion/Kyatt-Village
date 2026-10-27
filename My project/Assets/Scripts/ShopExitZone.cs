using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopExitZone : MonoBehaviour
{
    [Header("Confirmation UI Panel")]
    [Tooltip("Drag the UI panel that asks 'Are you sure you want to leave?' here.")]
    [SerializeField] private GameObject confirmLeavePanel;

    private void Start()
    {
        // Ensure the confirmation panel is hidden when the shop starts
        if (confirmLeavePanel != null)
        {
            confirmLeavePanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the zone
        if (!other.CompareTag("Player"))
            return;

        // Show the confirmation panel
        if (confirmLeavePanel != null)
        {
            confirmLeavePanel.SetActive(true);
            
            // Pause the game while the confirmation panel is open
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogWarning("[ShopExitZone] Confirmation panel is not assigned! Leaving immediately.");
            ProceedToNextLevel();
        }
    }

    // Hook this up to your confirmation panel's "YES" button OnClick() event in the Inspector
    public void OnConfirmYes()
    {
        // Explicitly hide the panel first so it disappears immediately
        if (confirmLeavePanel != null)
        {
            confirmLeavePanel.SetActive(false);
        }

        // Unpause the game time before changing scenes
        Time.timeScale = 1f;

        ProceedToNextLevel();
    }

    // Hook this up to your confirmation panel's "NO" button OnClick() event in the Inspector
    public void OnConfirmNo()
    {
        // Hide the panel and unpause the game
        if (confirmLeavePanel != null)
        {
            confirmLeavePanel.SetActive(false);
        }
        
        Time.timeScale = 1f;
    }

    private void ProceedToNextLevel()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadNextRoom();
        }
        else
        {
            Debug.LogError("[ShopExitZone] SceneController.Instance is missing!");
        }
    }
}