using Godot;

public abstract partial class EnemyBase : EntityBase
{
    [Signal] public delegate void DefeatedEventHandler(string enemyId);

    [Export] public int ContactDamage { get; set; } = 1;
    [Export] public float ContactKnockback { get; set; } = 185.0f;

    public PlayerController Player { get; set; }

    protected float AnimTime;
    private double _touchCooldown;

    public override void _PhysicsProcess(double delta)
    {
        AnimTime += (float)delta;
        TickFlash(delta);
        _touchCooldown = Mathf.Max(0.0, _touchCooldown - delta);

        var desired = ComputeVelocity(delta);
        Velocity = desired + Knockback.Tick(delta);
        MoveAndSlide();
        TryTouchDamage();
        QueueRedraw();
    }

    protected abstract Vector2 ComputeVelocity(double delta);

    protected override void OnDied()
    {
        EmitSignal(SignalName.Defeated, EntityId);
        base.OnDied();
    }

    private void TryTouchDamage()
    {
        if (Player == null || _touchCooldown > 0.0 || GlobalPosition.DistanceTo(Player.GlobalPosition) > 18.0f)
            return;

        var direction = (Player.GlobalPosition - GlobalPosition).Normalized();
        var playerHurtbox = Player.GetNodeOrNull<Hurtbox>("Hurtbox");
        playerHurtbox?.TakeDamage(new DamageInfo(ContactDamage, direction * ContactKnockback, this, DamageTeam.Enemy));
        _touchCooldown = 0.72;
    }
}
