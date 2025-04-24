using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettings : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle vfxToggle; 
    [SerializeField] private Slider vfxIntensitySlider;
    [SerializeField] private TextMeshProUGUI vfxIntensityText;

    [Header("Resolution Settings")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    private Resolution[] resolutions;

    private void Start()
    {
        LoadSettings();
        SetupUIEvents();

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width}x{resolutions[i].height}";
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        fullscreenToggle.isOn = Screen.fullScreen;
    }

    private void LoadSettings()
    {
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.RefreshShownValue();
        }

        if (VFXManager.Instance != null)
        {
            if (vfxToggle != null)
            {
                vfxToggle.isOn = VFXManager.Instance.IsVFXEnabled();
            }

            if (vfxIntensitySlider != null)
            {
                vfxIntensitySlider.value = VFXManager.Instance.GetVFXIntensity();
                UpdateVFXIntensityText(vfxIntensitySlider.value);
                vfxIntensitySlider.interactable = VFXManager.Instance.IsVFXEnabled();
            }
        }
        else
        {
            Debug.LogWarning("VFXManager no encontrado. Las opciones de VFX no estarán disponibles.");
            if (vfxToggle != null) vfxToggle.interactable = false;
            if (vfxIntensitySlider != null) vfxIntensitySlider.interactable = false;
        }
    }

    private void SetupUIEvents()
    {
        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
        }
        if (vfxToggle != null)
        {
            vfxToggle.onValueChanged.AddListener(SetVFXEnabled);
        }
        if (vfxIntensitySlider != null)
        {
            vfxIntensitySlider.onValueChanged.AddListener(SetVFXIntensity);
        }
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }
    }

    #region Graphics Settings
    public void SetQualityLevel(int level)
    {
        QualitySettings.SetQualityLevel(level, true);
        PlayerPrefs.SetInt("QualityLevel", level);
        PlayerPrefs.Save();
        Debug.Log($"Nivel de calidad establecido en: {QualitySettings.names[level]}");
    }
    #endregion

    #region VFX Settings
    private void SetVFXEnabled(bool enabled)
    {
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.SetVFXEnabled(enabled);
            if (vfxIntensitySlider != null)
            {
                vfxIntensitySlider.interactable = enabled;
            }
        }
    }

    private void SetVFXIntensity(float intensity)
    {
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.SetVFXIntensity(intensity);
            UpdateVFXIntensityText(intensity);
        }
    }

    private void UpdateVFXIntensityText(float intensity)
    {
        if (vfxIntensityText != null)
        {
            vfxIntensityText.text = $"Intensidad de VFX: {(intensity * 100):F0}%";
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        Debug.Log($"Resolución establecida en: {resolution.width}x{resolution.height}");
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Pantalla completa: {isFullscreen}");
    }
    #endregion
}
