using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider; // Slider para ajustar el volumen de la m�sica
    [SerializeField] private Slider sfxSlider;   // Slider para ajustar el volumen de los efectos de sonido

    [Header("Toggle Buttons")]
    [SerializeField] private Button musicToggleButton; // Bot�n para mutear/desmutear la m�sica
    [SerializeField] private Button sfxToggleButton;   // Bot�n para mutear/desmutear los efectos de sonido
    [SerializeField] private TextMeshProUGUI musicToggleText;     // Texto para mostrar "Music: ON/OFF"
    [SerializeField] private TextMeshProUGUI sfxToggleText;       // Texto para mostrar "SFX: ON/OFF"

    [Header("Pause/Resume Music Buttons (Optional)")]
    [SerializeField] private Button pauseMusicButton;  // Bot�n para pausar la m�sica
    [SerializeField] private Button resumeMusicButton; // Bot�n para reanudar la m�sica
    [SerializeField] private TextMeshProUGUI musicStatusText;     // Texto para mostrar el estado de la m�sica (Playing/Paused)

    private void Start()
    {
        // Inicializar los sliders con los valores actuales del AudioManager
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.onValueChanged.AddListener(delegate { SetMusicVolume(); });
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.onValueChanged.AddListener(delegate { SetSFXVolume(); });
        }

        // Inicializar el estado de los botones de muteo
        UpdateMusicToggleUI();
        UpdateSFXToggleUI();

        // Inicializar el estado de los botones de pausa/reanudar
        UpdateMusicStatusUI();
    }

    // M�todo para mutear/desmutear la m�sica
    public void ToggleMusic()
    {
        AudioManager.instance.ToggleMusic();
        UpdateMusicToggleUI();
    }

    // M�todo para mutear/desmutear los efectos de sonido
    public void ToggleSFX()
    {
        AudioManager.instance.ToggleSFX();
        UpdateSFXToggleUI();
    }

    // M�todo para ajustar el volumen de la m�sica
    public void SetMusicVolume()
    {
        if (musicSlider != null)
        {
            AudioManager.instance.SetMusicVolume(musicSlider.value);
        }
    }

    // M�todo para ajustar el volumen de los efectos de sonido
    public void SetSFXVolume()
    {
        if (sfxSlider != null)
        {
            AudioManager.instance.SetSFXVolume(sfxSlider.value);
        }
    }

    // M�todo para pausar la m�sica (opcional, para un men� de pausa)
    public void PauseMusic()
    {
        AudioManager.instance.PauseMusic();
        UpdateMusicStatusUI();
    }

    // M�todo para reanudar la m�sica (opcional, para un men� de pausa)
    public void ResumeMusic()
    {
        AudioManager.instance.ResumeMusic();
        UpdateMusicStatusUI();
    }

    // M�todo para detener todos los efectos de sonido (opcional, por ejemplo, al salir del juego)
    public void StopAllSFX()
    {
        AudioManager.instance.StopAllSFX();
    }

    // M�todo para detener la m�sica (opcional, por ejemplo, al salir del juego)
    public void StopMusic()
    {
        AudioManager.instance.StopMusic();
    }

    // Actualizar el texto del bot�n de muteo de m�sica
    private void UpdateMusicToggleUI()
    {
        if (musicToggleButton != null && musicToggleText != null)
        {
            bool isMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
            musicToggleText.text = isMuted ? "Music: OFF" : "Music: ON";
        }
    }

    // Actualizar el texto del bot�n de muteo de efectos de sonido
    private void UpdateSFXToggleUI()
    {
        if (sfxToggleButton != null && sfxToggleText != null)
        {
            bool isMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
            sfxToggleText.text = isMuted ? "SFX: OFF" : "SFX: ON";
        }
    }

    // Actualizar el texto del estado de la m�sica (Playing/Paused)
    private void UpdateMusicStatusUI()
    {
        if (musicStatusText != null)
        {
            musicStatusText.text = AudioManager.instance != null && musicSourceIsPlaying() ? "Playing" : "Paused";
        }

        // Habilitar/deshabilitar botones de pausa/reanudar seg�n el estado
        if (pauseMusicButton != null && resumeMusicButton != null)
        {
            bool isPlaying = musicSourceIsPlaying();
            pauseMusicButton.interactable = isPlaying;
            resumeMusicButton.interactable = !isPlaying;
        }
    }

    private bool musicSourceIsPlaying()
    {
        // Esto es una aproximaci�n. Idealmente, AudioManager deber�a exponer este estado.
        return AudioManager.instance != null && PlayerPrefs.GetInt("MusicMuted", 0) == 0;
    }
}
