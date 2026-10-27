using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleUI : MonoBehaviour
{
    public Button playButton;
    public Button settingsButton;
    
    [Header("Buttons")]
    [SerializeField] private Button closeSettingsButton; // Added SerializeField for the close button

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    void Start()
    {
        ShowMainPanel();

        if (playButton != null && SceneController.Instance != null)
        {
            // Triggers a fresh run with a randomized starting room
            playButton.onClick.AddListener(() => SceneController.Instance.StartNewRun());
        }

        // Hooks up the settings button when the game starts
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(() => ShowSettings());
        }

        // Hooks up the close/back button when the game starts
        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.AddListener(() => BackToMain());
        }
    }

    public void OnPlayButton()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.StartNewRun();
        }
    }

    public void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void BackToMain()
    {
        ShowMainPanel();
    }

    public void OnQuitButton()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}