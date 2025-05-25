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
        if (player.InputHandler.InteractInput && player.CheckInteraction())
        {
            Collider2D obj = Physics2D.OverlapCircle(player.interactionCheck.position, playerData.interactionRadius, playerData.whatIsInteractable);
            if (obj != null)
            {
                Debug.Log($"PlayerInteractionState: Intentando interactuar con {obj.name}. Tiene batería: {(player.battery != null ? "Sí" : "No")}");

                Lever_Mechanic lever = obj.GetComponent<Lever_Mechanic>();
                if (lever != null)
                {
                    Debug.Log($"PlayerInteractionState: Interactuando con palanca {obj.name}.");
                    lever.Interact();
                    AudioManager.instance.PlaySFX("Interact");
                    player.InputHandler.UseInteractInput();
                    stateMachine.ChangeState(player.IdleState);
                    return;
                }

                ChargeableObject chargeable = obj.GetComponent<ChargeableObject>();
                if (chargeable != null)
                {
                    if (player.battery == null)
                    {
                        Debug.Log($"PlayerInteractionState: No se puede interactuar con {obj.name}. Necesitas una batería.");
                        chargeable.Interact();
                        player.InputHandler.UseInteractInput();
                        stateMachine.ChangeState(player.IdleState);
                        return;
                    }

                    BatteryController battery = player.battery.GetComponent<BatteryController>();
                    if (battery == null)
                    {
                        Debug.LogWarning($"PlayerInteractionState: El objeto batería en {player.battery.name} no tiene BatteryController.");
                        chargeable.Interact();
                        player.InputHandler.UseInteractInput();
                        stateMachine.ChangeState(player.IdleState);
                        return;
                    }

                    Debug.Log($"PlayerInteractionState: Iniciando carga en {obj.name} con batería {player.battery.name}.");
                    chargeable.StartCharging(battery);
                    AudioManager.instance.PlaySFX("Interact");
                    player.InputHandler.UseInteractInput();
                    stateMachine.ChangeState(player.IdleState);
                    return;
                }

               /* BatteryCharger charger = obj.GetComponent<BatteryCharger>();
                if (charger != null)
                {
                    if (player.battery == null)
                    {
                        Debug.Log($"PlayerInteractionState: No se puede interactuar con {obj.name}. Necesitas una batería para recargar.");
                        player.InputHandler.UseInteractInput();
                        stateMachine.ChangeState(player.IdleState);
                        return;
                    }

                    Debug.Log($"PlayerInteractionState: Iniciando recarga en {obj.name}.");
                    charger.StartCharging();
                    AudioManager.instance.PlaySFX("Interact");
                    player.InputHandler.UseInteractInput();
                    stateMachine.ChangeState(player.IdleState);
                    return;
                }*/

                Debug.Log($"PlayerInteractionState: El objeto {obj.name} no es una palanca, ChargeableObject, ni BatteryCharger.");
            }
            else
            {
                Debug.Log("PlayerInteractionState: No se detectó ningún objeto interactuable.");
            }

            player.InputHandler.UseInteractInput();
            stateMachine.ChangeState(player.IdleState);
        }
        else
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }
}
