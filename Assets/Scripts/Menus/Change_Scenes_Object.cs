using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Change_Scenes_Object : MonoBehaviour
{
    [Header("Change Scene")]
    [SerializeField] private string scene_Menu;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Change_Scene();
        }
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
}
