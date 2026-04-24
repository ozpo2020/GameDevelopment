using Godot;

public partial class PlayerIdleState : PlayerStateBase
{
    public PlayerIdleState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        Player.Animation.SetState(PlayerStateNames.Idle);
    }

    public override void Update(double delta)
    {
        if (Player.IsPerformingAttack)
        {
            Machine.ChangeState(PlayerStateNames.Attack);
            return;
        }

        if (Motor.IsDashing)
        {
            Machine.ChangeState(PlayerStateNames.Dash);
            return;
        }

        if (Input.IsActionJustPressed(InputBindings.Dash) && Motor.CanDash)
        {
            Machine.ChangeState(PlayerStateNames.Dash);
            return;
        }

        if (Input.IsActionJustPressed(InputBindings.Jump))
        {
            Machine.ChangeState(PlayerStateNames.Jump);
            return;
        }

        if (!Player.IsOnFloor())
        {
            Machine.ChangeState(PlayerStateNames.Jump);
            return;
        }

        if (HasMoveInput())
            Machine.ChangeState(PlayerStateNames.Run);
    }
}