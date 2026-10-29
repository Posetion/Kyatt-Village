using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingUI : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle musicMuteToggle;
    [SerializeField] private Toggle sfxMuteToggle;

    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown qualityDropdown;

    private void Start()
    {
        LoadSettings();
        SetupListeners();
    }

    private void LoadSettings()
    {
        if (AudioManager.Instance != null)
        {
            // 1. Initialize Music Slider
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.maxValue = 1.0f;
                musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
            }

            // 2. Initialize SFX Slider
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.maxValue = 1.0f;
                sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
            }

            // 3. Initialize Music Mute Toggle
            if (musicMuteToggle != null)
            {
                musicMuteToggle.isOn = AudioManager.Instance.IsMusicMuted();
            }

            // 4. Initialize SFX Mute Toggle
            if (sfxMuteToggle != null)
            {
                sfxMuteToggle.isOn = AudioManager.Instance.IsSFXMuted();
            }
        }

        // Initialize Quality Dropdown
        if (qualityDropdown != null)
        {
            int savedQuality = PlayerPrefs.GetInt("GraphicsQuality", 2);
            qualityDropdown.value = savedQuality;
            QualitySettings.SetQualityLevel(savedQuality);
        }
    }

    private void SetupListeners()
    {
        // Remove old listeners to prevent duplicates, then add them fresh
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (musicMuteToggle != null)
        {
            musicMuteToggle.onValueChanged.RemoveAllListeners();
            musicMuteToggle.onValueChanged.AddListener(OnMusicMuteToggled);
        }

        if (sfxMuteToggle != null)
        {
            sfxMuteToggle.onValueChanged.RemoveAllListeners();
            sfxMuteToggle.onValueChanged.AddListener(OnSFXMuteToggled);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.RemoveAllListeners();
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance?.SetSfxVolume(value);
    }

    private void OnMusicMuteToggled(bool isMuted)
    {
        AudioManager.Instance?.ToggleMusicMute();
    }

    private void OnSFXMuteToggled(bool isMuted)
    {
        AudioManager.Instance?.ToggleSfxMute();
    }

    private void OnQualityChanged(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("GraphicsQuality", qualityIndex);
        PlayerPrefs.Save();
    }

    public void OnResetGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetAllProgress();
        }

        for (int i = 1; i <= 3; i++)
        {
            string levelKey = $"Level{i}Complete";
            if (PlayerPrefs.HasKey(levelKey)) PlayerPrefs.DeleteKey(levelKey);
        }

        PlayerPrefs.Save();
        SceneManager.LoadScene("Title");
    }
}