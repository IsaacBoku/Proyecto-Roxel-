using UnityEngine;

public class PlayerBoostState : PlayerState
{
    private float boostStartTime;
    private bool isBoosting;

    public PlayerBoostState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        BatteryController battery = player.battery.GetComponent<BatteryController>();

        if (battery.energyAmounts >= playerData.boostCost)
        {
            battery.energyAmounts -= playerData.boostCost;
            battery.energyAmounts = Mathf.Clamp(battery.energyAmounts, 0f, battery.maxEnergy);
            player.SetVelocityX(player.FacingDirection * playerData.boostSpeed);
            // Si está en el aire, reduce la velocidad vertical para un mejor control
            if (!player.CheckIfGrounded())
            {
                player.SetVelocityY(player.CurrentVelocity.y * 0.5f);
            }
            boostStartTime = Time.time;
            isBoosting = true;
        }
        else
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Termina el impulso después de la duración definida
        if (isBoosting && Time.time >= boostStartTime + playerData.boostDuration)
        {
            player.SetVelocityX(0f); // Detiene el movimiento horizontal
            isBoosting = false;
            stateMachine.ChangeState(player.IdleState);
        }

        // Permite cancelar el impulso si se suelta el input
        if (!player.InputHadler.BoostInput && isBoosting)
        {
            player.SetVelocityX(0f);
            isBoosting = false;
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.InputHadler.UseBoostInput(); // Consume el input para evitar que se detecte de nuevo
    }
}
