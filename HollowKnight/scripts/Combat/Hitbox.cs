using System.Collections.Generic;
using Godot;

public partial class Hitbox : Area2D
{
    [Signal] public delegate void HitConfirmedEventHandler(Vector2 position);

    [Export] public DamageTeam Team { get; set; } = DamageTeam.Neutral;
    [Export] public int Damage { get; set; } = 1;
    [Export] public float Knockback { get; set; } = 140.0f;

    private readonly HashSet<ulong> _hitTargets = new();
    private Node2D _source;
    private Vector2 _direction = Vector2.Right;
    private Vector2 _drawSize = new(18, 12);
    private double _activeTimer;

    public override void _Ready()
    {
        Monitoring = false;
        Monitorable = false;
        CollisionLayer = 0;
        CollisionMask = CollisionLayers.Hurtbox;
        AreaEntered += OnAreaEntered;

        if (GetChildCount() == 0)
        {
            var shape = new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(18, 12) } };
            AddChild(shape);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_activeTimer <= 0.0)
            return;

        _activeTimer -= delta;
        if (_activeTimer <= 0.0)
            Deactivate();
    }

    public void Activate(AttackProfile profile, Node2D source, Vector2 direction)
    {
        Damage = profile.Damage;
        Knockback = profile.Knockback;
        _source = source;
        _direction = direction == Vector2.Zero ? Vector2.Right : direction.Normalized();
        _activeTimer = profile.ActiveTime;
        _hitTargets.Clear();
        Monitoring = true;
        Visible = true;
        QueueRedraw();
    }

    public void SetBox(Vector2 size)
    {
        _drawSize = size;
        var shape = GetChildCount() > 0 ? GetChild(0) as CollisionShape2D : null;
        if (shape?.Shape is RectangleShape2D rectangle)
            rectangle.Size = size;
    }

    public void Deactivate()
    {
        Monitoring = false;
        Visible = false;
        _activeTimer = 0.0;
        QueueRedraw();
    }

    private void OnAreaEntered(Area2D area)
    {
        if (!Monitoring || area is not IDamageable damageable || damageable.Team == Team)
            return;

        var id = area.GetInstanceId();
        if (_hitTargets.Contains(id))
            return;

        _hitTargets.Add(id);
        var info = new DamageInfo(Damage, _direction * Knockback, _source, Team);
        damageable.TakeDamage(info);
        EmitSignal(SignalName.HitConfirmed, area.GlobalPosition);
    }

    public override void _Draw()
    {
        if (!Visible)
            return;

        DrawRect(new Rect2(-_drawSize * 0.5f, _drawSize), new Color(1.0f, 0.85f, 0.35f, 0.28f));
    }
}
