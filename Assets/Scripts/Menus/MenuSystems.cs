using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSystems : MonoBehaviour
{
    public bool isPause = false;
    [Header("Change Scene")]
    [SerializeField] private string scene_Menu;
    [SerializeField] private string scene_Retry;
    [Header("Menus")]
    [SerializeField] private GameObject menu_Pause;
    [SerializeField] private GameObject menu_Configurations;
    [SerializeField] private GameObject menu_Options;
    [SerializeField] private GameObject menu_Controls;
    [SerializeField] private GameObject menu_Audio;
    [SerializeField] private GameObject menu_Graphics;

    [Header("Scripts")]
    public MenuEventSystemHadler menusSystems;

    [Header("Animations Menus")]
    public Animator ani_MenuPause;
    public Animator ani_MenuOptions;

    [Header("Quit Panel")]
    [SerializeField] private GameObject quitPanel;
    public Animator aniQuitPanel;

    [SerializeField] PlayerInputHadler InputHadler;
    private void Start()
    {
        Menus_Closed();
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
        menu_Configurations.SetActive(false);
    }
    public void Menu_Pause()
    {
        isPause = !isPause;
        if (InputHadler.OptionsInput && isPause)
        {
            StartCoroutine(Cooldown_menus_Closed());
            Pause();
            InputHadler.UseOptionsInput();
            isPause = false;
        }
    }
    public void Button_Pause()
    {
        Resume();
        StartCoroutine(Cooldown_menus_Open());
        InputHadler.UseOptionsInput();
        isPause = true;
    }
    public void Button_MenuPause()
    {
        StartCoroutine(Cooldown_menus_Closed());
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(false);
    }
    public void Button_Options()
    {
        StartCoroutine(Cooldown_menu_options_open());
    }
    public void Button_Graphics()
    {
        menu_Options.SetActive(false);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(true);
    }
    public void Button_Audio()
    {
        menu_Options.SetActive(false);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(true);
        menu_Graphics.SetActive(false);
    }
    public void Button_Controls()
    {
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
    public void Button_Retry()
    {
        if (!string.IsNullOrEmpty(scene_Retry))
        {
            SceneManager.LoadScene(scene_Retry);
            Debug.Log("Ha cambiado a la escena: " + scene_Retry);
        }
        else
        {
            Debug.Log("No hay Scene cambiante");
        }
    }
    IEnumerator Cooldown_menus_Closed()
    {
        ani_MenuOptions.SetBool("Open", false);
        menu_Pause.SetActive(true);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        ani_MenuPause.SetBool("Open", true);
        menusSystems.enabled = true;
        yield return new WaitForSeconds(1f);
        menu_Configurations.SetActive(false);
    }
    IEnumerator Cooldown_menus_Open()
    {
        ani_MenuPause.SetBool("Open", false);
        yield return new WaitForSeconds(1f);
        menu_Pause.SetActive(false);
    }
    IEnumerator Cooldown_menu_options_open()
    {
        //ani_MenuPause.SetBool("Open", false);
        menusSystems.enabled = false;
        menu_Configurations.SetActive(true);
        menu_Options.SetActive(true);
        menu_Controls.SetActive(false);
        menu_Audio.SetActive(false);
        menu_Graphics.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        //menu_Pause.SetActive(false);
        ani_MenuOptions.SetBool("Open", true);

    }
    public void Button_QuitPanel()
    {
        StartCoroutine(QuitPanel());
    }
    private IEnumerator QuitPanel()
    {
        quitPanel.SetActive(true);
        menusSystems.enabled = false;
        yield return new WaitForSeconds(1f);
        aniQuitPanel.SetBool("Quit", true);
    }
    public void Button_Quit()
    {
        Application.Quit();
        //EditorApplication.isPlaying = false;
        Debug.Log("Ha salido del juego");
    }
    public void Button_FalsePanel()
    {
        StartCoroutine(FalsePanel());
    }
    private IEnumerator FalsePanel()
    {
        aniQuitPanel.SetBool("Quit", false);
        yield return new WaitForSeconds(1f);
        menusSystems.enabled = true;
        quitPanel.SetActive(false);
    }
}
