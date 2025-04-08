using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteractionState : PlayerState
{
    public PlayerInteractionState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (player.InputHadler.InteractInput && player.CheckInteraction())
        {
            Collider2D obj = Physics2D.OverlapCircle(player.InteractionCheck.position, playerData.interactionRadius, playerData.whatIsInteractable);
            if (obj != null && player.battery != null)
            {
                BatteryController battery = player.battery.GetComponent<BatteryController>();

                // Intentar cargar un ChargeableObject
                ChargeableObject chargeable = obj.GetComponent<ChargeableObject>();
                if (chargeable != null && battery.energyAmounts > 0)
                {
                    float consumedEnergy = chargeable.ReceiveEnergy(battery.energyAmounts);
                    if (consumedEnergy > 0)
                    {
                        battery.energyAmounts -= consumedEnergy;
                        player.InputHadler.UseInteractInput();
                        Debug.Log($"Energía transferida: {consumedEnergy}. Energía restante: {battery.energyAmounts}");
                    }
                }

                // Intentar recargar la batería
                BatteryCharger charger = obj.GetComponent<BatteryCharger>();
                if (charger != null)
                {
                    charger.StartCharging();
                    player.InputHadler.UseInteractInput();
                }
            }
            stateMachine.ChangeState(player.IdleState);
        }
        else if (!player.InputHadler.InteractInput)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }
}
