using UnityEngine;
using System.Collections;

public class PlayerMagneticState : PlayerState
{

    public PlayerMagneticState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName)
        : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (player.InputHadler.MagneticInput)
        {
            ApplyMagneticForce(true); // Solo atracción
        }
        else if (player.InputHadler.MagneticInputStop)
        {
            stateMachine.ChangeState(player.IdleState);
            player.InputHadler.UseMagneticInput();
        }

    }

    private void ApplyMagneticForce(bool attract)
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(player.transform.position, playerData.magneticRange, playerData.whatIsMetallic);
        foreach (Collider2D col in nearby)
        {
            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
            Vector2 direction = ((Vector2)player.transform.position - (Vector2)col.transform.position).normalized;
            float force = attract ? playerData.magneticForce : -playerData.magneticForce;
            rb.AddForce(direction * force);
        }
    }
}
