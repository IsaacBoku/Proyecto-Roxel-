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

        // Si el jugador presiona el bot�n de interacci�n (E o la tecla que uses)
        if (GrabInput)
        {
            // Si ya tienes un objeto, su�ltalo
            if (player.InteractionState.heldObject != null)
            {
                player.InteractionState.DropObject();

                stateMachine.ChangeState(player.IdleState);

               // Cambia al estado idle si sueltas el objeto
            }
            // Si no est�s sosteniendo nada y hay un objeto cerca, ag�rralo
            else if (isInteraction)
            {
                player.InteractionState.PickUpObject();
                stateMachine.ChangeState(player.InteractionState);
                // Cambia al estado de interacci�n si agarras el objeto
            }
        }



        // Si el jugador quiere lanzar el objeto y est� sosteni�ndolo
        if (ThrowInput && player.InteractionState.heldObject != null)
        {
            player.InteractionState.ThrowObject();
            stateMachine.ChangeState(player.IdleState);  // Cambia al estado idle despu�s de lanzar el objeto
        }

        if (JumpInput && player.JumpState.CanJump())
        {
            player.InputHadler.UseJumpInput();
            stateMachine.ChangeState(player.JumpState);
        }
        else if (!isGrounded)
        {
            player.AirState.StartCoyoteTime();
            stateMachine.ChangeState(player.AirState);
        }
        if (InteractInput && !player.RopeState.isConnected)
        {
            player.rb.velocity = player.RopeState.throwDirection * playerData.throwForce;
            player.InputHadler.UseInteractInput();
            //stateMachine.ChangeState(player.RopeState);
        }

        // --- DESCONEXI�N ---
        // Si se presiona el input de salto y la pila est� conectada, se desconecta la cuerda
        if (InteractInput && player.RopeState.isConnected)
        {
            player.RopeState.DisconnectRope();
            player.InputHadler.UseJumpInput();
           
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }


}
