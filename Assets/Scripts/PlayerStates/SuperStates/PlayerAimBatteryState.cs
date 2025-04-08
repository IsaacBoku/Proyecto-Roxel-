using UnityEngine;
using System;

public class PlayerAimBatteryState : PlayerState
{
    private Vector2 throwDirection;
    private bool isAiming;

    [SerializeField] private float batteryGravity = 1f; // Gravedad para la simulación de la trayectoria

    [Header("Aim Dots")]
    [SerializeField] private float spaceBetweenDots = 0.1f;

    private GameObject[] dots;

    public PlayerAimBatteryState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        isAiming = true;
        player.battery.transform.parent = null; // Separa la batería
        Rigidbody2D rb = player.battery.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Mantiene la batería en posición mientras apuntas
        rb.linearVelocity = Vector2.zero; // Resetea la velocidad
        rb.gravityScale = 0f; // Desactiva la gravedad mientras apuntas
        player.isSeparated = true;

        // Activa los puntos
        dots = player.AimDots; // Obtiene los puntos generados por Player
        DotsActive(true);
        player.IsTimerPaused = true; // Pausa el temporizador
        Debug.Log("Entrando en ThrowState - Apuntando con clic izquierdo");
    }

    public override void Exit()
    {
        base.Exit();

        DotsActive(false);
        player.IsTimerPaused = false;// Desactiva los puntos al salir
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (isAiming && player.InputHadler.ThrowInput) // Mientras mantienes clic izquierdo
        {
            // Calcula la dirección desde el jugador hacia el ratón
            throwDirection = AimDirection().normalized;

            // Posiciona la batería cerca del jugador mientras apuntas
            player.battery.transform.position = player.transform.position + (Vector3)(throwDirection * 0.5f);

            // Posiciona los puntos para simular la trayectoria
            for (int i = 0; i < dots.Length; i++)
            {
                dots[i].transform.position = DotsPosition(i * spaceBetweenDots);

                // Mejora 1: Escalado de puntos (más pequeños a medida que se alejan)
                float scale = 1f - (i * 0.05f); // Reduce el tamaño progresivamente
                dots[i].transform.localScale = Vector3.one * Mathf.Max(0.3f, scale); // Mínimo 0.3 para que no desaparezca

                // Mejora 2: Color gradiente (desvanecimiento)
                SpriteRenderer sr = dots[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color(1f, 1f, 1f, 1f - (i * 0.1f)); // Desvanece la opacidad
                }
            }
        }

        if (player.InputHadler.ThrowInputStop && isAiming) // Al soltar clic izquierdo
        {
            isAiming = false;
            Rigidbody2D rb = player.battery.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = throwDirection * playerData.throwForce; // Usa throwDirection directamente
            rb.gravityScale = 1f; // Restaura la gravedad
            player.InputHadler.UseThrowInput();
            player.ResetTimer();
            stateMachine.ChangeState(player.IdleState);
            Debug.Log("Lanzando batería con clic izquierdo");
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
    public Vector2 AimDirection()
    {
        Vector2 playerPosition = player.transform.position;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(player.InputHadler.MousePosition);
        Vector2 direction = mousePosition - playerPosition;
        return direction;
    }

    public void DotsActive(bool _isActive)
    {
        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].SetActive(_isActive);
        }
    }

    private Vector2 DotsPosition(float t)
    {
        Vector2 velocity = throwDirection * playerData.throwForce; // Velocidad inicial
        Vector2 position = (Vector2)player.transform.position + (velocity * t)
            + 0.5f * (Physics2D.gravity * batteryGravity) * (t * t);

        // Opcional: Añadir resistencia del aire (simulada)
        float airResistance = 0.99f; // Factor de resistencia (ajusta según necesidad)
        position *= Mathf.Pow(airResistance, t);

        return position;
    }
}
