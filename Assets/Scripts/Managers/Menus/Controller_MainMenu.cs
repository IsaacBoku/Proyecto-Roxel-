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
            Debug.LogError("Controller_Menus no encontrado. Asegúrate de que esté instanciado.");
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

        // Seleccionar botón inicial
        SelectInitialButton();
    }

    // Nuevo método para seleccionar el botón inicial
    public void SelectInitialButton()
    {
        if (startButton != null)
        {
            StartCoroutine(SelectButtonAfterDelay(startButton));
        }
        else
        {
            Debug.LogWarning("No se puede seleccionar botón inicial: startButton es null");
        }
    }

    private IEnumerator SelectButtonAfterDelay(Button button)
    {
        yield return null; // Esperar un frame para asegurar que el EventSystem esté listo
        EventSystem.current.SetSelectedGameObject(button.gameObject);
        Debug.Log("Botón seleccionado en Main Menu: " + button.name);
    }
}