using Godot;

public partial class PlayerDashState : PlayerStateBase
{
    public PlayerDashState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        Player.Animation.SetState(PlayerStateNames.Dash);
    }

    public override void Update(double delta)
    {
        if (!Motor.IsDashing)
        {
            if (Player.IsPerformingAttack)
            {
                Machine.ChangeState(PlayerStateNames.Attack);
                return;
            }

            TransitionToMovementState();
        }
    }
}