using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerGroundedState
{
    private float stepTimer;
    private float stepInterval = 0.4f;
    private bool isPlayingFootsteps = false;
    public PlayerMoveState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter()
    {
        base.Enter();
        stepTimer = 0f;
        isPlayingFootsteps = false;
    }

    public override void Exit()
    {
        base.Exit();
        if (isPlayingFootsteps)
        {
            AudioManager.instance.StopSFX("Footstep"); // Detener el sonido de pasos al salir
            isPlayingFootsteps = false;
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        player.CheckIfShouldFlip(xInput);

        player.SetVelocityX(playerData.movementVeclocity * xInput);

        // Gestionar sonido de pasos
        if (xInput != 0 && player.CheckIfGrounded())
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                if (!isPlayingFootsteps)
                {
                    AudioManager.instance.PlaySFX("Footstep"); // Reproducir sonido de pasos
                    isPlayingFootsteps = true;
                }
                stepTimer = 0f; // Reiniciar temporizador
            }
        }
        else if (isPlayingFootsteps)
        {
            AudioManager.instance.StopSFX("Footstep"); // Detener si no se mueve o no está en el suelo
            isPlayingFootsteps = false;
        }

        if (xInput == 0)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
