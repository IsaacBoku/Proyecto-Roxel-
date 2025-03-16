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


    private void Awake()
    {
        //_doc = GetComponent<UIDocument>();
        root = _doc.rootVisualElement;
        _playButton = _doc.rootVisualElement.Q<Button>("PlayButton");
        _settingsButton = _doc.rootVisualElement.Q<Button>("SettingsButton");
        _exitButton = _doc.rootVisualElement.Q<Button>("ExitButton");

        _playButton.clicked += Change_Scene;
        _exitButton.clicked += Quit_Button;

    }
    private void Start()
    {
        var buttons = root.Query<Button>().ToList();

        // Inicializar el primer botón
        if (buttons.Count > 0)
        {
            currentButton = buttons[0];
            currentButton.AddToClassList("menu-Button");
            currentButton.Focus();
        }
    }
    private void Update()
    {

    }
    public void Change_Scene()
    {
        if (!string.IsNullOrEmpty(SceneName))
        {
            SceneManager.LoadScene(SceneName);
            Debug.Log("Ha cambiado a la escena: " + SceneName);
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
