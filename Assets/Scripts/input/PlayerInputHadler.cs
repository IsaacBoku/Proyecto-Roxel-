using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerInputHadler : MonoBehaviour
{
    #region InputsVariables
    public Vector2 RawMovementInput {  get; private set; }
    public int NormInputX { get; private set; }
    public int NormInputY { get; private set; }
    public bool JumpInput { get; private set; }
    public bool JumpInputStop {  get; private set; }
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

    #endregion
    #region Unity CallBack
    PlayerInput input;

    bool isPaused;

    [SerializeField]
    private float inputHoldTime = 0.2f;

    private float jumpInputStartTime;
    private void Start()
    {
        input = GetComponent<PlayerInput>();

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
    }
    private void Update()
    {
        CheckJumpInputHoldTime();
    }
    #endregion
    #region Input
    public void OnMoveInput(InputAction.CallbackContext context)
    {
        RawMovementInput = context.ReadValue<Vector2>();

        NormInputX = (int)(RawMovementInput*Vector2.right).normalized.x;
        NormInputY = (int)(RawMovementInput*Vector2.up).normalized.y;
    }
    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            JumpInput = true;
            JumpInputStop = false;
            jumpInputStartTime = Time.time;
        }
        if(context.canceled)
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
            // Mouse input: Store screen position
            MousePosition = inputValue;
            // Convert screen position to world aim direction
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(inputValue.x, inputValue.y, Camera.main.nearClipPlane));
            AimDirection = (worldPos - transform.position).normalized;
            Debug.Log("Entrada de ratón: Posición en pantalla = " + MousePosition + ", Dirección de apuntado = " + AimDirection);
        }
        else if (controlDevice is Gamepad)
        {
            // Gamepad input: Use right stick direction
            AimDirection = inputValue.normalized;
            if (AimDirection.magnitude > 0.1f) // Apply dead zone to avoid jitter
            {
                // Calculate a world aim point based on stick direction
                Vector3 playerPos = transform.position;
                Vector3 aimPoint = playerPos + new Vector3(AimDirection.x, AimDirection.y, 0) * 10f; // Arbitrary distance
                MousePosition = Camera.main.WorldToScreenPoint(aimPoint); // Store as screen position for compatibility
                Debug.Log("Entrada de mando: Dirección del stick derecho = " + AimDirection + ", Punto de apuntado en pantalla = " + MousePosition);
            }
            else
            {
                // If stick is in dead zone, keep last valid aim direction
                Debug.Log("Stick derecho en zona muerta, manteniendo última dirección: " + AimDirection);
            }
        }
    }

    private void CheckJumpInputHoldTime()
    {
        if (Time.time >= jumpInputStartTime + inputHoldTime)
        {
            JumpInput = false;
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
    #region UseInput
    public void UseJumpInput() => JumpInput = false;
    public void UseThrowInput() => ThrowInput = false;
    public void UseMagneticInput() => MagneticInput = false;
    public void UseSeparateInput() => SeparateInput = false;
    public void UseInteractInput() => InteractInput = false;
    public void UseOptionsInput() => OptionsInput = false;
    public void UseUpgradesInput() => UpgradesInput = false;
    public void UseSwitchPolarityInput() => SwitchPolarityInput = false;

    #endregion
    #region UIController
    public void OnPause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            input.enabled = false;
        }
        else
        {
            input.enabled = true;
        }
    }
    public void OnGame()
    {
        isPaused = !isPaused;
        if (!isPaused)
        {
            input.enabled = true;
        }
    }
    #endregion
}
