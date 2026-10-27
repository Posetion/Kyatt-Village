using TMPro;
using UnityEngine;

public class LosePanelController : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text finalTimeText;
    [SerializeField] private TMP_Text bestRecordText;

    private void Awake()
    {
        // Ensure the panel starts disabled when the scene loads
        gameObject.SetActive(false);
    }

    // Call this method to show the panel and update the text automatically
    public void ShowLosePanel()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling(); // Bring to the absolute front of the canvas

        if (GameManager.Instance != null)
        {
            if (finalTimeText != null)
            {
                finalTimeText.text = $"Time: {GameManager.Instance.GetFormattedCurrentTime()}";
            }

            if (bestRecordText != null)
            {
                bestRecordText.text = $"Best Record: {GameManager.Instance.GetFormattedBestTime()}";
            }
        }
        else
        {
            Debug.LogWarning("[LosePanelController] GameManager.Instance is null!");
        }
    }

    public void HideLosePanel()
    {
        gameObject.SetActive(false);
    }
}