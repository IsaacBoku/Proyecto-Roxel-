using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Xml.Linq;
using UnityEngine.InputSystem.Controls;
using Unity.VisualScripting;

public class UIManager : MonoBehaviour
{
    public UIDocument _doc;
    public InputActionAsset inputActions;

    private EventSystem eventSystem;
    private InputSystemUIInputModule uiInputModule;

    private VisualElement root;
    private Button currentButton;


    private Button _playButton;
    private Button _settingsButton;
    private Button _exitButton;

    [Header("Name Scene")]
    public string SceneName;


    private void Update()
    {

    }
    public void Change_Scene(string nameScene)
    {
        if (!string.IsNullOrEmpty(nameScene))
        {
            SceneManager.LoadScene(nameScene);
            Debug.Log("Ha cambiado a la escena: " + nameScene);
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
