using UnityEngine;

public class PlayerWithoutBatteryState : PlayerAbilityState
{
    private float timeWithoutBattery = 5f;
    private float timer;
    public PlayerWithoutBatteryState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter()
    {
        base.Enter();
        timer = timeWithoutBattery;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            player.gameObject.SetActive(false);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
