using UnityEngine;
using System;

public class PlayerAimBatteryState : PlayerState
{
    private Vector2 throwDirection;
    private bool isAiming;

    [SerializeField] private float batteryGravity = 1f; 

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
        player.battery.transform.parent = null;
        Rigidbody2D rb = player.battery.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; 
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f; 
        player.isSeparated = true;
        player.SetVelocityX(0);

        dots = player.AimDots; 
        DotsActive(true);
        player.IsLifeProgressPaused = true;
        Debug.Log("Entrando en ThrowState - Apuntando con clic izquierdo");
    }

    public override void Exit()
    {
        base.Exit();

        DotsActive(false);
        player.IsLifeProgressPaused = false;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (isAiming && player.InputHandler.ThrowInput) 
        {
            throwDirection = AimDirection().normalized;

            player.battery.transform.position = player.transform.position + (Vector3)(throwDirection * 0.5f);

            for (int i = 0; i < dots.Length; i++)
            {
                dots[i].transform.position = DotsPosition(i * spaceBetweenDots);

                float scale = 1f - (i * 0.05f); 
                dots[i].transform.localScale = Vector3.one * Mathf.Max(0.3f, scale);

                SpriteRenderer sr = dots[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = new Color(1f, 1f, 1f, 1f - (i * 0.1f));
                }
            }
        }

        if (player.InputHandler.ThrowInputStop && isAiming) 
        {
            isAiming = false;
            Rigidbody2D rb = player.battery.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = throwDirection * playerData.throwForce; 
            rb.gravityScale = 1f;
            AudioManager.instance.PlaySFX("BatteryThrow");
            player.InputHandler.UseThrowInput();
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
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(player.InputHandler.MousePosition);
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
        Vector2 velocity = throwDirection * playerData.throwForce;
        Vector2 position = (Vector2)player.transform.position + (velocity * t)
            + 0.5f * (Physics2D.gravity * batteryGravity) * (t * t);

        float airResistance = 0.99f; 
        position *= Mathf.Pow(airResistance, t);

        return position;
    }
}
