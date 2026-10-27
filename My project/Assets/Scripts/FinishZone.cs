using UnityEngine;

public class FinishZone : MonoBehaviour
{
    [Header("Lock Settings")]
    public bool isLocked = true;

    [Header("Gate")]
    public GameObject gateVisual;
    public GameObject gateBlocker;

    [Header("Gate Opening VFX")]
    public GameObject openVFX;
    public Transform vfxSpawnPoint;
    public float vfxLifetime = 3f;

    [Header("Warning Message")]
    public GameObject warningMessageUI;
    public float warningMessageDuration = 2f;

    private bool playerEntered = false;

    private void Start()
    {
        if (!CompareTag("ExitZone"))
        {
            gameObject.tag = "ExitZone";
        }

        if (gateVisual != null)
            gateVisual.SetActive(true);

        if (gateBlocker != null)
            gateBlocker.SetActive(isLocked);

        if (warningMessageUI != null)
            warningMessageUI.SetActive(false);
    }

    public void UnlockZone()
    {
        if (!isLocked) return;

        isLocked = false;

        if (openVFX != null)
        {
            Vector3 spawnPosition = vfxSpawnPoint != null
                ? vfxSpawnPoint.position
                : transform.position;

            GameObject effect = Instantiate(
                openVFX,
                spawnPosition,
                Quaternion.identity
            );

            Destroy(effect, vfxLifetime);
        }

        if (gateVisual != null)
            gateVisual.SetActive(false);

        if (gateBlocker != null)
            gateBlocker.SetActive(false);

        Debug.Log("Finish gate opened!");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug log to see what entered the zone
        Debug.Log("Trigger entered by: " + other.gameObject.name + " (Tag: " + other.tag + ")");

        if (!other.CompareTag("Player"))
        {
            Debug.LogWarning("Object is not tagged as 'Player'!");
            return;
        }

        if (isLocked)
        {
            Debug.Log("Player entered, but the zone is STILL LOCKED.");
            ShowLockedMessage();
            return;
        }

        if (!playerEntered)
        {
            playerEntered = true;
            Debug.Log("Zone is unlocked! Attempting to load next room via SceneController...");

            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadNextRoom();
            }
            else
            {
                Debug.LogError("SceneController.Instance is missing in the scene!");
            }
        }
    }

    private void ShowLockedMessage()
    {
        if (warningMessageUI == null) return;

        warningMessageUI.SetActive(true);

        CancelInvoke(nameof(HideLockedMessage));
        Invoke(nameof(HideLockedMessage), warningMessageDuration);
    }

    private void HideLockedMessage()
    {
        if (warningMessageUI != null)
            warningMessageUI.SetActive(false);
    }
}