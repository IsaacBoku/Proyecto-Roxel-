using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteractionState : PlayerGroundedState
{
    public GameObject heldObject;
    private Rigidbody2D heldRb;
    private Transform holdPosition;
    private float throwForce;
    private bool isChargingThrow;
    private bool isInteraction;
    public PlayerInteractionState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
        holdPosition = player.ObjectPosition;
        throwForce = playerData.throwForce;
    }

    public override void DoChecks()
    {
        base.DoChecks();
        isInteraction = player.CheckInteraction();
    }

    public override void Enter()
    {
        base.Enter();
        PickUpObject();
    }

    public override void Exit()
    {
        base.Exit();

    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        stateMachine.ChangeState(player.IdleState);
       
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public void PickUpObject()
    {
        Collider2D detectedObject = Physics2D.OverlapCircle(player.InteractionCheck.position, playerData.interactionRadius, playerData.whatIsInteractable);

        if (detectedObject != null)
        {
            Debug.Log("Objeto detectado: " + detectedObject.name);
            player.ObjectInteraction = detectedObject.gameObject;
            heldObject = player.ObjectInteraction;
            heldRb = heldObject.GetComponent<Rigidbody2D>();

            if (heldRb != null)
            {
                heldRb.isKinematic = true;
                heldObject.transform.SetParent(holdPosition); // Usamos el punto de agarre
                heldObject.transform.localPosition = Vector3.zero; // Ajustamos la posición del objeto
                Debug.Log("Objeto agarrado correctamente");
            }
            else
            {
                Debug.Log("El objeto no tiene Rigidbody2D");
            }
        }
        else
        {
            Debug.Log("No hay objeto dentro del radio de interacción");
        }
    }
    public void DropObject()
    {
        if (heldRb != null)
        {
            heldRb.isKinematic = false;  // Reactiva la física cuando lo sueltas
            heldObject.transform.SetParent(null);  // Lo desacoplas del player
            heldObject = null;  // Lo quitas de las variables de seguimiento

            Debug.Log("Objeto soltado correctamente");
        }
    }

    public void ThrowObject()
    {
        if (heldRb != null)
        {
            heldRb.GetComponent<FixedJoint2D>().enabled = false;
            heldRb.isKinematic = false;
            heldObject.transform.SetParent(null);
            heldRb.AddForce(player.transform.right * throwForce, ForceMode2D.Impulse);
            heldObject = null;
        }
    }
}
