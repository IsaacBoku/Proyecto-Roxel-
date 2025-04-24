using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Controller_MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private string startSceneName = "Level1";

    private void Start()
    {
        Controller_Menus controller = Controller_Menus.Instance;
        if (controller == null)
        {
            Debug.LogError("Controller_Menus no encontrado. Aseg�rate de que est� instanciado.");
            return;
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => controller.Button_ChangeScene(startSceneName));
        }
        else
        {
            Debug.LogWarning("StartButton no asignado en MainMenuButtonSetup.");
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.RemoveAllListeners();
            optionsButton.onClick.AddListener(controller.OpenOptionsDirectly);
        }
        else
        {
            Debug.LogWarning("OptionsButton no asignado en MainMenuButtonSetup.");
        }

        // Registrar botones en MenuEventSystemHandler
        MenuEventSystemHadler eventSystemHandler = FindFirstObjectByType<MenuEventSystemHadler>();
        if (eventSystemHandler != null)
        {
            eventSystemHandler.Selectables.Clear();
            if (startButton != null) eventSystemHandler.Selectables.Add(startButton);
            if (optionsButton != null) eventSystemHandler.Selectables.Add(optionsButton);
            eventSystemHandler.Awake(); // Reinicializar para registrar los nuevos Selectables
            Debug.Log("Botones del Main Menu registrados en MenuEventSystemHandler");
        }

        // Seleccionar bot�n inicial
        SelectInitialButton();
    }

    // Nuevo m�todo para seleccionar el bot�n inicial
    public void SelectInitialButton()
    {
        if (startButton != null)
        {
            StartCoroutine(SelectButtonAfterDelay(startButton));
        }
        else
        {
            Debug.LogWarning("No se puede seleccionar bot�n inicial: startButton es null");
        }
    }

    private IEnumerator SelectButtonAfterDelay(Button button)
    {
        yield return null; // Esperar un frame para asegurar que el EventSystem est� listo
        EventSystem.current.SetSelectedGameObject(button.gameObject);
        Debug.Log("Bot�n seleccionado en Main Menu: " + button.name);
    }
}