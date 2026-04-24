using Godot;

public partial class PlayerController : CharacterBody2D, ICombatFeedback
{
	[Signal] public delegate void HealthChangedEventHandler(int current, int maximum);
	[Signal] public delegate void PlayerDiedEventHandler();
	[Signal] public delegate void PlayerHurtEventHandler(Vector2 position);
	[Signal] public delegate void AttackLandedEventHandler(Vector2 position);
	[Signal] public delegate void FocusChangedEventHandler(int current, int maximum);

	public HealthComponent Health { get; private set; }
	public InvincibilityComponent Invincibility { get; private set; }
	public KnockbackComponent Knockback { get; private set; }
	public PlayerAbilities Abilities { get; private set; }
	public PlayerAnimationBridge Animation { get; private set; }
	public int Facing { get; set; } = 1;
	public int Focus { get; private set; }
	public int MaxFocus { get; private set; } = 9;
	public string WeaponName => _combat?.WeaponName ?? "SWORD";
	public string StateText => Animation?.CurrentState ?? "Booting";
	public bool IsPerformingAttack => _slashTimer > 0.0;

	private PlayerMotor _motor;
	private PlayerCombat _combat;
	private const float SlashWindup = 0.10f;
	private const float SlashSwing = 0.085f;
	private const float SlashRecover = 0.15f;
	private const float SlashVisualDuration = SlashWindup + SlashSwing + SlashRecover;
	private double _slashTimer;
	private double _slashAge;
	private Vector2 _slashDirection = Vector2.Right;
	private int _slashFacing = 1;
	private int _slashComboStep = 1;
	private PlayerWeaponKind _equippedWeapon = PlayerWeaponKind.Sword;
	private PlayerWeaponKind _slashWeapon = PlayerWeaponKind.Sword;
	private double _airJumpFxTimer;
	private int _airJumpFxFacing = 1;

	public PlayerStateMachine StateMachine { get; private set; }

	public override void _Ready()
	{
		ZIndex = 10;
		CollisionLayer = CollisionLayers.PlayerBody;
		CollisionMask = CollisionLayers.World | CollisionLayers.EnemyBody;

		AddChild(new CollisionShape2D
		{
			Shape = new CapsuleShape2D { Radius = 6.0f, Height = 20.0f }
		});

		Health = new HealthComponent { Name = "HealthComponent", MaxHealth = 5 };
		AddChild(Health);
		Health.HealthChanged += OnHealthChanged;
		Health.Died += OnDied;

		Invincibility = new InvincibilityComponent { Name = "InvincibilityComponent" };
		AddChild(Invincibility);

		Knockback = new KnockbackComponent { Name = "KnockbackComponent", Recovery = 920.0f };
		AddChild(Knockback);

		Abilities = new PlayerAbilities { Name = "PlayerAbilities", HasDoubleJump = true };
		AddChild(Abilities);

		Animation = new PlayerAnimationBridge { Name = "PlayerAnimationBridge" };
		AddChild(Animation);

		var hurtbox = new Hurtbox { Name = "Hurtbox", Team = DamageTeam.Player, InvincibilityTime = 0.55f };
		AddChild(hurtbox);
		hurtbox.Bind(Health, Invincibility);

		_motor = new PlayerMotor { Name = "PlayerMotor" };
		AddChild(_motor);
		_motor.Initialize(this, Abilities);

		_combat = new PlayerCombat { Name = "PlayerCombat" };
		AddChild(_combat);
		_combat.Initialize(this);
		_combat.AttackStarted += OnAttackStarted;

		StateMachine = new PlayerStateMachine { Name = "PlayerStateMachine" };
		AddChild(StateMachine);
		InitializePlayerStates();
	}

	private void InitializePlayerStates()
	{
		StateMachine.AddState(PlayerStateNames.Idle, new PlayerIdleState(this));
		StateMachine.AddState(PlayerStateNames.Run, new PlayerRunState(this));
		StateMachine.AddState(PlayerStateNames.Jump, new PlayerJumpState(this));
		StateMachine.AddState(PlayerStateNames.Dash, new PlayerDashState(this));
		StateMachine.AddState(PlayerStateNames.Attack, new PlayerAttackState(this));
		StateMachine.AddState(PlayerStateNames.Hurt, new PlayerHurtState(this));
		StateMachine.ChangeState(PlayerStateNames.Idle);
	}


