using UnityEngine;
using UnityEngine.UI;

public class Controller_MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton; // Bot�n de Start
    [SerializeField] private Button optionsButton; // Bot�n de Options
    [SerializeField] private string startSceneName = "Level1"; // Escena a cargar al presionar Start

    private void Start()
    {
        // Buscar el Controller_Menus persistente
        Controller_Menus controller = Controller_Menus.Instance;
        if (controller == null)
        {
            Debug.LogError("Controller_Menus no encontrado. Aseg�rate de que est� instanciado.");
            return;
        }

        // Configurar el evento OnClick del bot�n Start
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners(); // Limpiar eventos previos
            startButton.onClick.AddListener(() => controller.Button_ChangeScene(startSceneName));
        }
        else
        {
            Debug.LogWarning("StartButton no asignado en MainMenuButtonSetup.");
        }

        // Configurar el evento OnClick del bot�n Options
        if (optionsButton != null)
        {
            optionsButton.onClick.RemoveAllListeners(); // Limpiar eventos previos
            optionsButton.onClick.AddListener(controller.OpenOptionsDirectly);
        }
        else
        {
            Debug.LogWarning("OptionsButton no asignado en MainMenuButtonSetup.");
        }
    }
}
