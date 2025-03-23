using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Change_SceneMenu : MonoBehaviour
{
    [Header("Change Scene")]
    [SerializeField] private string scene_Menu;

    [Header("Quit Panel")]
    [SerializeField] private GameObject quitPanel;
    public Animator aniQuitPanel;
    [Header("Scripts")]
    public MenuEventSystemHadler menusSystems;
    private void Start()
    {
        Cursor.visible = true;
    }
    public void Change_Scene()
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
    public void Button_Itchio()
    {
        Application.OpenURL("https://isaacboku.itch.io/");
    }
    public void  Button_Litree()
    {
        Application.OpenURL("https://linktr.ee/IsaacBoku");
    }
}
