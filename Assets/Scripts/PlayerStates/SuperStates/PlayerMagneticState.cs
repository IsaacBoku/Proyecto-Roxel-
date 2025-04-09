using UnityEngine;
using System.Collections;

public class PlayerMagneticState : PlayerState
{

    public PlayerMagneticState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Entrando en MagneticState");
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log("Saliendo de MagneticState");
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (player.InputHadler.MagneticInput)
        {
            ApplyMagneticForce();
        }
        else if (player.InputHadler.MagneticInputStop)
        {
            stateMachine.ChangeState(player.IdleState);
            player.InputHadler.UseMagneticInput();
        }
    }

    private void ApplyMagneticForce()
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(player.transform.position, playerData.magneticRange, playerData.whatIsMetallic);
        if (nearby.Length == 0) return;

        foreach (Collider2D col in nearby)
        {
            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            Vector2 direction = ((Vector2)player.transform.position - (Vector2)col.transform.position).normalized;
            float distance = Vector2.Distance(player.transform.position, col.transform.position);
            float distanceFactor = Mathf.Clamp01(1f - (distance / playerData.magneticRange));
            bool isPositivePolarity = player.battery.GetComponent<BatteryController>().isPositivePolarity;
            float force = (isPositivePolarity ? playerData.magneticForce : -playerData.magneticForce) * distanceFactor;

            rb.AddForce(direction * force);

            // Limita la velocidad máxima
            float maxSpeed = 10f; // Ajusta según necesidad
            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }

            MetallicObject metallic = col.GetComponent<MetallicObject>();
            if (metallic != null)
            {
                metallic.OnMagneticForceApplied(isPositivePolarity);
            }
        }
    }
}
