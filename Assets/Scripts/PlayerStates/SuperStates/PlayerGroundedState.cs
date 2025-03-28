using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerGroundedState : PlayerState
{
    protected int xInput;
    private bool JumpInput;
    private bool InteractInput;
    private bool GrabInput;
    private bool ThrowInput;
    private bool isGrounded;
    private bool isInteraction;

    private bool grabInputReleased = false;
    public PlayerGroundedState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();

        isGrounded = player.CheckIfGrounded();
        isInteraction = player.CheckInteraction();
    }

    public override void Enter()
    {
        base.Enter();
        player.JumpState.ResetAmountOfJumpsLeft();
    }

    public override void Exit()
    {
        base.Exit();
        
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        xInput = player.InputHadler.NormInputX;
        JumpInput = player.InputHadler.JumpInput;
        InteractInput = player.InputHadler.InteractInput;
        GrabInput = player.InputHadler.GrabInput;
        ThrowInput = player.InputHadler.ThrowInput;

        // Solo procesar la acci�n si el input fue liberado antes
        if (player.InputHadler.GrabInput && grabInputReleased)
        {
            grabInputReleased = false; // Bloquear la acci�n hasta que se libere el bot�n

            if (player.InteractionState.heldObject != null)
            {
                player.InteractionState.DropObject();
                stateMachine.ChangeState(player.IdleState);
            }
            else if (isInteraction)
            {
                player.InteractionState.PickUpObject();
                AudioManager.instance.PlaySFX("Grab");
                stateMachine.ChangeState(player.InteractionState);
            }
        }

        // Si el jugador suelta el bot�n, permitir que lo vuelva a usar
        if (!player.InputHadler.GrabInput)
        {
            grabInputReleased = true;
        }

        // Si el jugador quiere lanzar el objeto y est� sosteni�ndolo
        if (ThrowInput && player.InteractionState.heldObject != null)
        {
            player.InteractionState.ThrowObject();
            stateMachine.ChangeState(player.IdleState);  // Cambia al estado idle despu�s de lanzar el objeto
        }

        if (JumpInput && player.JumpState.CanJump() && isGrounded)
        {
            player.InputHadler.UseJumpInput();
            stateMachine.ChangeState(player.JumpState);
        }
        else if (!isGrounded)
        {
            player.AirState.StartCoyoteTime();
            stateMachine.ChangeState(player.AirState);
        }
        if (player.CheckIfGrounded())
        {
            player.JumpState.ResetAmountOfJumpsLeft();  // Resetea los saltos cuando el jugador toque el suelo
        }

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

}
