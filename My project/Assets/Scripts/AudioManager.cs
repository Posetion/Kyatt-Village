using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicAudioSource;
    private AudioSource sfxAudioSource; 
    [SerializeField] private AudioSource natureAudioSource;

    [Header("Scene Music Clips (Drag in Inspector)")]
    [Tooltip("Music that plays on your Title/Menu scene.")]
    [SerializeField] private AudioClip titleMusicClip;    

    [Tooltip("Music that plays during standard gameplay/levels.")]
    [SerializeField] private AudioClip startMusicClip;    

    [Tooltip("Music that plays in the shop. Leave empty (None) to turn background music completely OFF in the shop.")]
    [SerializeField] private AudioClip shopMusicClip;     

    [Header("Optional Starting Clips")]
    [SerializeField] private AudioClip startNatureClip;
    
    [Header("Scene Name Settings")]
    [Tooltip("Must match your exact Title/Menu scene name.")]
    [SerializeField] private string titleSceneName = "Title"; 
    
    [Tooltip("Must match your exact Shop scene name.")]
    [SerializeField] private string shopSceneName = "ShopScene";   

    [Header("Volume Defaults")]
    [Range(0f, 1f)] [SerializeField] private float defaultMusicVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float defaultSfxVolume = 1.0f;

    private float musicVolume;
    private float sfxVolume;
    private bool musicMuted = false;
    private bool sfxMuted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetupAudioSources();
        LoadAudioSettings();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        SetupAudioSources();
        HandleSceneMusic(SceneManager.GetActiveScene().name);

        if (startNatureClip != null && natureAudioSource != null && !natureAudioSource.isPlaying)
        {
            natureAudioSource.clip = startNatureClip;
            natureAudioSource.loop = true;
            natureAudioSource.volume = sfxMuted ? 0f : sfxVolume;
            natureAudioSource.Play();
        }

        ApplyAudioSettings();
    }

    private void SetupAudioSources()
    {
        if (musicAudioSource == null)
        {
            musicAudioSource = gameObject.AddComponent<AudioSource>();
        }
        musicAudioSource.enabled = true;

        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
            if (sfxAudioSource == null || sfxAudioSource == musicAudioSource)
            {
                sfxAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        sfxAudioSource.enabled = true;
        sfxAudioSource.playOnAwake = false;

        if (natureAudioSource == null)
        {
            natureAudioSource = gameObject.AddComponent<AudioSource>();
        }
        natureAudioSource.enabled = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupAudioSources();
        HandleSceneMusic(scene.name);
    }

    private void HandleSceneMusic(string sceneName)
    {
        if (sceneName == titleSceneName)
        {
            if (titleMusicClip != null)
            {
                PlayMusic(titleMusicClip);
            }
            else
            {
                StopMusic();
            }
        }
        else if (sceneName == shopSceneName)
        {
            if (shopMusicClip != null)
            {
                PlayMusic(shopMusicClip);
            }
            else
            {
                StopMusic(); 
            }
        }
        else
        {
            if (startMusicClip != null)
            {
                PlayMusic(startMusicClip);
            }
        }
    }

    public void LoadAudioSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        
        if (musicVolume <= 0.001f && PlayerPrefs.GetInt("MusicMuted", 0) == 0)
        {
            musicVolume = defaultMusicVolume;
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        }

        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", defaultSfxVolume);
        if (sfxVolume <= 0.001f && PlayerPrefs.GetInt("SFXMuted", 0) == 0)
        {
            sfxVolume = defaultSfxVolume;
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        }
        
        musicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        sfxMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
    }

    private void ApplyAudioSettings()
    {
        SetupAudioSources();

        if (musicAudioSource != null)
            musicAudioSource.volume = musicMuted ? 0f : musicVolume;

        if (sfxAudioSource != null)
            sfxAudioSource.volume = sfxMuted ? 0f : sfxVolume;

        if (natureAudioSource != null)
            natureAudioSource.volume = sfxMuted ? 0f : sfxVolume;
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
    {
        SetupAudioSources();

        if (sfxAudioSource != null && clip != null && !sfxMuted)
        {
            if (!sfxAudioSource.enabled)
                sfxAudioSource.enabled = true;

            sfxAudioSource.PlayOneShot(clip, sfxVolume * volumeScale);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        SetupAudioSources();

        if (musicAudioSource == null || clip == null) return;

        if (!musicAudioSource.enabled)
            musicAudioSource.enabled = true;

        if (musicAudioSource.clip == clip && musicAudioSource.isPlaying)
            return;

        musicAudioSource.clip = clip;
        musicAudioSource.loop = true;
        musicAudioSource.volume = musicMuted ? 0f : musicVolume;
        musicAudioSource.Play();
    }

    public void StopMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
            musicAudioSource.clip = null;
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicAudioSource != null)
        {
            musicAudioSource.enabled = true;
            musicAudioSource.volume = musicMuted ? 0f : musicVolume;
        }
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        
        if (sfxAudioSource != null)
        {
            sfxAudioSource.enabled = true;
            sfxAudioSource.volume = sfxMuted ? 0f : sfxVolume;
        }

        if (natureAudioSource != null)
        {
            natureAudioSource.enabled = true;
            natureAudioSource.volume = sfxMuted ? 0f : sfxVolume;
        }

        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    public void ToggleMusicMute()
    {
        musicMuted = !musicMuted;
        if (musicAudioSource != null)
        {
            musicAudioSource.enabled = true;
            musicAudioSource.volume = musicMuted ? 0f : musicVolume;
        }
        PlayerPrefs.SetInt("MusicMuted", musicMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleSfxMute()
    {
        sfxMuted = !sfxMuted;
        
        if (sfxAudioSource != null)
        {
            sfxAudioSource.enabled = true;
            sfxAudioSource.volume = sfxMuted ? 0f : sfxVolume; 
        }

        if (natureAudioSource != null)
        {
            natureAudioSource.enabled = true;
            natureAudioSource.volume = sfxMuted ? 0f : sfxVolume;
        }

        PlayerPrefs.SetInt("SFXMuted", sfxMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    public bool IsMusicMuted() => musicMuted;
    public bool IsSFXMuted() => sfxMuted;
}