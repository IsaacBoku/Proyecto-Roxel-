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
                heldRb = heldObject.GetComponent<Rigidbody2D>();

                if (heldRb != null)
                {
                    heldRb.isKinematic = true;  // Evita que se caiga
                    heldRb.velocity = Vector2.zero;
                    heldRb.angularVelocity = 0f;
                }

                heldObject.transform.parent = holdPosition.transform;
                heldObject.transform.localPosition = new Vector3(0, 1f, 0);

                // Ignorar colisiones con el jugador para evitar bloqueos
                Physics2D.IgnoreCollision(heldObject.GetComponent<Collider2D>(), player.GetComponent<Collider2D>(), true);

                // Reducir velocidad
                player.originalSpeed = playerData.movementVeclocity;
                playerData.movementVeclocity *= 0.5f;
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
                rb.isKinematic = false;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            heldObject.transform.parent = null;

            // Restaurar colisión con el jugador
            Physics2D.IgnoreCollision(heldObject.GetComponent<Collider2D>(), player.GetComponent<Collider2D>(), false);

            heldObject = null;
            heldRb = null;

            // Restaurar velocidad
            playerData.movementVeclocity = player.originalSpeed;
        }
    }

    public void ThrowObject()
    {
        if (heldObject != null && heldRb != null)
        {
            heldRb.isKinematic = false;
            heldObject.transform.SetParent(null);
            heldRb.AddForce(player.transform.right * throwForce, ForceMode2D.Impulse);

            heldObject.transform.parent = null;
            // Restaurar colisión con el jugador
            Physics2D.IgnoreCollision(heldObject.GetComponent<Collider2D>(), player.GetComponent<Collider2D>(), false);

            heldObject = null;
            heldRb = null;

            // Restaurar velocidad original
            playerData.movementVeclocity = player.originalSpeed;
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
