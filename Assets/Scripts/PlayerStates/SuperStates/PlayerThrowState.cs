using UnityEngine;

public class PlayerThrowState : PlayerState
{
    private Vector2 throwDirection;
    public PlayerThrowState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) 
        : base(player, stateMachine, playerData, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        throwDirection = new Vector2(player.FacingDirection, 0.5f).normalized;
        player.battery.transform.parent = null;
        Rigidbody2D rb = player.battery.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = throwDirection * playerData.throwForce;
        player.isSeparated = true;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (!player.InputHandler.ThrowInput)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }
}
