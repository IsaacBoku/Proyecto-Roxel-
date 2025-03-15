using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Slider musicSlider, sfxSlider;

    public void TogleMusic()
    {
        AudioManager.instance.toggleMusic();
    }
    public void TogleSFX()
    {
        AudioManager.instance.toggleSFX();
    }
    public void SetMusicVolume()
    {
        AudioManager.instance.MusicVolume(musicSlider.value);
    }
    public void SetSFXVolume()
    {
        AudioManager.instance.SFXVolume(sfxSlider.value);
    }
}
