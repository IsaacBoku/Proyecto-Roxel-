using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuInputHandler : MonoBehaviour, IMenuInputHandler
{
    [SerializeField] private InputActionAsset inputActions;
    private PlayerInput input;

    public Vector2 NavigateInput { get; private set; }
    public bool SubmitInput { get; private set; }
    public bool CancelInput { get; private set; }
    public bool QInput { get; private set; }
    public bool RInput { get; private set; }
    public bool OptionsInput { get; private set; }

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        if (input == null)
        {
            input = gameObject.AddComponent<PlayerInput>();
        }

        // Asignar el archivo de Input Actions si no está configurado
        if (input.actions == null && inputActions != null)
        {
            input.actions = inputActions;
            Debug.Log("Archivo de Input Actions asignado dinámicamente en MainMenuInputHandler");
        }
        else if (input.actions == null)
        {
            Debug.LogError("No se encontró un archivo de Input Actions. Asigna uno en el Inspector o asegúrate de que esté configurado en el componente PlayerInput.");
            return;
        }

        // Verificar que el mapa "UI" exista
        if (input.actions.FindActionMap("UI") == null)
        {
            Debug.LogError("El mapa de acciones 'UI' no se encontró en el archivo de Input Actions asignado.");
            return;
        }
        input.defaultActionMap = "UI";
        SwitchToUIInput();
    }

    private void Start()
    {
        if (Controller_Menus.Instance != null)
        {
            Controller_Menus.Instance.RegisterInputHandler(this);
        }
    }

    private void OnDestroy()
    {
        if (Controller_Menus.Instance != null)
        {
            Controller_Menus.Instance.UnregisterInputHandler();
        }
    }

    public void OnNavigateInput(InputAction.CallbackContext context)
    {
        NavigateInput = context.ReadValue<Vector2>();
    }

    public void OnSubmitInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SubmitInput = true;
        }
        if (context.canceled)
        {
            SubmitInput = false;
        }
    }

    public void OnCancelInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            CancelInput = true;
        }
        if (context.canceled)
        {
            CancelInput = false;
        }
    }

    public void OnQInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            QInput = true;
        }
        if (context.canceled)
        {
            QInput = false;
        }
    }

    public void OnRInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            RInput = true;
        }
        if (context.canceled)
        {
            RInput = false;
        }
    }

    public void OnOptionsInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OptionsInput = true;
        }
        if (context.canceled)
        {
            OptionsInput = false;
        }
    }

    public void UseSubmitInput() => SubmitInput = false;
    public void UseCancelInput() => CancelInput = false;
    public void UseQInput() => QInput = false;
    public void UseRInput() => RInput = false;
    public void UseOptionsInput() => OptionsInput = false;

    public void OnPause()
    {
        SwitchToUIInput();
    }

    public void OnGame()
    {
        SwitchToGameplayInput();
    }

    private void SwitchToUIInput()
    {
        if (input != null && input.actions != null)
        {
            input.SwitchCurrentActionMap("UI");
            Debug.Log("Cambiado al mapa de acciones UI en MainMenuInputHandler");
        }
        else
        {
            Debug.LogWarning("No se puede cambiar al mapa UI: PlayerInput o actions es null");
        }
    }

    private void SwitchToGameplayInput()
    {
        if (input != null && input.actions != null)
        {
            input.SwitchCurrentActionMap("Gameplay");
            Debug.Log("Cambiado al mapa de acciones Gameplay en MainMenuInputHandler");
        }
        else
        {
            Debug.LogWarning("No se puede cambiar al mapa Gameplay: PlayerInput o actions es null");
        }
    }
}
