using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuSystems : MonoBehaviour
{
    public bool isPause = false;
    [Header("Change Scene")]
    [SerializeField] private string scene_Menu;
    [Header("Menus")]
    [SerializeField] private GameObject menu_Pause;
    [SerializeField] private GameObject menu_Options;
    [SerializeField] private GameObject menu_Controls;
    [SerializeField] private GameObject menu_Audio;
    [SerializeField] private GameObject menu_Graphics;

    [Header("Animations Menus")]
    public Animator ani_MenuPause;

    [SerializeField] PlayerInputHadler InputHadler;
    private void Awake()
    {

    }
    private void Start()
    {
        //Menus_Closed();
    }
    private void Update()
    {
        Menu_Pause();
    }
    void Resume()
    {
        InputHadler.OnGame();
        Time.timeScale = 1f;
    }
    void Pause()
    {
        InputHadler.OnPause();
        //Time.timeScale = 0f;
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
            StartCoroutine(Cooldown_menus_Closed());
            //ani_MenuPause.SetBool("Open", true);
            Pause();
            /*menu_Pause.SetActive(true);
            menu_Options.SetActive(false);
            menu_Controls.SetActive(false);
            menu_Audio.SetActive(false);
            menu_Graphics.SetActive(false);*/
            InputHadler.UseOptionsInput();
            isPause = false;
        }
    }
    public void Button_Pause()
    {
        //ani_MenuPause.SetBool("Open", false);
        Resume();
        StartCoroutine(Cooldown_menus_Open());
        /*menu_Pause.SetActive(false);
        menu_Options.SetActive(false);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(false);*/
        InputHadler.UseOptionsInput();
        isPause = true;
    }
    public void Button_MenuPause()
    {
        ani_MenuPause.SetBool("Open", true);
        menu_Pause.SetActive(true);
        menu_Options.SetActive(false);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(false);
        
    }
    public void Button_Options()
    {
        //ani_MenuPause.SetBool("Open", false);
        menu_Pause.SetActive(false);
        menu_Options.SetActive(true);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(false);
        StartCoroutine(Cooldown_menus_Open());
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
    public void Button_ChangeScene()
    {
        if (!string.IsNullOrEmpty(scene_Menu))
        {
            SceneManager.LoadScene(scene_Menu);
            Debug.Log("Ha cambiado a la escena: " + scene_Menu);
        }
        else
        {
            Debug.Log("No hay Scene cambiante");
        }
    }
    IEnumerator Cooldown_menus_Closed()
    {
        menu_Pause.SetActive(true);
        menu_Options.SetActive(false);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        ani_MenuPause.SetBool("Open", true);
    }
    IEnumerator Cooldown_menus_Open()
    {
        ani_MenuPause.SetBool("Open", false);
        yield return new WaitForSeconds(1f);
        menu_Pause.SetActive(false);
        menu_Options.SetActive(false);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(false);
    }
}
