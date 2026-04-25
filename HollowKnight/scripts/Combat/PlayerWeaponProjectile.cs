using System.Collections.Generic;
using Godot;

public partial class PlayerWeaponProjectile : Area2D
{
	[Export] public DamageTeam Team { get; set; } = DamageTeam.Player;
	[Export] public int Damage { get; set; } = 2;
	[Export] public float Knockback { get; set; } = 230.0f;

	private readonly HashSet<ulong> _hitTargets = new();
	private Node2D _source;
	private Vector2 _velocity = Vector2.Right;
	private Vector2 _direction = Vector2.Right;
	private double _life = 0.95;
	private int _pierceLeft = 2;
	private float _pulse;
	private PlayerWeaponKind _weapon = PlayerWeaponKind.Sword;

	public override void _Ready()
	{
		Monitoring = true;
		Monitorable = false;
		CollisionLayer = 0;
		CollisionMask = CollisionLayers.Hurtbox;
		AreaEntered += OnAreaEntered;

		if (GetChildCount() == 0)
		{
			var shape = new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(24, 12) } };
			AddChild(shape);
		}
	}

	public void Configure(Node2D source, Vector2 position, Vector2 direction, PlayerWeaponKind weapon = PlayerWeaponKind.Sword)
	{
		_source = source;
		_weapon = weapon;
		_direction = direction == Vector2.Zero ? Vector2.Right : direction.Normalized();
		_velocity = _direction * (weapon == PlayerWeaponKind.Hammer ? 176.0f : weapon == PlayerWeaponKind.Axe ? 198.0f : 224.0f);
		Damage = weapon == PlayerWeaponKind.Hammer ? 4 : weapon == PlayerWeaponKind.Axe ? 3 : 2;
		Knockback = weapon == PlayerWeaponKind.Hammer ? 360.0f : weapon == PlayerWeaponKind.Axe ? 275.0f : 230.0f;
		_pierceLeft = weapon == PlayerWeaponKind.Sword ? 2 : 1;
		_life = weapon == PlayerWeaponKind.Hammer ? 0.72 : weapon == PlayerWeaponKind.Axe ? 0.88 : 0.95;
		GlobalPosition = position;
		Rotation = _direction.Angle();

		if (GetChildCount() > 0 && GetChild(0) is CollisionShape2D shape && shape.Shape is RectangleShape2D rectangle)
		{
			var baseSize = weapon == PlayerWeaponKind.Hammer ? new Vector2(24, 24) : weapon == PlayerWeaponKind.Axe ? new Vector2(28, 20) : new Vector2(30, 13);
			rectangle.Size = Mathf.Abs(_direction.Y) > 0.5f ? new Vector2(baseSize.Y, baseSize.X) : baseSize;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += _velocity * (float)delta;
		_life -= delta;
		_pulse += (float)delta * 18.0f;

		if (_life <= 0.0)
			QueueFree();

		QueueRedraw();
	}

	private void OnAreaEntered(Area2D area)
	{
		if (area is not IDamageable damageable || damageable.Team == Team)
			return;

		var id = area.GetInstanceId();
		if (_hitTargets.Contains(id))
			return;

		_hitTargets.Add(id);
		damageable.TakeDamage(new DamageInfo(Damage, _direction * Knockback, _source, Team));
		_pierceLeft--;

		if (_pierceLeft <= 0)
			QueueFree();
	}

	public override void _Draw()
	{
		var body = _weapon == PlayerWeaponKind.Hammer ? new Color(0.94f, 0.78f, 0.55f) : new Color(0.84f, 0.97f, 1.0f);
		var edge = _weapon == PlayerWeaponKind.Axe ? new Color(0.58f, 0.92f, 0.78f, 0.76f) : new Color(0.22f, 0.76f, 1.0f, 0.72f);
		var glow = new Color(0.24f, 0.82f, 1.0f, 0.22f + Mathf.Abs(Mathf.Sin(_pulse)) * 0.10f);
		var horizontal = Mathf.Abs(_direction.X) >= Mathf.Abs(_direction.Y);

		if (horizontal)
		{
			var size = _weapon == PlayerWeaponKind.Hammer ? new Vector2(34, 22) : _weapon == PlayerWeaponKind.Axe ? new Vector2(38, 20) : new Vector2(36, 16);
			DrawRect(new Rect2(-size * 0.5f, size), glow);
			DrawRect(new Rect2(new Vector2(-15, -3), new Vector2(30, 6)), edge);
			DrawRect(new Rect2(new Vector2(-11, -1), new Vector2(22, 2)), body);
			DrawRect(new Rect2(new Vector2(9, -5), new Vector2(_weapon == PlayerWeaponKind.Hammer ? 11 : 8, _weapon == PlayerWeaponKind.Hammer ? 12 : 10)), body);
			return;
		}

		DrawRect(new Rect2(new Vector2(-8, -18), new Vector2(16, 36)), glow);
		DrawRect(new Rect2(new Vector2(-3, -15), new Vector2(6, 30)), edge);
		DrawRect(new Rect2(new Vector2(-1, -11), new Vector2(2, 22)), body);
		DrawRect(new Rect2(new Vector2(-5, _direction.Y > 0.0f ? 9 : -17), new Vector2(10, 8)), body);
	}
}
