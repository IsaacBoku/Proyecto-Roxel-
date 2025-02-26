using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerGroundedState : PlayerState
{
    protected int xInput;
    private bool JumpInput;
    private bool InteractInput;
    private bool isGrounded;
    public PlayerGroundedState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();

        isGrounded = player.CheckIfGrounded();
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

        /*if(InteractInput && player.PushState.CanPush())
        {
            player.InputHadler.UseInteractInput();
            stateMachine.ChangeState(player.PushState);
            Debug.Log(player.PushState.isPush);
        }
        else if (!player.PushState.isPush && xInput == 0)
        {
            stateMachine.ChangeState(player.IdleState);
        }
        else if(xInput != 0)
        {
            stateMachine.ChangeState(player.MoveState);
        }*/
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

        // --- DESCONEXIÓN ---
        // Si se presiona el input de salto y la pila está conectada, se desconecta la cuerda
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
