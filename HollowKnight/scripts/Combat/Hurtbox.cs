using Godot;

public partial class Hurtbox : Area2D, IDamageable
{
    [Signal] public delegate void ReceivedDamageEventHandler(int amount, Vector2 knockback);

    [Export] public DamageTeam Team { get; set; } = DamageTeam.Neutral;
    [Export] public float InvincibilityTime { get; set; } = 0.45f;

    private HealthComponent _health;
    private InvincibilityComponent _invincibility;

    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = true;
        CollisionLayer = CollisionLayers.Hurtbox;
        CollisionMask = 0;

        _health = GetParent()?.GetNodeOrNull<HealthComponent>("HealthComponent");
        _invincibility = GetParent()?.GetNodeOrNull<InvincibilityComponent>("InvincibilityComponent");

        if (GetChildCount() == 0)
        {
            var shape = new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(14, 22) } };
            AddChild(shape);
        }
    }

    public void Bind(HealthComponent health, InvincibilityComponent invincibility)
    {
        _health = health;
        _invincibility = invincibility;
    }

    public void TakeDamage(DamageInfo info)
    {
        if (info.Team == Team || (_invincibility?.IsInvincible ?? false))
            return;

        _health?.ApplyDamage(info.Amount);
        _invincibility?.Start(InvincibilityTime);

        if (GetParent() is ICombatFeedback feedback)
            feedback.OnDamageReceived(info);

        EmitSignal(SignalName.ReceivedDamage, info.Amount, info.Knockback);
    }
}

