using UnityEngine;
using TMPro;

public class GraphicsSettingsManager : MonoBehaviour
{
    [Header("Graphics Dropdown")]
    [SerializeField] private TMP_Dropdown graphicsDropdown;

    [Header("Auto-Balancing Settings")]
    [Tooltip("If enabled, automatically lowers graphics if the mobile browser starts lagging.")]
    [SerializeField] private bool enableAutoBalancing = true;
    [SerializeField] private float checkInterval = 3f; // How often to check FPS (seconds)
    [SerializeField] private int minAcceptableFPS = 45; // If average FPS drops below this, lower quality

    private const string GraphicsQualityKey = "GraphicsQuality";
    
    private float timer = 0f;
    private int frameCount = 0;
    private float fpsSum = 0f;

    private void Awake()
    {
        // Enforce 60 FPS and turn off VSync immediately for WebGL/Mobile
        QualitySettings.vSyncCount = 0; 
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        // Default to Normal (1) for mobile, clamped between 0 (Low) and 2 (High)
        int savedQuality = PlayerPrefs.GetInt(GraphicsQualityKey, 1);
        savedQuality = Mathf.Clamp(savedQuality, 0, 2);

        ApplyGraphicsQuality(savedQuality);

        if (graphicsDropdown != null)
        {
            graphicsDropdown.value = savedQuality;
            graphicsDropdown.RefreshShownValue();

            graphicsDropdown.onValueChanged.AddListener(ApplyGraphicsQuality);
        }
    }

    private void Update()
    {
        if (!enableAutoBalancing) return;

        timer += Time.unscaledDeltaTime;
        frameCount++;
        fpsSum += 1f / Time.unscaledDeltaTime;

        if (timer >= checkInterval)
        {
            float averageFPS = fpsSum / frameCount;
            int currentQuality = PlayerPrefs.GetInt(GraphicsQualityKey, 1);

            // If mobile FPS drops and we aren't already on Low quality (0), step it down
            if (averageFPS < minAcceptableFPS && currentQuality > 0)
            {
                int newQuality = currentQuality - 1;
                string qualityName = (newQuality == 0) ? "Low" : (newQuality == 1) ? "Normal" : "High";
                
                Debug.Log($"[Auto-Balancer] Mobile lag detected ({averageFPS:F1 FPS}). Lowering quality to: {qualityName}");
                
                ApplyGraphicsQuality(newQuality);

                if (graphicsDropdown != null)
                {
                    graphicsDropdown.value = newQuality;
                    graphicsDropdown.RefreshShownValue();
                }
            }

            timer = 0f;
            frameCount = 0;
            fpsSum = 0f;
        }
    }

    public void ApplyGraphicsQuality(int qualityLevel)
    {
        qualityLevel = Mathf.Clamp(qualityLevel, 0, 2);

        // Re-enforce 60 FPS and VSync=0
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        string qualityName = "Normal";

        switch (qualityLevel)
        {
            case 0:
                ApplyLowQuality();
                qualityName = "Low";
                break;

            case 1:
                ApplyNormalQuality();
                qualityName = "Normal";
                break;

            case 2:
                ApplyHighQuality();
                qualityName = "High";
                break;
        }

        // Save setting
        PlayerPrefs.SetInt(GraphicsQualityKey, qualityLevel);
        PlayerPrefs.Save();

        Debug.Log("Mobile Graphics Quality changed to: " + qualityName + " | Target FPS: " + Application.targetFrameRate);
    }

    private void ApplyLowQuality()
    {
        Debug.Log("Applying MOBILE LOW graphics");
        // Disable shadows entirely for maximum mobile performance
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.globalTextureMipmapLimit = 2; // Lower texture resolution to save VRAM
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.lodBias = 0.4f;
        QualitySettings.softParticles = false;
        QualitySettings.realtimeReflectionProbes = false;
    }

    private void ApplyNormalQuality()
    {
        Debug.Log("Applying MOBILE NORMAL graphics");
        // Lightweight shadows for standard mobile phones
        QualitySettings.shadows = ShadowQuality.HardOnly;
        QualitySettings.shadowDistance = 20f;
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.globalTextureMipmapLimit = 1;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.lodBias = 0.7f;
        QualitySettings.softParticles = false;
        QualitySettings.realtimeReflectionProbes = false;
    }

    private void ApplyHighQuality()
    {
        Debug.Log("Applying MOBILE HIGH graphics");
        // Balanced high settings for powerful phones/tablets (keeping it safe for WebGL)
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowDistance = 40f; // Kept low because mobile GPUs crash/lag on high shadow distances
        QualitySettings.shadowResolution = ShadowResolution.Medium;
        QualitySettings.globalTextureMipmapLimit = 0;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        QualitySettings.lodBias = 1f;
        QualitySettings.softParticles = true;
        QualitySettings.realtimeReflectionProbes = false; // Realtime reflection probes are brutal on mobile WebGL, keep disabled
    }

    private void OnDestroy()
    {
        if (graphicsDropdown != null)
        {
            graphicsDropdown.onValueChanged.RemoveListener(ApplyGraphicsQuality);
        }
    }
}