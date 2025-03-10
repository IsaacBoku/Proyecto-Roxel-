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
        if (heldObject == null)
        {
            Collider2D objectToPickUp = DetectObjectToPickUp();
            if (objectToPickUp != null)
            {
                heldObject = objectToPickUp.gameObject;
                Rigidbody2D rb = heldObject.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    rb.isKinematic = true;  // Desactiva la física al agarrarlo
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }

                heldObject.transform.parent = holdPosition.transform;
                heldObject.transform.localPosition = new Vector3(0, 1f, 0); // Ajusta la posición sobre la cabeza

                playerData.movementVeclocity *= 0.5f; // Reduce la velocidad al agarrar un objeto
            }
        }
    }
    public void DropObject()
    {
        if (heldObject != null)
        {
            Rigidbody2D rb = heldObject.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.isKinematic = false; // Reactiva la física
                rb.velocity = Vector2.zero; // Evita que salga disparado
                rb.angularVelocity = 0f;
            }

            heldObject.transform.parent = null;
            heldObject = null;

            playerData.movementVeclocity = 5; // Restaura la velocidad normal
        }
    }

    public void ThrowObject()
    {
        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldObject.transform.SetParent(null);
            heldRb.AddForce(player.transform.right * throwForce, ForceMode2D.Impulse);
            heldObject = null;
        }
    }

    private Collider2D DetectObjectToPickUp()
    {
        float detectionRadius = 1f; // Ajusta el radio según lo necesites
        LayerMask objectLayer = LayerMask.GetMask("Box"); // Asegúrate de que tus objetos están en esta capa

        Collider2D detectedObject = Physics2D.OverlapCircle(player.transform.position, detectionRadius, objectLayer);
        return detectedObject;
    }
}
