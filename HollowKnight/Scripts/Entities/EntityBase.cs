using Godot;

public abstract partial class EntityBase : CharacterBody2D, ICombatFeedback
{
    [Export] public string EntityId { get; set; } = "Entity";
    [Export] public int MaxHealth { get; set; } = 3;
    [Export] public float MoveSpeed { get; set; } = 56.0f;

    protected HealthComponent Health;
    protected InvincibilityComponent Invincibility;
    protected KnockbackComponent Knockback;
    protected double FlashTimer;

    public override void _Ready()
    {
        CollisionLayer = CollisionLayers.EnemyBody;
        CollisionMask = CollisionLayers.World | CollisionLayers.PlayerBody;

        AddChild(new CollisionShape2D
        {
            Shape = new RectangleShape2D { Size = new Vector2(16, 18) }
        });

        Health = new HealthComponent { Name = "HealthComponent", MaxHealth = MaxHealth };
        AddChild(Health);
        Health.Died += OnDied;

        Invincibility = new InvincibilityComponent { Name = "InvincibilityComponent" };
        AddChild(Invincibility);

        Knockback = new KnockbackComponent { Name = "KnockbackComponent", Recovery = 760.0f };
        AddChild(Knockback);

        var hurtbox = new Hurtbox { Name = "Hurtbox", Team = DamageTeam.Enemy, InvincibilityTime = 0.08f };
        AddChild(hurtbox);
        hurtbox.Bind(Health, Invincibility);
    }

    public virtual void OnDamageReceived(DamageInfo info)
    {
        Knockback.Apply(info.Knockback);
        FlashTimer = 0.12;
    }

    protected virtual void OnDied()
    {
        QueueFree();
    }

    protected void TickFlash(double delta)
    {
        FlashTimer = Mathf.Max(0.0, FlashTimer - delta);
    }
}

