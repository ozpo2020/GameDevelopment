using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerCombat : Node
{
    private const double AttackBufferTime = 0.18;
    private const double ComboResetTime = 0.76;
    private const int SpiritBladeCost = 3;

    private PlayerController _player;
    private Hitbox _hitbox;
    private AttackProfile _pendingProfile = CreateProfile(1, PlayerWeaponKind.Sword);
    private double _cooldown;
    private double _artCooldown;
    private double _comboTimer;
    private double _attackBufferTimer;
    private double _pendingHitTimer;
    private bool _pendingHit;
    private bool _attackBuffered;
    private int _comboStep;
    private int _pendingComboStep = 1;
    private PlayerWeaponKind _currentWeapon = PlayerWeaponKind.Sword;
    private PlayerWeaponKind _pendingWeapon = PlayerWeaponKind.Sword;
    private Vector2 _lastAttackDirection = Vector2.Right;
    private readonly Queue<PlayerWeaponProjectile> _projectilePool = new();

    public event Action? AttackStarted;

    public string WeaponName => _currentWeapon switch
    {
        PlayerWeaponKind.Axe => "AXE",
        PlayerWeaponKind.Hammer => "HAMMER",
        _ => "SWORD"
    };

    public void Initialize(PlayerController player)
    {
        _player = player;
        _hitbox = new Hitbox
        {
            Name = "SlashHitbox",
            Team = DamageTeam.Player,
            Visible = false,
            Position = Vector2.Zero
        };
        _hitbox.HitConfirmed += OnHitConfirmed;
        _player.AddChild(_hitbox);
    }

    public void Tick(double delta)
    {
        _cooldown = Mathf.Max(0.0, _cooldown - delta);
        _artCooldown = Mathf.Max(0.0, _artCooldown - delta);
        _comboTimer = Mathf.Max(0.0, _comboTimer - delta);

        if (_comboTimer <= 0.0 && !_pendingHit)
            _comboStep = 0;

        UpdateWeaponSelection();

        var stateName = _player.StateMachine.CurrentStateName;
        var combatInputAllowed = stateName != PlayerStateNames.Dash && stateName != PlayerStateNames.Hurt;

        if (combatInputAllowed && Input.IsActionJustPressed(InputBindings.Attack))
        {
            _attackBuffered = true;
            _attackBufferTimer = AttackBufferTime;
        }

        if (_attackBuffered)
        {
            _attackBufferTimer -= delta;
            if (_attackBufferTimer <= 0.0)
                _attackBuffered = false;
        }

        if (_pendingHit)
        {
            _pendingHitTimer -= delta;
            if (_pendingHitTimer <= 0.0)
                ActivatePendingHitbox();
        }

        if (combatInputAllowed && Input.IsActionJustPressed(InputBindings.Interact) && TryCastSpiritBlade())
            return;

        if (_cooldown > 0.0 || _pendingHit || !_attackBuffered)
            return;

        BeginAttack();
    }

    private void BeginAttack()
    {
        _attackBuffered = false;
        if (_comboTimer <= 0.0)
            _comboStep = 0;

        _comboStep = _comboStep % 3 + 1;
        _pendingComboStep = _comboStep;
        _pendingWeapon = _currentWeapon;
        _pendingProfile = CreateProfile(_pendingComboStep, _pendingWeapon);

        var direction = AttackDirection();
        _lastAttackDirection = direction;
        _pendingHit = true;
        _pendingHitTimer = WindupFor(_pendingComboStep, _pendingWeapon);
        _cooldown = _pendingProfile.Cooldown;
        _comboTimer = ComboResetTime;
        _player.StartSlash(direction, _pendingComboStep, _pendingWeapon);
        AttackStarted?.Invoke();
    }

    private bool TryCastSpiritBlade()
    {
        var cost = _currentWeapon == PlayerWeaponKind.Hammer ? SpiritBladeCost + 1 : SpiritBladeCost;
        if (_artCooldown > 0.0 || _pendingHit || !_player.TrySpendFocus(cost))
            return false;

        var direction = AttackDirection();
        _comboStep = 0;
        _comboTimer = 0.0;
        _attackBuffered = false;
        _artCooldown = 0.42;
        _cooldown = Mathf.Max(_cooldown, 0.18);
        _lastAttackDirection = direction;
        _player.StartWeaponArt(direction, _currentWeapon);
        SpawnSpiritBlade(direction);
        return true;
    }

    private PlayerWeaponProjectile AcquireProjectile()
    {
        if (_projectilePool.TryDequeue(out var projectile))
            return projectile;

        projectile = new PlayerWeaponProjectile();
        projectile.SetReturnCallback(ReturnProjectileToPool);
        _player.GetParent()?.AddChild(projectile);
        return projectile;
    }

    private void ReturnProjectileToPool(PlayerWeaponProjectile projectile)
    {
        projectile.Visible = false;
        projectile.Monitoring = false;
        projectile.SetPhysicsProcess(false);
        _projectilePool.Enqueue(projectile);
    }

    private void SpawnSpiritBlade(Vector2 direction)
    {
        var projectile = AcquireProjectile();
        var offset = direction * 20.0f + new Vector2(0, -8);
        projectile.Configure(_player, _player.GlobalPosition + offset, direction, _currentWeapon);
    }

    private void ActivatePendingHitbox()
    {
        _pendingHit = false;
        var direction = _lastAttackDirection;
        var range = RangeFor(_pendingComboStep, _pendingWeapon);
        var verticalOffset = direction.Y > 0.5f ? 1.0f : direction.Y < -0.5f ? -3.0f : -2.0f;
        _hitbox.SetBox(HitboxSizeFor(_pendingComboStep, direction, _pendingWeapon));
        _hitbox.Position = direction * range + new Vector2(0, verticalOffset);
        _hitbox.Rotation = direction.Angle();
        _hitbox.Activate(_pendingProfile, _player, direction);
        _player.ApplySlashImpulse(direction, _pendingComboStep, _pendingWeapon);
    }

    private void OnHitConfirmed(Vector2 position)
    {
        if (_lastAttackDirection == Vector2.Down)
            _player.BounceFromDownStrike();

        _player.NotifyAttackLanded(position, _pendingComboStep);
    }

    private void UpdateWeaponSelection()
    {
        if (_pendingHit)
            return;

        if (Input.IsActionJustPressed(InputBindings.WeaponSword))
            SetWeapon(PlayerWeaponKind.Sword);
        else if (Input.IsActionJustPressed(InputBindings.WeaponAxe))
            SetWeapon(PlayerWeaponKind.Axe);
        else if (Input.IsActionJustPressed(InputBindings.WeaponHammer))
            SetWeapon(PlayerWeaponKind.Hammer);
        else if (Input.IsActionJustPressed(InputBindings.WeaponNext))
            SetWeapon(_currentWeapon switch
            {
                PlayerWeaponKind.Sword => PlayerWeaponKind.Axe,
                PlayerWeaponKind.Axe => PlayerWeaponKind.Hammer,
                _ => PlayerWeaponKind.Sword
            });
    }

    private void SetWeapon(PlayerWeaponKind weapon)
    {
        if (_currentWeapon == weapon)
            return;

        _currentWeapon = weapon;
        _comboStep = 0;
        _comboTimer = 0.0;
        _player.NotifyWeaponChanged(_currentWeapon);
    }

    private Vector2 AttackDirection()
    {
        if (Input.IsActionPressed(InputBindings.MoveUp))
            return Vector2.Up;

        if (Input.IsActionPressed(InputBindings.MoveDown) && !_player.IsOnFloor())
            return Vector2.Down;

        return new Vector2(_player.Facing, 0.0f);
    }

    private static AttackProfile CreateProfile(int comboStep, PlayerWeaponKind weapon)
    {
        if (weapon == PlayerWeaponKind.Axe)
        {
            return comboStep switch
            {
                2 => new AttackProfile { Damage = 2, Knockback = 255.0f, Cooldown = 0.40f, ActiveTime = 0.14f, HitStop = 0.045f },
                3 => new AttackProfile { Damage = 3, Knockback = 330.0f, Cooldown = 0.58f, ActiveTime = 0.18f, HitStop = 0.070f },
                _ => new AttackProfile { Damage = 1, Knockback = 225.0f, Cooldown = 0.34f, ActiveTime = 0.13f, HitStop = 0.040f }
            };
        }

        if (weapon == PlayerWeaponKind.Hammer)
        {
            return comboStep switch
            {
                2 => new AttackProfile { Damage = 2, Knockback = 330.0f, Cooldown = 0.52f, ActiveTime = 0.16f, HitStop = 0.060f },
                3 => new AttackProfile { Damage = 4, Knockback = 430.0f, Cooldown = 0.74f, ActiveTime = 0.19f, HitStop = 0.090f },
                _ => new AttackProfile { Damage = 2, Knockback = 300.0f, Cooldown = 0.46f, ActiveTime = 0.15f, HitStop = 0.055f }
            };
        }

        return comboStep switch
        {
            2 => new AttackProfile
            {
                Damage = 1,
                Knockback = 215.0f,
                Cooldown = 0.27f,
                ActiveTime = 0.105f,
                HitStop = 0.035f
            },
            3 => new AttackProfile
            {
                Damage = 2,
                Knockback = 275.0f,
                Cooldown = 0.43f,
                ActiveTime = 0.13f,
                HitStop = 0.055f
            },
            _ => new AttackProfile
            {
                Damage = 1,
                Knockback = 190.0f,
                Cooldown = 0.24f,
                ActiveTime = 0.095f,
                HitStop = 0.035f
            }
        };
    }

    private static double WindupFor(int comboStep)
    {
        return WindupFor(comboStep, PlayerWeaponKind.Sword);
    }

    private static double WindupFor(int comboStep, PlayerWeaponKind weapon)
    {
        if (weapon == PlayerWeaponKind.Axe)
        {
            return comboStep switch
            {
                2 => 0.13,
                3 => 0.19,
                _ => 0.11
            };
        }

        if (weapon == PlayerWeaponKind.Hammer)
        {
            return comboStep switch
            {
                2 => 0.22,
                3 => 0.30,
                _ => 0.18
            };
        }

        return comboStep switch
        {
            2 => 0.065,
            3 => 0.115,
            _ => 0.075
        };
    }

    private static Vector2 HitboxSizeFor(int comboStep, Vector2 direction, PlayerWeaponKind weapon)
    {
        if (weapon == PlayerWeaponKind.Axe)
        {
            if (direction.Y != 0.0f)
                return comboStep == 3 ? new Vector2(24, 36) : new Vector2(22, 30);

            return comboStep == 3 ? new Vector2(38, 24) : comboStep == 2 ? new Vector2(34, 22) : new Vector2(30, 20);
        }

        if (weapon == PlayerWeaponKind.Hammer)
        {
            if (direction.Y != 0.0f)
                return comboStep == 3 ? new Vector2(24, 34) : new Vector2(22, 28);

            return comboStep == 3 ? new Vector2(34, 26) : comboStep == 2 ? new Vector2(30, 24) : new Vector2(26, 22);
        }

        if (direction.Y != 0.0f)
            return comboStep == 3 ? new Vector2(18, 30) : new Vector2(16, 24);

        return comboStep == 3 ? new Vector2(30, 17) : comboStep == 2 ? new Vector2(24, 14) : new Vector2(20, 12);
    }

    private static float RangeFor(int comboStep, PlayerWeaponKind weapon)
    {
        if (weapon == PlayerWeaponKind.Axe)
            return comboStep == 3 ? 28.0f : comboStep == 2 ? 25.0f : 22.0f;

        if (weapon == PlayerWeaponKind.Hammer)
            return comboStep == 3 ? 22.0f : comboStep == 2 ? 20.0f : 18.0f;

        return comboStep == 3 ? 23.0f : comboStep == 2 ? 20.0f : 18.0f;
    }
}
