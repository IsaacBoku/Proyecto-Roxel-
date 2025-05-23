using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerAbilityState
{
    private int amountOfJumpsLeft;
    public PlayerJumpState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
        amountOfJumpsLeft = playerData.amountOfJumps;
    }

    public override void Enter()
    {
        base.Enter();

        player.SetVelocityY(playerData.jumpVelocity);
        isAbilityDone = true;
        DecreaseAmountOfJUmpsLeft();
        player.AirState.SetIsJumping();
        AudioManager.instance.PlaySFX("Jump");
    }


    public bool CanJump()
    {
        if (amountOfJumpsLeft > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void ResetAmountOfJumpsLeft()
    {
        if (player.CheckIfGrounded())
        {
            amountOfJumpsLeft = playerData.amountOfJumps; 
        }
    }
    public void DecreaseAmountOfJUmpsLeft() => amountOfJumpsLeft--;
}
