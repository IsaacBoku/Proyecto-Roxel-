using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("VFX Settings")]
    [SerializeField] private bool vfxEnabled = true;
    [SerializeField, Range(0f, 1f)] private float vfxIntensity = 1f;

    private List<ParticleSystem> particleSystems;
    private List<VFXEffect> vfxEffects;

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

        particleSystems = new List<ParticleSystem>();
        vfxEffects = new List<VFXEffect>();

        LoadSettings();
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
        FindAllParticleSystems();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAllParticleSystems();
    }

    #region VFX Control
    public void RegisterParticleSystem(ParticleSystem particleSystem)
    {
        if (particleSystem != null && !particleSystems.Contains(particleSystem))
        {
            particleSystems.Add(particleSystem);
            ApplyVFXSettingsToParticleSystem(particleSystem);
            Debug.Log($"Sistema de partículas registrado: {particleSystem.gameObject.name}");
        }
    }

    public void RegisterVFXEffect(VFXEffect effect)
    {
        if (effect != null && !vfxEffects.Contains(effect))
        {
            vfxEffects.Add(effect);
            ApplyVFXSettingsToEffect(effect);
        }
    }

    public void SetVFXEnabled(bool enabled)
    {
        vfxEnabled = enabled;
        UpdateAllVFX();
        PlayerPrefs.SetInt("VFXEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"VFX habilitados: {enabled}");
    }

    public void SetVFXIntensity(float intensity)
    {
        vfxIntensity = Mathf.Clamp01(intensity);
        UpdateAllVFX();
        PlayerPrefs.SetFloat("VFXIntensity", vfxIntensity);
        PlayerPrefs.Save();
        Debug.Log($"Intensidad de VFX establecida en: {vfxIntensity}");
    }

    public bool IsVFXEnabled()
    {
        return vfxEnabled;
    }

    public float GetVFXIntensity()
    {
        return vfxIntensity;
    }

    private void UpdateAllVFX()
    {
        foreach (var particleSystem in particleSystems)
        {
            ApplyVFXSettingsToParticleSystem(particleSystem);
        }

        foreach (var effect in vfxEffects)
        {
            ApplyVFXSettingsToEffect(effect);
        }
    }

    private void ApplyVFXSettingsToParticleSystem(ParticleSystem particleSystem)
    {
        if (particleSystem == null) return;

        if (!vfxEnabled)
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            return;
        }

        if (!particleSystem.isPlaying)
        {
            particleSystem.Play();
        }

        var emission = particleSystem.emission;
        emission.rateOverTimeMultiplier = vfxIntensity;
    }

    private void ApplyVFXSettingsToEffect(VFXEffect effect)
    {
        if (effect == null) return;

        effect.SetEnabled(vfxEnabled);
        effect.SetIntensity(vfxIntensity);
    }

    private void FindAllParticleSystems()
    {
        // Buscar todos los sistemas de partículas en la escena, incluyendo inactivos
        ParticleSystem[] systems = FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var system in systems)
        {
            RegisterParticleSystem(system);
        }
        Debug.Log($"Se encontraron y registraron {systems.Length} sistemas de partículas en la escena: {SceneManager.GetActiveScene().name}");
    }

    private void LoadSettings()
    {
        vfxEnabled = PlayerPrefs.GetInt("VFXEnabled", 1) == 1;
        vfxIntensity = PlayerPrefs.GetFloat("VFXIntensity", 1f);
        UpdateAllVFX();
        //Debug.Log($"Configuraciones de VFX cargadas: VFXEnabled={vfxEnabled}, VFXIntensity={vfxIntensity}");
    }
    #endregion
}

public interface VFXEffect
{
    void SetEnabled(bool enabled);
    void SetIntensity(float intensity);
}