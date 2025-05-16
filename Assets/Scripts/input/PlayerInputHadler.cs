using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerInputHadler : MonoBehaviour
{
    #region InputsVariables
    // Gameplay inputs
    public Vector2 RawMovementInput { get; private set; }
    public int NormInputX { get; private set; }
    public int NormInputY { get; private set; }
    public bool JumpInput { get; private set; }
    public bool JumpInputStop { get; private set; }
    public bool OptionsInput { get; private set; }
    public bool OptionsInputStop { get; private set; }
    public bool GrabInput { get; private set; }
    public bool GrabInputStop { get; private set; }
    public bool ThrowInput { get; private set; }
    public bool ThrowInputStop { get; private set; }
    public Vector2 MousePosition { get; private set; }
    public Vector2 AimDirection { get; private set; }
    public bool MagneticInput { get; private set; }
    public bool MagneticInputStop { get; private set; }
    public bool SeparateInput { get; private set; }
    public bool SeparateInputStop { get; private set; }
    public bool AttractInput { get; private set; }
    public bool InteractInput { get; private set; }
    public bool InteractInputStop { get; private set; }
    public bool SwitchPolarityInput { get; private set; }
    public bool UpgradesInput { get; private set; }
    public bool UpgradesInputStop { get; private set; }

    // UI inputs
    public Vector2 NavigateInput { get; private set; }
    public bool SubmitInput { get; private set; }
    public bool CancelInput { get; private set; }
    public bool QInput { get; private set; } // Nuevo: Input para Q
    public bool RInput { get; private set; } // Nuevo: Input para R
    #endregion

    #region Unity CallBack
    private PlayerInput input;
    private bool isPaused;

    [SerializeField]
    private float inputHoldTime = 0.2f;

    private float jumpInputStartTime;

    private void Start()
    {
        input = GetComponent<PlayerInput>();
        if (input == null)
        {
            Debug.LogError("Componente PlayerInput no encontrado en " + gameObject.name);
            input = gameObject.AddComponent<PlayerInput>();
        }

        if (Controller_Menus.Instance != null)
        {
            Controller_Menus.Instance.RegisterInputHandler(this);
        }

        ControlsSettings controlsSettings = FindAnyObjectByType<ControlsSettings>();
        if (controlsSettings != null)
        {
            controlsSettings.InitializeActions();
            controlsSettings.ApplyKeyBindings(this);
        }

        SwitchToGameplayInput();
        Debug.Log("PlayerInputHandler inicializado en " + gameObject.name);
    }

    private void Update()
    {
        CheckJumpInputHoldTime();
    }
    #endregion

    #region Input Handlers - Gameplay
    public void OnMoveInput(InputAction.CallbackContext context)
    {
        RawMovementInput = context.ReadValue<Vector2>();
        NormInputX = (int)(RawMovementInput * Vector2.right).normalized.x;
        NormInputY = (int)(RawMovementInput * Vector2.up).normalized.y;
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            JumpInput = true;
            JumpInputStop = false;
            jumpInputStartTime = Time.time;
        }
        if (context.canceled)
        {
            JumpInputStop = true;
        }
    }

    public void OnThrowInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ThrowInput = true;
            ThrowInputStop = false;
        }
        else if (context.canceled)
        {
            ThrowInput = false;
            ThrowInputStop = true;
        }
    }

    public void OnMousePosition(InputAction.CallbackContext context)
    {
        Vector2 inputValue = context.ReadValue<Vector2>();
        var controlDevice = context.control.device;

        if (controlDevice is Mouse)
        {
            MousePosition = inputValue;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(inputValue.x, inputValue.y, Camera.main.nearClipPlane));
            AimDirection = (worldPos - transform.position).normalized;
           //Debug.Log("Entrada de ratón: Posición en pantalla = " + MousePosition + ", Dirección de apuntado = " + AimDirection);
        }
        else if (controlDevice is Gamepad)
        {
            AimDirection = inputValue.normalized;
            if (AimDirection.magnitude > 0.1f)
            {
                Vector3 playerPos = transform.position;
                Vector3 aimPoint = playerPos + new Vector3(AimDirection.x, AimDirection.y, 0) * 10f;
                MousePosition = Camera.main.WorldToScreenPoint(aimPoint);
                //Debug.Log("Entrada de mando: Dirección del stick derecho = " + AimDirection + ", Punto de apuntado en pantalla = " + MousePosition);
            }
            else
            {
                Debug.Log("Stick derecho en zona muerta, manteniendo última dirección: " + AimDirection);
            }
        }
    }

    public void OnOptionsInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OptionsInput = true;
            OptionsInputStop = false;
        }
        if (context.canceled)
        {
            OptionsInputStop = true;
        }
    }

    public void OnUpgradesInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            UpgradesInput = true;
            UpgradesInputStop = false;
        }
        if (context.canceled)
        {
            UpgradesInputStop = true;
        }
    }

    public void OnSeparateInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SeparateInput = true;
            SeparateInputStop = false;
        }
        if (context.canceled)
        {
            SeparateInputStop = true;
            SeparateInput = false;
        }
    }

    public void OnInteractInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InteractInput = true;
            InteractInputStop = false;
        }
        if (context.canceled)
        {
            InteractInputStop = true;
            InteractInput = false;
        }
    }

    public void OnMagneticInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            MagneticInput = true;
            MagneticInputStop = false;
        }
        else if (context.canceled)
        {
            MagneticInput = false;
            MagneticInputStop = true;
        }
    }

    public void OnSwitchPolarityInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SwitchPolarityInput = true;
        }
    }
    #endregion

    #region Input Handlers - UI
    public void OnNavigateInput(InputAction.CallbackContext context)
    {
        NavigateInput = context.ReadValue<Vector2>();
        //Debug.Log("Entrada de navegación UI: " + NavigateInput);
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
    #endregion

    #region Input Management
    private void CheckJumpInputHoldTime()
    {
        if (Time.time >= jumpInputStartTime + inputHoldTime)
        {
            JumpInput = false;
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

    public void UseJumpInput() => JumpInput = false;
    public void UseThrowInput() => ThrowInput = false;
    public void UseMagneticInput() => MagneticInput = false;
    public void UseSeparateInput() => SeparateInput = false;
    public void UseInteractInput() => InteractInput = false;
    public void UseOptionsInput() => OptionsInput = false;
    public void UseUpgradesInput() => UpgradesInput = false;
    public void UseSwitchPolarityInput() => SwitchPolarityInput = false;
    public void UseSubmitInput() => SubmitInput = false;
    public void UseCancelInput() => CancelInput = false;
    public void UseQInput() => QInput = false;
    public void UseRInput() => RInput = false;

    private void ResetInputs()
    {
        JumpInput = false;
        ThrowInput = false;
        MagneticInput = false;
        SeparateInput = false;
        InteractInput = false;
        OptionsInput = false;
        UpgradesInput = false;
        SwitchPolarityInput = false;
        SubmitInput = false;
        CancelInput = false;
        NavigateInput = Vector2.zero;
        Debug.Log("Todos los inputs reseteados");
    }
    #endregion

    #region UIController
    public void OnPause()
    {
        isPaused = true;
        ResetInputs();
        SwitchToUIInput();
        //Debug.Log("Juego pausado, cambiado a mapa de entrada UI");
    }

    public void OnGame()
    {
        isPaused = false;
        ResetInputs();
        SwitchToGameplayInput();
        //Debug.Log("Juego reanudado, cambiado a mapa de entrada Gameplay");
    }

    private void SwitchToGameplayInput()
    {
        if (input != null)
        {
            input.SwitchCurrentActionMap("Gameplay");
            //Debug.Log("Cambiado al mapa de entrada Gameplay");
        }
        else
        {
            Debug.LogWarning("No se puede cambiar al mapa Gameplay: input es null");
        }
    }

    private void SwitchToUIInput()
    {
        if (input != null)
        {
            input.SwitchCurrentActionMap("UI");
            //Debug.Log("Cambiado al mapa de entrada UI");
        }
        else
        {
            Debug.LogWarning("No se puede cambiar al mapa UI: input es null");
        }
    }
    #endregion
}
