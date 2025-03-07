using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputHadler : MonoBehaviour
{
    public Vector2 RawMovementInput {  get; private set; }
    public int NormInputX { get; private set; }
    public int NormInputY { get; private set; }
    public bool JumpInput { get; private set; }
    public bool JumpInputStop {  get; private set; }
    public bool InteractInput {  get; private set; }
    public bool InteractInputStop { get; private set; }
    public bool OptionsInput { get; private set; }
    public bool OptionsInputStop { get; private set; }
    public bool GrabInput { get; private set; }  // Para agarrar objetos
    public bool GrabInputStop { get; private set; }
    public bool ThrowInput { get; private set; } // Para iniciar el lanzamiento

    public bool ThrowInputReleased { get; private set; } // Para soltar el objeto lanzado


    PlayerInput input;
    bool isPaused;

    [SerializeField]
    private float inputHoldTime = 0.2f;

    private float jumpInputStartTime;
    private float pushInputStartTime;
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
    public void OnInteractInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InteractInput = true;
            InteractInputStop = false;
            pushInputStartTime = Time.time;
        }
        if (context.canceled)
        {
            InteractInput = false;
        }
    }
    public void UseInteractInput()=> InteractInput = false;
    public void OnGrabInput(InputAction.CallbackContext context)
    {
        if (context.started)  // Cuando presionas el botón
        {
            GrabInput = true;   // Activamos el agarre
            Debug.Log("Botón de agarre presionado");
        }
         if (context.canceled)  // Cuando sueltas el botón
        {
            GrabInput = false;  // Desactivamos el agarre
            Debug.Log("Botón de agarre soltado");
        }

    }
    public void OnThrowInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ThrowInput = true;
            Debug.Log("Botón de agarre soltado");
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
        if(Time.time >= pushInputStartTime + inputHoldTime)
        {
            InteractInput= false;
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
    public void OnMouseInput()
    {
       
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
