using Godot;

public partial class PlayerJumpState : PlayerStateBase
{
    public PlayerJumpState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        Player.Animation.SetState(PlayerStateNames.Jump);
    }

    public override void Update(double delta)
    {
        if (Motor.IsDashing)
        {
            Machine.ChangeState(PlayerStateNames.Dash);
            return;
        }

        if (Player.IsPerformingAttack)
        {
            Machine.ChangeState(PlayerStateNames.Attack);
            return;
        }

        if (Player.IsOnFloor())
            TransitionToMovementState();
    }
}