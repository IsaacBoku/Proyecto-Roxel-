using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Change_SceneMenu : MonoBehaviour
{
    [Header("Name Scene")] 
    public string SceneName;
    

    public void Change_Scene()
    {
        if (!string.IsNullOrEmpty(SceneName))
        {
            SceneManager.LoadScene(SceneName);
            Debug.Log("Ha cambiado a la escena: "+ SceneName);
        }
        else
        {
            Debug.Log("No hay Scene cambiante");
        }
    }
    public void Quit_Button()
    {
        Application.Quit();
        Debug.Log("Has salido del juego");
    }

}
