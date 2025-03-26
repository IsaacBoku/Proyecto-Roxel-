using UnityEngine;
using System.Collections;

public class PlayerMagneticState : PlayerGroundedState
{
    private float magneticForce;
    private float magneticRadius;
    private LayerMask metallicObjects;
    private bool isMagnetActive;

    public PlayerMagneticState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // Obtener valores de PlayerData
        metallicObjects = playerData.whatIsMagentic;
        magneticForce = playerData.magneticForce;
        magneticRadius = playerData.magneticRadius;

        isMagnetActive = true;
        Debug.Log("Magnetismo Activado");
    }

    public override void Exit()
    {
        base.Exit();
        isMagnetActive = false;
        Debug.Log("Magnetismo Desactivado");
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (isMagnetActive)
        {
            ApplyMagneticForce(true);

            // Verificar si hay objetos en el radio
            Collider2D[] objects = Physics2D.OverlapCircleAll(player.transform.position, magneticRadius, metallicObjects);
            Debug.Log($"Objetos detectados en el radio: {objects.Length}");

            if (objects.Length == 0)
            {
                Debug.Log("No hay objetos en el radio. Cambiando a IdleState.");
                stateMachine.ChangeState(player.IdleState);
            }
        }

    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    private void ApplyMagneticForce(bool attract)
    {
        Collider2D[] objects = Physics2D.OverlapCircleAll(player.transform.position, magneticRadius, metallicObjects);
        Debug.Log($"Objetos detectados: {objects.Length}");

        foreach (var obj in objects)
        {
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Debug.Log($"Atrayendo objeto: {obj.gameObject.name}");

                Vector2 forceDir = (attract ? player.transform.position - obj.transform.position : obj.transform.position - player.transform.position).normalized;
                rb.AddForce(forceDir * magneticForce, ForceMode2D.Impulse);
            }
        }
    }
}
