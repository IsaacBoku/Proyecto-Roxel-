using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRopeState : PlayerGroundedState
{
    public bool isConnected = false;

    public Vector2 throwDirection;
    public PlayerRopeState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();
        if (player.springJoints != null)
            player.springJoints.enabled = false;
    }

    public override void Enter()
    {
        base.Enter();

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        throwDirection = (mousePos - (Vector2)player.rb.transform.position).normalized;

        if (player.lineRenderer != null)
        {
            player.lineRenderer.positionCount = 2;
            player.lineRenderer.SetPosition(0, player.playerCheck.position);
            player.lineRenderer.SetPosition(1, player.rb.transform.position);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();


    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnergySlot"))
        {
            ConnectRope(collision.transform.position);
        }
    }
    public void ConnectRope(Vector2 targetPos)
    {
        if (player.springJoints == null)
        {
            Debug.LogWarning("No se encontró SpringJoint2D asignado.");
            return;
        }

        // Activa y configura el SpringJoint2D
        player.springJoints.enabled = true;
        player.springJoints.connectedAnchor = targetPos;
        // Ajusta las propiedades del resorte según lo necesites
        player.springJoints.frequency = 2f;
        player.springJoints.dampingRatio = 0.5f;

        isConnected = true;
        Debug.Log("Cuerda conectada al Energy Slot.");
    }
    public void DisconnectRope()
    {
        if (player.springJoints != null)
        {
            player.springJoints.enabled = false;
        }
        isConnected = false;
        Debug.Log("Cuerda desconectada. ¡Avanza!");
    }
}
