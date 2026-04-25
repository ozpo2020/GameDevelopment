using Godot;

public partial class PlayerMotor : Node
{
	[Export] public float MaxSpeed { get; set; } = 126.0f;
	[Export] public float Acceleration { get; set; } = 1120.0f;
	[Export] public float Friction { get; set; } = 1280.0f;
	[Export] public float JumpVelocity { get; set; } = -252.0f;
	[Export] public float Gravity { get; set; } = 760.0f;
	[Export] public float FallGravityMultiplier { get; set; } = 1.28f;
	[Export] public float CoyoteTime { get; set; } = 0.12f;
	[Export] public float JumpBufferTime { get; set; } = 0.13f;
	[Export] public float DashSpeed { get; set; } = 292.0f;
	[Export] public float DashTime { get; set; } = 0.15f;
	[Export] public float DashCooldown { get; set; } = 0.48f;

	private PlayerController _player;
	private PlayerAbilities _abilities;
	private double _coyoteTimer;
	private double _jumpBufferTimer;
	private double _dashTimer;
	private double _dashCooldownTimer;
	private bool _usedDoubleJump;

	public bool IsDashing => _dashTimer > 0.0;
	public bool CanDash => _abilities?.HasDash == true && _dashCooldownTimer <= 0.0;

	public void Initialize(PlayerController player, PlayerAbilities abilities)
	{
		_player = player;
		_abilities = abilities;
	}

	public void Tick(double delta)
	{
		var dt = (float)delta;
		var velocity = _player.Velocity;
		var inputX = Input.GetAxis(InputBindings.MoveLeft, InputBindings.MoveRight);

		_dashCooldownTimer = Mathf.Max(0.0, _dashCooldownTimer - delta);
		_dashTimer = Mathf.Max(0.0, _dashTimer - delta);
		_jumpBufferTimer = Mathf.Max(0.0, _jumpBufferTimer - delta);

		if (_player.IsOnFloor())
		{
			_coyoteTimer = CoyoteTime;
			_usedDoubleJump = false;
		}
		else
		{
			_coyoteTimer = Mathf.Max(0.0, _coyoteTimer - delta);
		}

		if (Input.IsActionJustPressed(InputBindings.Jump))
			_jumpBufferTimer = JumpBufferTime;

		if (_dashTimer > 0.0)
		{
			velocity.X = _player.Facing * DashSpeed;
			velocity.Y = 0.0f;
		}
		else
		{
			var targetX = inputX * MaxSpeed;
			var rate = Mathf.Abs(targetX) > 0.01f ? Acceleration : Friction;
			velocity.X = Mathf.MoveToward(velocity.X, targetX, rate * dt);

			var gravity = velocity.Y > 0.0f ? Gravity * FallGravityMultiplier : Gravity;
			velocity.Y += gravity * dt;

			if (_jumpBufferTimer > 0.0 && CanJump())
			{
				var isAirJump = _coyoteTimer <= 0.0;
				velocity.Y = isAirJump ? JumpVelocity * 0.94f : JumpVelocity;
				_jumpBufferTimer = 0.0;

				if (isAirJump)
				{
					_usedDoubleJump = true;
					velocity.X += _player.Facing * 22.0f;
					_player.TriggerDoubleJump();
				}
			}

			if (Input.IsActionJustReleased(InputBindings.Jump) && velocity.Y < -70.0f)
				velocity.Y = -70.0f;

			if (CanDash && Input.IsActionJustPressed(InputBindings.Dash))
			{
				_dashTimer = DashTime;
				_dashCooldownTimer = DashCooldown;
				velocity = new Vector2(_player.Facing * DashSpeed, 0.0f);
				_player.Invincibility.Start(0.14f);
			}
		}

		if (Mathf.Abs(inputX) > 0.01f)
			_player.Facing = inputX > 0.0f ? 1 : -1;

		velocity += _player.Knockback.Tick(delta);
		_player.Velocity = velocity;
	}

	private bool CanJump()
	{
		if (_coyoteTimer > 0.0)
			return true;

		return _abilities?.HasDoubleJump == true && !_usedDoubleJump;
	}
}