	public override void _Process(double delta)
	{
		StateMachine.Update(delta);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Health.IsDead)
			return;

		StateMachine.PhysicsUpdate(delta);
		_motor.Tick(delta);
		_combat.Tick(delta);
		_airJumpFxTimer = Mathf.Max(0.0, _airJumpFxTimer - delta);
		if (_slashTimer > 0.0)
		{
			_slashAge += delta;
			_slashTimer = Mathf.Max(0.0, _slashTimer - delta);
		}
		MoveAndSlide();
		UpdateAnimationState();
		QueueRedraw();
	}

	public override void _Input(InputEvent @event)
	{
		StateMachine.HandleInput(@event);
	}


	public void RespawnAt(Vector2 position)
	{
		GlobalPosition = position;
		Velocity = Vector2.Zero;
		Focus = 0;
		Health.ResetHealth();
		Animation.SetState("Idle");
		EmitSignal(SignalName.FocusChanged, Focus, MaxFocus);
		QueueRedraw();
	}

	public void OnDamageReceived(DamageInfo info)
	{
		Knockback.Apply(info.Knockback);
		Animation.SetState("Hurt");
		StateMachine.ChangeState(PlayerStateNames.Hurt);
		EmitSignal(SignalName.PlayerHurt, GlobalPosition);
		QueueRedraw();
	}

	public void NotifyAttackLanded(Vector2 position, int comboStep = 1)
	{
		GainFocus(comboStep >= 3 ? 2 : 1);
		EmitSignal(SignalName.AttackLanded, position);
	}

	public void StartSlash(Vector2 direction, int comboStep = 1, PlayerWeaponKind weapon = PlayerWeaponKind.Sword)
	{
		_slashComboStep = Mathf.Clamp(comboStep, 1, 3);
		_slashWeapon = weapon;
		_slashDirection = direction == Vector2.Zero ? new Vector2(Facing, 0.0f) : direction.Normalized();
		_slashFacing = _slashDirection.X == 0.0f ? Facing : (_slashDirection.X > 0.0f ? 1 : -1);
		var extra = weapon == PlayerWeaponKind.Hammer ? 0.11f : weapon == PlayerWeaponKind.Axe ? 0.07f : 0.0f;
		_slashTimer = SlashVisualDuration + (_slashComboStep == 3 ? 0.045f : 0.0f) + extra;
		_slashAge = 0.0;
		Animation.SetState("Attack");
		QueueRedraw();
	}

	public void StartWeaponArt(Vector2 direction, PlayerWeaponKind weapon = PlayerWeaponKind.Sword)
	{
		_slashComboStep = 3;
		_slashWeapon = weapon;
		_slashDirection = direction == Vector2.Zero ? new Vector2(Facing, 0.0f) : direction.Normalized();
		_slashFacing = _slashDirection.X == 0.0f ? Facing : (_slashDirection.X > 0.0f ? 1 : -1);
		_slashTimer = SlashVisualDuration + (weapon == PlayerWeaponKind.Hammer ? 0.18f : 0.09f);
		_slashAge = SlashWindup * 0.45f;
		Animation.SetState("Attack");
		Velocity = new Vector2(Velocity.X * 0.72f, Velocity.Y);
		QueueRedraw();
	}

	public void NotifyWeaponChanged(PlayerWeaponKind weapon)
	{
		_equippedWeapon = weapon;
		QueueRedraw();
	}

	public void TriggerDoubleJump()
	{
		_airJumpFxTimer = 0.24;
		_airJumpFxFacing = Facing;
		QueueRedraw();
	}

	public void BounceFromDownStrike()
	{
		Velocity = new Vector2(Velocity.X, -178.0f);
	}

	public void ApplySlashImpulse(Vector2 direction, int comboStep = 1, PlayerWeaponKind weapon = PlayerWeaponKind.Sword)
	{
		if (Mathf.Abs(direction.X) < 0.5f)
			return;

		var force = comboStep switch
		{
			2 => 42.0f,
			3 => 58.0f,
			_ => 34.0f
		};

		if (weapon == PlayerWeaponKind.Axe)
			force *= 0.82f;
		else if (weapon == PlayerWeaponKind.Hammer)
			force *= 0.55f;

		Velocity = new Vector2(Velocity.X + direction.X * force, Velocity.Y);
	}

	public void GainFocus(int amount)
	{
		var next = Mathf.Clamp(Focus + amount, 0, MaxFocus);
		if (next == Focus)
			return;

		Focus = next;
		EmitSignal(SignalName.FocusChanged, Focus, MaxFocus);
	}

	public bool TrySpendFocus(int amount)
	{
		if (Focus < amount)
			return false;

		Focus -= amount;
		EmitSignal(SignalName.FocusChanged, Focus, MaxFocus);
		return true;
	}

	private void OnHealthChanged(int current, int maximum)
	{
		EmitSignal(SignalName.HealthChanged, current, maximum);
		SignalBus.Instance?.EmitSignal(SignalBus.SignalName.PlayerHealthChanged, current, maximum);
	}

	private void OnDied()
	{
		Animation.SetState("Dead");
		EmitSignal(SignalName.PlayerDied);
	}

	private void OnAttackStarted()
	{
		StateMachine.ChangeState(PlayerStateNames.Attack);
	}

	private void UpdateAnimationState()
	{
		if (StateMachine.CurrentStateName == PlayerStateNames.Hurt)
		{
			Animation.SetState("Hurt");
			return;
		}

		if (StateMachine.CurrentStateName == PlayerStateNames.Attack)
		{
			Animation.SetState("Attack");
			return;
		}

		if (StateMachine.CurrentStateName == PlayerStateNames.Dash)
		{
			Animation.SetState("Dash");
			return;
		}

		if (_slashTimer > 0.0)
			Animation.SetState("Attack");
		else if (_motor.IsDashing)
			Animation.SetState("Dash");
		else if (!IsOnFloor())
			Animation.SetState(Velocity.Y < 0.0f ? "Jump" : "Fall");
		else if (Mathf.Abs(Velocity.X) > 6.0f)
			Animation.SetState("Run");
		else
			Animation.SetState("Idle");
	}

	public override void _Draw()
	{
		if (Invincibility.IsInvincible && Engine.GetPhysicsFrames() % 8 < 4)
			return;

		var mask = new Color(0.86f, 0.89f, 0.86f);
		var maskShade = new Color(0.62f, 0.67f, 0.68f);
		var cloak = Abilities.HasDash ? new Color(0.12f, 0.50f, 0.70f) : new Color(0.20f, 0.25f, 0.29f);
		var cloakDeep = cloak.Darkened(0.36f);
		var outline = new Color(0.025f, 0.030f, 0.035f);
		var eye = new Color(0.025f, 0.035f, 0.045f);
		var wind = Animation.CurrentState == "Attack" ? 0.0f : Mathf.Round(Mathf.Sin((float)Engine.GetPhysicsFrames() * 0.16f) * 1.0f);
		var runOffset = Animation.CurrentState == "Run" ? (Engine.GetPhysicsFrames() / 6 % 2 == 0 ? 1 : -1) : 0;
		var visualFacing = _slashTimer > 0.0 ? _slashFacing : Facing;
		var slashProgress = SlashProgress();
		var swingProgress = SlashSwingProgress();
		var windupProgress = SlashWindupProgress();
		var recoverProgress = SlashRecoverProgress();
		var swingEase = _slashTimer > 0.0 ? Mathf.Sin(swingProgress * Mathf.Pi) : 0.0f;
		var comboWeight = _slashComboStep == 3 ? 1.22f : _slashComboStep == 2 ? 1.08f : 1.0f;
		var bodyLean = Pixel(_slashTimer > 0.0
			? visualFacing * (-2.0f * (1.0f - windupProgress) + 5.0f * swingEase - 2.0f * recoverProgress) * comboWeight
			: 0.0f);
		var maskLift = Pixel(_slashDirection.Y < 0.0f && _slashTimer > 0.0 ? -2.0f * swingEase : 0.0f);
		var crouch = Pixel(_slashDirection.Y > 0.0f && _slashTimer > 0.0f ? 2.0f * swingEase : 0.0f);

		DrawRect(new Rect2(new Vector2(-9, 7), new Vector2(18, 2)), new Color(0.0f, 0.0f, 0.0f, 0.28f));

		if (Animation.CurrentState == "Dash")
		{
			DrawRect(new Rect2(new Vector2(-visualFacing * 25 - 5, -9), new Vector2(18, 5)), new Color(0.34f, 0.86f, 1.0f, 0.32f));
			DrawRect(new Rect2(new Vector2(-visualFacing * 32 - 5, -3), new Vector2(15, 3)), new Color(0.34f, 0.86f, 1.0f, 0.20f));
		}

		DrawAirJumpBurst();
		DrawSlashEffect(slashProgress);

		DrawRect(new Rect2(new Vector2(-7 + bodyLean, -13 + crouch), new Vector2(14, 20 - crouch)), outline);
		DrawRect(new Rect2(new Vector2(-6 + bodyLean, -12 + crouch), new Vector2(12, 18 - crouch)), cloak);
		DrawRect(new Rect2(new Vector2(-4 + bodyLean, -9 + crouch), new Vector2(8, 15 - crouch)), cloakDeep);
		DrawRect(new Rect2(new Vector2(-7 + bodyLean, -4 + wind + crouch), new Vector2(3, 10 - crouch * 0.5f)), cloak.Lightened(0.13f));
		DrawRect(new Rect2(new Vector2(4 + bodyLean, -4 - wind + crouch), new Vector2(3, 10 - crouch * 0.5f)), cloak.Darkened(0.16f));
		DrawRect(new Rect2(new Vector2(-5 + bodyLean, 5), new Vector2(4, 3)), outline);
		DrawRect(new Rect2(new Vector2(1 + bodyLean, 5), new Vector2(4, 3)), outline);

		var head = new Vector2(bodyLean * 0.5f, maskLift + crouch * 0.5f);
		DrawRect(new Rect2(new Vector2(-7, -21) + head, new Vector2(14, 14)), outline);
		DrawRect(new Rect2(new Vector2(-6, -20) + head, new Vector2(12, 12)), mask);
		DrawRect(new Rect2(new Vector2(-5, -19) + head, new Vector2(10, 2)), mask.Lightened(0.15f));
		DrawRect(new Rect2(new Vector2(-6, -11) + head, new Vector2(12, 3)), maskShade);
		DrawRect(new Rect2(new Vector2(-8, -18) + head, new Vector2(2, 6)), mask);
		DrawRect(new Rect2(new Vector2(6, -18) + head, new Vector2(2, 6)), mask);
		DrawRect(new Rect2(new Vector2(-10, -23) + head, new Vector2(4, 8)), outline);
		DrawRect(new Rect2(new Vector2(6, -23) + head, new Vector2(4, 8)), outline);
		DrawRect(new Rect2(new Vector2(-9, -24) + head, new Vector2(2, 8)), mask);
		DrawRect(new Rect2(new Vector2(7, -24) + head, new Vector2(2, 8)), mask);
		DrawRect(new Rect2(new Vector2(visualFacing > 0 ? 1 : -4, -16) + head, new Vector2(3, 3)), eye);
		DrawRect(new Rect2(new Vector2(visualFacing > 0 ? 3 : -4, -15) + head, new Vector2(1, 1)), new Color(0.75f, 0.92f, 0.95f, 0.55f));

		DrawArmAndBlade(outline, maskShade, visualFacing);

		DrawRect(new Rect2(new Vector2(-5 + bodyLean * 0.3f, 6 + runOffset), new Vector2(3, 5)), outline);
		DrawRect(new Rect2(new Vector2(2 + bodyLean * 0.3f, 6 - runOffset), new Vector2(3, 5)), outline);
	}

	private void DrawArmAndBlade(Color outline, Color armColor, int visualFacing)
	{
		if (_slashTimer <= 0.0)
		{
			DrawRect(new Rect2(new Vector2(visualFacing > 0 ? 6 : -9, -8), new Vector2(3, 9)), outline);
			DrawRect(new Rect2(new Vector2(visualFacing > 0 ? 7 : -8, -7), new Vector2(1, 7)), armColor);
			DrawHeldWeapon(new Vector2(visualFacing > 0 ? 10 : -10, -6), visualFacing, _equippedWeapon, outline);
			DrawRect(new Rect2(new Vector2(visualFacing > 0 ? 8 : -10, -6), new Vector2(3, 4)), outline);
			return;
		}

		var t = SlashSwingProgress();
		var windup = SlashWindupProgress();
		var recover = SlashRecoverProgress();
		var visualWeapon = _slashTimer > 0.0 ? _slashWeapon : _equippedWeapon;
		var blade = visualWeapon == PlayerWeaponKind.Hammer
			? new Color(0.72f, 0.76f, 0.74f)
			: visualWeapon == PlayerWeaponKind.Axe
				? new Color(0.80f, 0.87f, 0.84f)
				: new Color(0.86f, 0.90f, 0.84f);
		var finisher = _slashComboStep == 3;
		if (_slashDirection.Y < -0.5f)
		{
			var hilt = Pixel(new Vector2(visualFacing * Mathf.Lerp(6.0f, 9.0f, windup), -10.0f - 4.0f * SmoothStep01(t)));
			var upAngle = Mathf.Lerp(Mathf.DegToRad(150.0f * visualFacing), Mathf.DegToRad(-86.0f), SmoothStep01(t));
			if (recover > 0.0f)
				upAngle = Mathf.Lerp(upAngle, Mathf.DegToRad(-25.0f * visualFacing), SmoothStep01(recover));
			DrawPixelRect(hilt - new Vector2(1, 2), new Vector2(3, 8), outline);
			DrawPixelRect(hilt, new Vector2(1, 6), armColor);
			DrawWeapon(hilt + new Vector2(0, -3), upAngle, finisher ? 30.0f : 24.0f, blade, outline, visualWeapon);
			return;
		}

		if (_slashDirection.Y > 0.5f)
		{
			var hilt = Pixel(new Vector2(visualFacing * 7.0f, -4.0f + 4.0f * SmoothStep01(t)));
			var downAngle = Mathf.Lerp(Mathf.DegToRad(-30.0f * visualFacing), Mathf.DegToRad(86.0f), SmoothStep01(t));
			if (recover > 0.0f)
				downAngle = Mathf.Lerp(downAngle, Mathf.DegToRad(15.0f * visualFacing), SmoothStep01(recover));
			DrawPixelRect(hilt - new Vector2(1, 1), new Vector2(3, 8), outline);
			DrawPixelRect(hilt, new Vector2(1, 7), armColor);
			DrawWeapon(hilt + new Vector2(0, 4), downAngle, finisher ? 31.0f : 25.0f, blade, outline, visualWeapon);
			return;
		}

		var side = visualFacing;
		var swing = SmoothStep01(t);
		var startAngle = _slashComboStep == 2 ? 42.0f : finisher ? -158.0f : -132.0f;
		var endAngle = _slashComboStep == 2 ? -118.0f : finisher ? 70.0f : 32.0f;
		var hiltStartY = _slashComboStep == 2 ? -5.0f : -11.0f;
		var hiltEndY = _slashComboStep == 2 ? -12.0f : -6.0f;
		var hiltX = side * Pixel(Mathf.Lerp(4.0f, finisher ? 13.0f : 11.0f, Mathf.Max(windup, swing)));
		var hiltY = Pixel(Mathf.Lerp(hiltStartY, hiltEndY, swing));
		var sideAngle = Mathf.Lerp(Mathf.DegToRad(startAngle * side), Mathf.DegToRad(endAngle * side), swing);
		if (recover > 0.0f)
		{
			hiltX = side * Pixel(Mathf.Lerp(11.0f, 7.0f, SmoothStep01(recover)));
			hiltY = Pixel(Mathf.Lerp(-6.0f, -8.0f, SmoothStep01(recover)));
			sideAngle = Mathf.Lerp(sideAngle, Mathf.DegToRad(8.0f * side), SmoothStep01(recover));
		}
		DrawPixelRect(new Vector2(side > 0 ? 5 : -9, hiltY - 3), new Vector2(4, 8), outline);
		DrawPixelRect(new Vector2(side > 0 ? 6 : -8, hiltY - 2), new Vector2(2, 6), armColor);
		var weaponLength = visualWeapon == PlayerWeaponKind.Axe ? 30.0f : visualWeapon == PlayerWeaponKind.Hammer ? 24.0f : 25.0f;
		if (finisher)
			weaponLength += visualWeapon == PlayerWeaponKind.Hammer ? 6.0f : 6.0f;
		DrawWeapon(new Vector2(hiltX, hiltY), sideAngle, weaponLength, blade, outline, visualWeapon);
	}

	private void DrawSlashEffect(float slashProgress)
	{
		if (_slashTimer <= 0.0)
			return;

		var t = SlashSwingProgress();
		if (t <= 0.0f || t >= 1.0f)
			return;

		var finisher = _slashComboStep == 3;
		var alpha = (finisher ? 0.92f : 0.75f) * (1.0f - Mathf.Abs(t - 0.55f));
		var slash = new Color(0.92f, 0.98f, 0.88f, alpha);
		var glow = new Color(0.36f, 0.86f, 1.0f, alpha * 0.32f);

		if (_slashDirection.Y < -0.5f)
		{
			for (var i = 0; i < 6; i++)
			{
				var x = -18 + i * 7;
				var y = -31 + Mathf.Abs(i - 3) * 3 + SmoothStep01(t) * 4;
				DrawRect(new Rect2(new Vector2(x, y), new Vector2(7, 2)), glow);
				DrawRect(new Rect2(new Vector2(x + 1, y - 1), new Vector2(5, 1)), slash);
			}
			return;
		}

		if (_slashDirection.Y > 0.5f)
		{
			for (var i = 0; i < 6; i++)
			{
				var x = -18 + i * 7;
				var y = 13 - Mathf.Abs(i - 3) * 2 - SmoothStep01(t) * 3;
				DrawRect(new Rect2(new Vector2(x, y), new Vector2(7, 2)), glow);
				DrawRect(new Rect2(new Vector2(x + 1, y + 1), new Vector2(5, 1)), slash);
			}
			return;
		}

		var side = _slashFacing;
		var startAngle = _slashComboStep == 2 ? 42.0f : finisher ? -158.0f : -126.0f;
		var endAngle = _slashComboStep == 2 ? -118.0f : finisher ? 70.0f : 28.0f;
		var segments = finisher ? 11 : 8;
		for (var i = 0; i < segments; i++)
		{
			var segment = Mathf.Clamp(SmoothStep01(t) - i * (finisher ? 0.032f : 0.045f), 0.0f, 1.0f);
			var angle = Mathf.Lerp(Mathf.DegToRad(startAngle * side), Mathf.DegToRad(endAngle * side), segment);
			var radius = (finisher ? 28.0f : 22.0f) - i * 0.8f;
			var point = Pixel(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius + new Vector2(side * 8.0f, -8.0f));
			DrawFacingRect(point, new Vector2((finisher ? 10.0f : 8.0f) - i * 0.55f, finisher && i < 5 ? 3 : 2), side, i % 2 == 0 ? slash : glow);
		}
	}

	private void DrawAirJumpBurst()
	{
		if (_airJumpFxTimer <= 0.0)
			return;

		var t = 1.0f - Mathf.Clamp((float)(_airJumpFxTimer / 0.24), 0.0f, 1.0f);
		var alpha = 0.58f * (1.0f - t);
		var wind = new Color(0.52f, 0.92f, 1.0f, alpha);
		var dust = new Color(0.86f, 0.92f, 0.78f, alpha * 0.72f);

		for (var i = 0; i < 6; i++)
		{
			var side = i % 2 == 0 ? -1.0f : 1.0f;
			var x = side * (6.0f + i * 2.4f + t * 8.0f);
			var y = 7.0f + i % 3 * 3.0f + t * 12.0f;
			DrawRect(new Rect2(Pixel(new Vector2(x, y)), new Vector2(6 - i * 0.45f, 2)), i < 3 ? wind : dust);
		}

		DrawRect(new Rect2(new Vector2(-_airJumpFxFacing * (12 + t * 12), 9 + t * 4), new Vector2(18, 2)), wind);
	}

	private void DrawHeldWeapon(Vector2 hilt, int side, PlayerWeaponKind weapon, Color outline)
	{
		if (weapon == PlayerWeaponKind.Axe)
		{
			var handleX = side > 0 ? hilt.X : hilt.X - 22;
			DrawRect(new Rect2(new Vector2(handleX, hilt.Y - 1), new Vector2(22, 3)), new Color(0.54f, 0.40f, 0.24f));
			DrawRect(new Rect2(new Vector2(side > 0 ? handleX + 18 : handleX - 1, hilt.Y - 5), new Vector2(6, 9)), outline);
			DrawRect(new Rect2(new Vector2(side > 0 ? handleX + 19 : handleX, hilt.Y - 4), new Vector2(4, 7)), new Color(0.80f, 0.87f, 0.84f));
			return;
		}

		if (weapon == PlayerWeaponKind.Hammer)
		{
			var handleX = side > 0 ? hilt.X : hilt.X - 18;
			DrawRect(new Rect2(new Vector2(handleX, hilt.Y), new Vector2(20, 3)), new Color(0.50f, 0.34f, 0.22f));
			DrawRect(new Rect2(new Vector2(side > 0 ? handleX + 15 : handleX - 5, hilt.Y - 5), new Vector2(10, 10)), outline);
			DrawRect(new Rect2(new Vector2(side > 0 ? handleX + 16 : handleX - 4, hilt.Y - 4), new Vector2(8, 8)), new Color(0.68f, 0.72f, 0.70f));
			return;
		}

		var bladeX = side > 0 ? hilt.X : hilt.X - 18;
		DrawRect(new Rect2(new Vector2(bladeX, hilt.Y - 1), new Vector2(18, 2)), new Color(0.82f, 0.86f, 0.82f));
	}

	private void DrawWeapon(Vector2 hilt, float angle, float length, Color blade, Color outline, PlayerWeaponKind weapon)
	{
		hilt = Pixel(hilt);
		var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		var tip = Pixel(hilt + direction * length);
		var guard = Pixel(hilt - direction * 3.0f);

		var shaft = weapon == PlayerWeaponKind.Sword ? blade : new Color(0.56f, 0.40f, 0.24f);
		DrawLine(guard, tip, outline, weapon == PlayerWeaponKind.Hammer ? 6.0f : 5.0f, false);
		DrawLine(hilt, tip, shaft, weapon == PlayerWeaponKind.Sword ? 2.0f : 3.0f, false);
		if (weapon == PlayerWeaponKind.Axe)
		{
			var cross = new Vector2(-direction.Y, direction.X);
			DrawLine(tip - cross * 7.0f - direction * 2.0f, tip + cross * 7.0f - direction * 2.0f, outline, 7.0f, false);
			DrawLine(tip - cross * 5.0f, tip + cross * 5.0f, blade, 4.0f, false);
		}
		else if (weapon == PlayerWeaponKind.Hammer)
		{
			var cross = new Vector2(-direction.Y, direction.X);
			DrawLine(tip - cross * 8.0f, tip + cross * 8.0f, outline, 10.0f, false);
			DrawLine(tip - cross * 6.0f, tip + cross * 6.0f, blade, 7.0f, false);
		}
		else
		{
			DrawCircle(tip, 1.0f, blade.Lightened(0.15f));
		}
		DrawRect(new Rect2(hilt - new Vector2(2, 2), new Vector2(4, 4)), outline);
		DrawRect(new Rect2(hilt - new Vector2(1, 1), new Vector2(2, 2)), new Color(0.78f, 0.70f, 0.52f));
	}

	private void DrawFacingRect(Vector2 origin, Vector2 size, float side, Color color)
	{
		var position = side >= 0.0f ? origin : new Vector2(origin.X - size.X, origin.Y);
		position = Pixel(position);
		DrawRect(new Rect2(position, size), color);
	}

	private void DrawPixelRect(Vector2 origin, Vector2 size, Color color)
	{
		DrawRect(new Rect2(Pixel(origin), size), color);
	}

	private float SlashProgress()
	{
		if (_slashTimer <= 0.0)
			return 0.0f;

		return Mathf.Clamp((float)(_slashAge / SlashVisualDuration), 0.0f, 1.0f);
	}

	private float SlashSwingProgress()
	{
		if (_slashTimer <= 0.0)
			return 0.0f;

		var swingAge = (float)_slashAge - SlashWindup;
		return Mathf.Clamp(swingAge / SlashSwing, 0.0f, 1.0f);
	}

	private float SlashWindupProgress()
	{
		if (_slashTimer <= 0.0)
			return 0.0f;

		return Mathf.Clamp((float)_slashAge / SlashWindup, 0.0f, 1.0f);
	}

	private float SlashRecoverProgress()
	{
		if (_slashTimer <= 0.0)
			return 0.0f;

		var recoverAge = (float)_slashAge - SlashWindup - SlashSwing;
		return Mathf.Clamp(recoverAge / SlashRecover, 0.0f, 1.0f);
	}

	private static float SmoothStep01(float value)
	{
		value = Mathf.Clamp(value, 0.0f, 1.0f);
		return value * value * (3.0f - 2.0f * value);
	}

	private static float Pixel(float value)
	{
		return Mathf.Round(value);
	}

	private static Vector2 Pixel(Vector2 value)
	{
		return value.Round();
	}
}
