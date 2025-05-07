using UnityEngine;
using UnityEngine.Windows;

public class PlayerSeparatedState : PlayerState
{
    protected int xInput;
    public PlayerSeparatedState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        //player.rb.mass = playerData.lightMass;
    }

    public override void Exit()
    {
        base.Exit();
        //player.rb.mass = playerData.normalMass;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        xInput = player.InputHadler.NormInputX;

        if (xInput != 0)
        {
            stateMachine.ChangeState(player.MoveState);
        }
        else
        {
            stateMachine.ChangeState(player.IdleState);
        }
        if (player.InputHadler.JumpInput && player.CheckIfGrounded())
        {
            player.SetVelocityY(playerData.jumpVelocity);
            player.InputHadler.UseJumpInput();
        }
    }
}
