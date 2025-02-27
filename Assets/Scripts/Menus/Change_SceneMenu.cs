using UnityEngine;
using UnityEngine.SceneManagement;

public class Change_SceneMenu : MonoBehaviour
{
    [Header("Change Scene")]
    [SerializeField] private string scene_Menu;
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
}
