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
        if (amountOfJumpsLeft > 0) // Solo permite saltar si no está sosteniendo un objeto
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
        if (player.CheckIfGrounded())  // Si el jugador está tocando el suelo
        {
            amountOfJumpsLeft = playerData.amountOfJumps;  // Restablece los saltos
        }
    }
    public void DecreaseAmountOfJUmpsLeft() => amountOfJumpsLeft--;
}
