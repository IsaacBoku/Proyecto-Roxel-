using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHadler : MonoBehaviour
{
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
    public bool ThrowInputReleased { get; private set; } 
    public bool MagneticInput { get; private set; }
    public bool MagneticInputStop { get; private set; }
    public bool SeparateInput { get; private set; } // Para separar/reunir bater�a
    public bool SeparateInputStop { get; private set; }
    public bool AttractInput { get; private set; }
    public bool InteractInput { get; private set; } // Para interactuar/transferir energ�a
    public bool InteractInputStop { get; private set; }

    PlayerInput input;
    bool isPaused;

    [SerializeField]
    private float inputHoldTime = 0.2f;

    private float jumpInputStartTime;
    private void Start()
    {
        input = GetComponent<PlayerInput>();
    }
    private void Update()
    {
        CheckJumpInputHoldTime();
    }
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
    public void UseJumpInput() => JumpInput = false;
    public void OnGrabInput(InputAction.CallbackContext context)
    {
        if (context.started)  // Cuando presionas el bot�n
        {
            GrabInput = true;   // Activamos el agarre
            Debug.Log("Bot�n de agarre presionado");
        }
        else if (context.canceled)  // Cuando sueltas el bot�n
        {
            GrabInput = false;  // Desactivamos el agarre
            Debug.Log("Bot�n de agarre soltado");
        }

    }
    public void OnThrowInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ThrowInput = true;
            Debug.Log("Bot�n de agarre soltado");
        }
        else if (context.canceled)
        {
            ThrowInput = false;
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
    public void OnSeparateInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SeparateInput = true;
            SeparateInputStop = false;
            Debug.Log("Bot�n de separaci�n presionado");
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
            Debug.Log("Bot�n de interacci�n presionado");
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
            Debug.Log("MagneticInput activado - Atrayendo");
        }
        else if (context.canceled)
        {
            MagneticInput = false;
            MagneticInputStop = true;
            Debug.Log("MagneticInput desactivado - Parando");
        }
    }
    public void UseMagneticInput() => MagneticInput = false;
    public void UseSeparateInput() => SeparateInput = false;
    public void UseInteractInput() => InteractInput = false;
    public void UseOptionsInput() => OptionsInput = false;

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
}
