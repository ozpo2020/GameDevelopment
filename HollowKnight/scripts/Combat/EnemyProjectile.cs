using Godot;

public partial class EnemyProjectile : Area2D
{
    [Export] public DamageTeam Team { get; set; } = DamageTeam.Enemy;
    [Export] public int Damage { get; set; } = 1;
    [Export] public float Knockback { get; set; } = 165.0f;

    private Vector2 _velocity = Vector2.Left;
    private double _life = 2.5;
    private float _spin;
    private bool _armed = true;

    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = false;
        CollisionLayer = 0;
        CollisionMask = CollisionLayers.Hurtbox;
        AreaEntered += OnAreaEntered;

        if (GetChildCount() == 0)
        {
            var shape = new CollisionShape2D { Shape = new CircleShape2D { Radius = 5.0f } };
            AddChild(shape);
        }
    }

    public void Configure(Vector2 position, Vector2 velocity, double life = 2.5, int damage = 1, float knockback = 165.0f)
    {
        GlobalPosition = position;
        _velocity = velocity;
        _life = life;
        Damage = damage;
        Knockback = knockback;
        Rotation = velocity == Vector2.Zero ? 0.0f : velocity.Angle();
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += _velocity * (float)delta;
        _life -= delta;
        _spin += (float)delta * 10.0f;

        if (_life <= 0.0)
            QueueFree();

        QueueRedraw();
    }

    private void OnAreaEntered(Area2D area)
    {
        if (!_armed || area is not IDamageable damageable || damageable.Team == Team)
            return;

        var direction = _velocity.LengthSquared() > 0.0f ? _velocity.Normalized() : Vector2.Left;
        damageable.TakeDamage(new DamageInfo(Damage, direction * Knockback, this, Team));
        _armed = false;
        QueueFree();
    }

    public override void _Draw()
    {
        var core = new Color(0.82f, 0.96f, 1.0f);
        var glow = new Color(0.24f, 0.82f, 1.0f, 0.26f);
        var ember = new Color(1.0f, 0.58f, 0.28f, 0.70f);

        DrawCircle(Vector2.Zero, 8.0f, glow);
        DrawRect(new Rect2(new Vector2(-6, -2), new Vector2(12, 4)), core);
        DrawRect(new Rect2(new Vector2(-2, -6), new Vector2(4, 12)), core.Darkened(0.12f));
        DrawRect(new Rect2(new Vector2(-3, -3).Rotated(_spin), new Vector2(6, 2)), ember);
        DrawCircle(Vector2.Zero, 2.0f, Colors.White);
    }
}
