using UnityEngine;
using System.Collections;

public class PlayerMagneticState : PlayerState
{
    private const float MinForceThreshold = 0.1f; // Umbral mínimo para aplicar fuerza
    private const float MaxSpeedX = 10f; // Velocidad máxima en el eje X

    public PlayerMagneticState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        AudioManager.instance.PlaySFX("MagneticField");
    }

    public override void Exit()
    {
        base.Exit();
        AudioManager.instance.StopSFX("MagneticField");
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.InputHandler.MagneticInput)
        {
            ApplyMagneticForce();
            player.SetVelocityX(0);
        }
        else if (player.InputHandler.MagneticInputStop)
        {
            stateMachine.ChangeState(player.IdleState);
            player.SetVelocityX(0);
            player.InputHandler.UseMagneticInput();
        }
    }

    private void ApplyMagneticForce()
    {
        // Detectar objetos metálicos dentro del rango magnético
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(player.transform.position, playerData.magneticRange, playerData.whatIsMetallic);
        if (nearbyObjects.Length == 0) return;

        bool isPositivePolarity = player.battery.GetComponent<BatteryController>().isPositivePolarity;

        foreach (Collider2D col in nearbyObjects)
        {
            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            // Calcular dirección solo en el eje X
            float directionX = player.transform.position.x - col.transform.position.x;
            Vector2 direction = new Vector2(directionX, 0f).normalized;

            // Calcular factor de distancia para modular la fuerza
            float distance = Vector2.Distance(player.transform.position, col.transform.position);
            float distanceFactor = Mathf.Clamp01(1f - (distance / playerData.magneticRange));

            // Calcular fuerza magnética (positiva = atracción, negativa = repulsión)
            float forceMagnitude = (isPositivePolarity ? playerData.magneticForce : -playerData.magneticForce) * distanceFactor;

            // Evitar fuerzas insignificantes
            if (Mathf.Abs(forceMagnitude) < MinForceThreshold) continue;

            // Aplicar fuerza solo en el eje X
            rb.AddForce(direction * forceMagnitude);

            // Limitar velocidad en el eje X
            float currentSpeedX = rb.linearVelocity.x;
            if (Mathf.Abs(currentSpeedX) > MaxSpeedX)
            {
                rb.linearVelocity = new Vector2(Mathf.Sign(currentSpeedX) * MaxSpeedX, rb.linearVelocity.y);
            }

            // Notificar al objeto metálico
            MetallicObject metallic = col.GetComponent<MetallicObject>();
            if (metallic != null)
            {
                metallic.OnMagneticForceApplied(isPositivePolarity);
            }
        }
    }
}
