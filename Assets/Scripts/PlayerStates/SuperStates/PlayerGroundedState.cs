using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerGroundedState : PlayerState
{
    protected int xInput;
    private bool JumpInput;
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
            player.JumpState.ResetAmountOfJumpsLeft();  
        }

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

}
