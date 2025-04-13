using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; // Para música de fondo
    [SerializeField] private int sfxChannels = 5; // Número de canales para SFX
    private List<AudioSource> sfxSources; // Pool de AudioSource para SFX

    [Header("Sounds")]
    [SerializeField] private Sound[] musicSounds; // Lista de sonidos de música
    [SerializeField] private Sound[] sfxSounds; // Lista de sonidos de efectos

    [Header("Fade Settings")]
    [SerializeField] private float musicFadeDuration = 1f; // Duración del fade para música

    private string currentMusic; // Nombre de la música actualmente en reproducción

    private void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Inicializar el pool de AudioSource para SFX
        sfxSources = new List<AudioSource>();
        for (int i = 0; i < sfxChannels; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sfxSources.Add(source);
        }

        // Cargar configuraciones guardadas
        LoadSettings();
    }

    private void Start()
    {
        PlayMusic("MainTheme");
    }

    #region Music Playback
    public void PlayMusic(string name)
    {
        if (currentMusic == name && musicSource.isPlaying)
        {
            Debug.Log($"La música '{name}' ya está reproduciéndose.");
            return;
        }

        Sound s = Array.Find(musicSounds, x => x.name == name);
        if (s == null)
        {
            Debug.LogWarning($"Música '{name}' no encontrada!");
            return;
        }

        currentMusic = name;
        StartCoroutine(FadeMusic(s));
    }

    public void StopMusic()
    {
        StartCoroutine(FadeOutMusic());
    }

    public void PauseMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
            Debug.Log($"Música '{currentMusic}' pausada.");
        }
    }

    public void ResumeMusic()
    {
        if (!musicSource.isPlaying && !string.IsNullOrEmpty(currentMusic))
        {
            musicSource.Play();
            Debug.Log($"Música '{currentMusic}' reanudada.");
        }
    }

    private IEnumerator FadeMusic(Sound sound)
    {
        // Si ya hay música reproduciéndose, hacer fade out
        if (musicSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutMusic());
        }

        // Configurar el nuevo clip
        musicSource.clip = sound.clips.Length > 0 ? sound.clips[UnityEngine.Random.Range(0, sound.clips.Length)] : null;
        if (musicSource.clip == null)
        {
            Debug.LogWarning($"No hay clips asignados para la música '{sound.name}'.");
            yield break;
        }

        musicSource.volume = 0f;
        musicSource.pitch = sound.pitch;
        musicSource.loop = sound.loop;
        musicSource.Play();

        // Hacer fade in
        float elapsedTime = 0f;
        float targetVolume = sound.volume;
        while (elapsedTime < musicFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsedTime / musicFadeDuration);
            yield return null;
        }
        musicSource.volume = targetVolume;
    }

    private IEnumerator FadeOutMusic()
    {
        float elapsedTime = 0f;
        float startVolume = musicSource.volume;
        while (elapsedTime < musicFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / musicFadeDuration);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = 0f;
        currentMusic = null;
    }
    #endregion

    #region SFX Playback
    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);
        if (s == null)
        {
            Debug.LogWarning($"Efecto de sonido '{name}' no encontrado!");
            return;
        }

        // Elegir un clip aleatorio si hay variaciones
        AudioClip clip = s.clips.Length > 0 ? s.clips[UnityEngine.Random.Range(0, s.clips.Length)] : null;
        if (clip == null)
        {
            Debug.LogWarning($"No hay clips asignados para el efecto de sonido '{name}'.");
            return;
        }

        // Buscar un AudioSource disponible
        AudioSource availableSource = sfxSources.Find(source => !source.isPlaying);
        if (availableSource == null)
        {
            Debug.LogWarning($"No hay canales de SFX disponibles para reproducir '{name}'. Aumenta el número de canales.");
            return;
        }

        availableSource.clip = clip;
        availableSource.volume = s.volume;
        availableSource.pitch = s.pitch;
        availableSource.loop = s.loop;
        availableSource.Play();
    }

    public void StopAllSFX()
    {
        foreach (AudioSource source in sfxSources)
        {
            source.Stop();
        }
    }
    #endregion

    #region Volume and Mute Controls
    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
        PlayerPrefs.SetInt("MusicMuted", musicSource.mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleSFX()
    {
        bool newMuteState = !sfxSources[0].mute;
        foreach (AudioSource source in sfxSources)
        {
            source.mute = newMuteState;
        }
        PlayerPrefs.SetInt("SFXMuted", newMuteState ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void MusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MusicVolume", musicSource.volume);
        PlayerPrefs.Save();
    }

    public void SFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        foreach (AudioSource source in sfxSources)
        {
            source.volume = volume;
        }
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        // Cargar configuraciones guardadas
        musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        foreach (AudioSource source in sfxSources)
        {
            source.volume = sfxVolume;
        }

        musicSource.mute = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        bool sfxMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
        foreach (AudioSource source in sfxSources)
        {
            source.mute = sfxMuted;
        }
    }
    #endregion
}
