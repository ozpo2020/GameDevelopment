using Godot;

public partial class PlayerHurtState : PlayerStateBase
{
    private double _hurtTimer;

    public PlayerHurtState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        Player.Animation.SetState(PlayerStateNames.Hurt);
        _hurtTimer = 0.28;
    }

    public override void PhysicsUpdate(double delta)
    {
        if (Player.Health.IsDead)
            return;

        _hurtTimer = Mathf.Max(0.0, _hurtTimer - delta);
        if (_hurtTimer > 0.0 || Player.Invincibility.IsInvincible)
            return;

        if (Player.IsOnFloor())
            TransitionToMovementState();
    }
}