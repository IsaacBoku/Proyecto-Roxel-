using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private int sfxChannels = 5;
    private List<AudioSource> sfxSources;

    [Header("Sounds")]
    [SerializeField] private Sound[] musicSounds;
    [SerializeField] private Sound[] sfxSounds;
    private AudioSource currentPlayingSource; // Para rastrear el sonido actual

    [Header("Fade Settings")]
    [SerializeField] private float musicFadeDuration = 1f;

    private string currentMusic;

    private float globalSFXVolume = 1f; // Variable para almacenar el volumen global de SFX
    private bool globalSFXMuted = false; // Variable para almacenar el estado de muteo global de SFX

    private void Awake()
    {
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

        if (musicSource == null)
        {
            Debug.LogWarning("MusicSource no está asignado en AudioManager. Creando uno nuevo.");
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }

        sfxSources = new List<AudioSource>();
        for (int i = 0; i < sfxChannels; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sfxSources.Add(source);
        }
    }

    private void Start()
    {
        LoadSettings(); // Cargar configuraciones al inicio
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
        if (musicSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutMusic());
        }

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

        AudioClip clip = s.clips.Length > 0 ? s.clips[UnityEngine.Random.Range(0, s.clips.Length)] : null;
        if (clip == null)
        {
            Debug.LogWarning($"No hay clips asignados para el efecto de sonido '{name}'.");
            return;
        }

        AudioSource availableSource = sfxSources.Find(source => !source.isPlaying);
        if (availableSource == null)
        {
            Debug.LogWarning($"No hay canales de SFX disponibles para reproducir '{name}'. Aumenta el número de canales.");
            return;
        }

        availableSource.clip = clip;
        availableSource.volume = s.volume * globalSFXVolume; // Multiplicar por el volumen global
        availableSource.pitch = s.pitch;
        availableSource.loop = s.loop;
        availableSource.mute = globalSFXMuted; // Aplicar el estado de muteo global
        availableSource.Play();
    }

    public void StopSFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);
        if (s == null)
        {
            Debug.LogWarning($"Efecto de sonido '{name}' no encontrado!");
            return;
        }

        AudioSource playingSource = sfxSources.Find(source => source.isPlaying && source.clip == s.clips[0]);
        if (playingSource != null)
        {
            playingSource.Stop();
            Debug.Log($"Efecto de sonido '{name}' detenido.");
        }
        else
        {
            Debug.LogWarning($"No se encontró un canal reproduciendo '{name}'.");
        }
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
    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
        Debug.Log($"Volumen de música establecido en: {volume}");
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        globalSFXVolume = volume; // Actualizar el volumen global
        foreach (AudioSource source in sfxSources)
        {
            if (source.isPlaying)
            {
                Sound sound = Array.Find(sfxSounds, s => s.clips != null && s.clips.Length > 0 && s.clips[0] == source.clip);
                source.volume = sound != null ? sound.volume * volume : volume; // Aplicar volumen global
            }
        }
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
        Debug.Log($"Volumen de SFX establecido en: {volume}");
    }

    public void SetMusicMute(bool mute)
    {
        musicSource.mute = mute;
        PlayerPrefs.SetInt("MusicMuted", mute ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Música silenciada: {mute}");
    }

    public void SetSFXMute(bool mute)
    {
        globalSFXMuted = mute; // Actualizar el estado de muteo global
        foreach (AudioSource source in sfxSources)
        {
            source.mute = mute;
        }
        PlayerPrefs.SetInt("SFXMuted", mute ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"SFX silenciados: {mute}");
    }

    public void ToggleMusic()
    {
        SetMusicMute(!musicSource.mute);
    }

    public void ToggleSFX()
    {
        SetSFXMute(!globalSFXMuted);
    }

    public float GetMusicVolume()
    {
        return musicSource.volume;
    }

    public float GetSFXVolume()
    {
        return globalSFXVolume;
    }

    public bool IsMusicMuted()
    {
        return musicSource.mute;
    }

    public bool IsSFXMuted()
    {
        return globalSFXMuted;
    }

    private void LoadSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        globalSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        bool musicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        globalSFXMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;

        musicSource.volume = musicVolume;
        musicSource.mute = musicMuted;

        foreach (AudioSource source in sfxSources)
        {
            source.volume = globalSFXVolume;
            source.mute = globalSFXMuted;
        }

        Debug.Log($"Configuraciones de audio cargadas: MusicVolume={musicVolume}, SFXVolume={globalSFXVolume}, MusicMuted={musicMuted}, SFXMuted={globalSFXMuted}");
    }
    #endregion
}
