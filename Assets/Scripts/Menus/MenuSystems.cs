using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuSystems : MonoBehaviour
{
    public bool isPause = false;

    [Header("Menus")]
    [SerializeField] private GameObject menu_Pause;
    [SerializeField] private GameObject menu_Options;
    [SerializeField] private GameObject menu_Controls;
    [SerializeField] private GameObject menu_Audio;
    [SerializeField] private GameObject menu_Graphics;

    [SerializeField] PlayerInputHadler InputHadler;
    private void Awake()
    {

    }
    private void Start()
    {
        Menus_Closed();
    }
    private void Update()
    {
        Menu_Pause();
    }
    void Menus_Closed()
    {
        //Desactiva todos los menus antes de entrar en la Scene
        menu_Pause.SetActive(false);
        menu_Options.SetActive(false);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(false);
    }
    public void Menu_Pause()
    {
        isPause = !isPause;
        if (InputHadler.OptionsInput && isPause)
        {
            InputHadler.OnPause();
            menu_Pause.SetActive(true);
            menu_Options.SetActive(false);
            menu_Controls.SetActive(false);
            menu_Audio.SetActive(false);
            menu_Graphics.SetActive(false);
            InputHadler.UseOptionsInput();
            isPause = false;
        }
    }
    public void Button_Pause()
    {
        InputHadler.OnGame();
        menu_Pause.SetActive(false);
        menu_Options.SetActive(false);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(false);
        InputHadler.UseOptionsInput();
        isPause = true;
    }
    public void Button_MenuPause()
    {
        menu_Pause.SetActive(true);
        menu_Options.SetActive(false);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(false);
    }
    public void Button_Options()
    {
        menu_Pause.SetActive(false);
        menu_Options.SetActive(true);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(false);
    }
    public void Button_Graphics()
    {
        menu_Pause.SetActive(false);
        menu_Options.SetActive(false);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(true);
    }
    public void Button_Audio()
    {
        menu_Pause.SetActive(false);
        menu_Options.SetActive(false);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(true);
        menu_Graphics.SetActive(false);
    }
    public void Button_Controls()
    {
        menu_Pause.SetActive(false);
        menu_Options.SetActive(false);
        menu_Controls.SetActive(true);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(false);
    }
}
