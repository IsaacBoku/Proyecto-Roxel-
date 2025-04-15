using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider; // Slider para ajustar el volumen de la música
    [SerializeField] private Slider sfxSlider;   // Slider para ajustar el volumen de los efectos de sonido

    [Header("Toggle Buttons")]
    [SerializeField] private Button musicToggleButton; // Botón para mutear/desmutear la música
    [SerializeField] private Button sfxToggleButton;   // Botón para mutear/desmutear los efectos de sonido
    [SerializeField] private TextMeshProUGUI musicToggleText;     // Texto para mostrar "Music: ON/OFF"
    [SerializeField] private TextMeshProUGUI sfxToggleText;       // Texto para mostrar "SFX: ON/OFF"

    [Header("Pause/Resume Music Buttons (Optional)")]
    [SerializeField] private Button pauseMusicButton;  // Botón para pausar la música
    [SerializeField] private Button resumeMusicButton; // Botón para reanudar la música
    [SerializeField] private TextMeshProUGUI musicStatusText;     // Texto para mostrar el estado de la música (Playing/Paused)

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

    // Método para mutear/desmutear la música
    public void ToggleMusic()
    {
        AudioManager.instance.ToggleMusic();
        UpdateMusicToggleUI();
    }

    // Método para mutear/desmutear los efectos de sonido
    public void ToggleSFX()
    {
        AudioManager.instance.ToggleSFX();
        UpdateSFXToggleUI();
    }

    // Método para ajustar el volumen de la música
    public void SetMusicVolume()
    {
        if (musicSlider != null)
        {
            AudioManager.instance.SetMusicVolume(musicSlider.value);
        }
    }

    // Método para ajustar el volumen de los efectos de sonido
    public void SetSFXVolume()
    {
        if (sfxSlider != null)
        {
            AudioManager.instance.SetSFXVolume(sfxSlider.value);
        }
    }

    // Método para pausar la música (opcional, para un menú de pausa)
    public void PauseMusic()
    {
        AudioManager.instance.PauseMusic();
        UpdateMusicStatusUI();
    }

    // Método para reanudar la música (opcional, para un menú de pausa)
    public void ResumeMusic()
    {
        AudioManager.instance.ResumeMusic();
        UpdateMusicStatusUI();
    }

    // Método para detener todos los efectos de sonido (opcional, por ejemplo, al salir del juego)
    public void StopAllSFX()
    {
        AudioManager.instance.StopAllSFX();
    }

    // Método para detener la música (opcional, por ejemplo, al salir del juego)
    public void StopMusic()
    {
        AudioManager.instance.StopMusic();
    }

    // Actualizar el texto del botón de muteo de música
    private void UpdateMusicToggleUI()
    {
        if (musicToggleButton != null && musicToggleText != null)
        {
            bool isMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
            musicToggleText.text = isMuted ? "Music: OFF" : "Music: ON";
        }
    }

    // Actualizar el texto del botón de muteo de efectos de sonido
    private void UpdateSFXToggleUI()
    {
        if (sfxToggleButton != null && sfxToggleText != null)
        {
            bool isMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
            sfxToggleText.text = isMuted ? "SFX: OFF" : "SFX: ON";
        }
    }

    // Actualizar el texto del estado de la música (Playing/Paused)
    private void UpdateMusicStatusUI()
    {
        if (musicStatusText != null)
        {
            musicStatusText.text = AudioManager.instance != null && musicSourceIsPlaying() ? "Playing" : "Paused";
        }

        // Habilitar/deshabilitar botones de pausa/reanudar según el estado
        if (pauseMusicButton != null && resumeMusicButton != null)
        {
            bool isPlaying = musicSourceIsPlaying();
            pauseMusicButton.interactable = isPlaying;
            resumeMusicButton.interactable = !isPlaying;
        }
    }

    private bool musicSourceIsPlaying()
    {
        // Esto es una aproximación. Idealmente, AudioManager debería exponer este estado.
        return AudioManager.instance != null && PlayerPrefs.GetInt("MusicMuted", 0) == 0;
    }
}
