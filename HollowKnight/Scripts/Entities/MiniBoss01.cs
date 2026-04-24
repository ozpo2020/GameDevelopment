using Godot;

public partial class MiniBoss01 : EnemyBase
{
    private double _attackTimer = 1.0;
    private double _dashTimer;
    private double _windupTimer;
    private double _volleyTimer;
    private float _pendingDashDirection = 1.0f;
    private Vector2 _dashVelocity;
    private int _patternIndex = -1;
    private int _volleyShots;
    private bool _leaping;

    public MiniBoss01()
    {
        EntityId = "MiniBoss_01";
        MaxHealth = 12;
        MoveSpeed = 38.0f;
        ContactDamage = 1;
        ContactKnockback = 235.0f;
    }

    protected override Vector2 ComputeVelocity(double delta)
    {
        if (Player == null)
            return Vector2.Zero;

        _attackTimer -= delta;
        _dashTimer = Mathf.Max(0.0, _dashTimer - delta);

        if (_dashTimer > 0.0)
        {
            _dashVelocity.Y += 820.0f * (float)delta;
            return _dashVelocity;
        }

        if (_leaping)
        {
            _leaping = false;
            SpawnShockwave();
        }

        if (_windupTimer > 0.0)
        {
            _windupTimer = Mathf.Max(0.0, _windupTimer - delta);
            if (_windupTimer <= 0.0)
            {
                _dashVelocity = new Vector2(_pendingDashDirection * 205.0f, -55.0f);
                _dashTimer = 0.32;
                return _dashVelocity;
            }

            return new Vector2(-_pendingDashDirection * 18.0f, Velocity.Y + 820.0f * (float)delta);
        }

        if (_volleyShots > 0)
        {
            _volleyTimer -= delta;
            if (_volleyTimer <= 0.0)
                FireVolleyShot();

            return new Vector2(0.0f, Velocity.Y + 820.0f * (float)delta);
        }

        if (_attackTimer <= 0.0)
        {
            StartNextPattern();
            return new Vector2(-_pendingDashDirection * 14.0f, Velocity.Y + 820.0f * (float)delta);
        }

        var chase = Player.GlobalPosition.X > GlobalPosition.X ? 1.0f : -1.0f;
        return new Vector2(chase * MoveSpeed, Velocity.Y + 820.0f * (float)delta);
    }

    private void StartNextPattern()
    {
        var enraged = Health != null && Health.CurrentHealth <= MaxHealth / 2;
        _pendingDashDirection = Player.GlobalPosition.X > GlobalPosition.X ? 1.0f : -1.0f;
        _patternIndex = (_patternIndex + 1) % 3;

        if (_patternIndex == 0)
        {
            _windupTimer = enraged ? 0.20 : 0.28;
            _attackTimer = enraged ? 1.18 : 1.45;
            return;
        }

        if (_patternIndex == 1)
        {
            _volleyShots = enraged ? 3 : 2;
            _volleyTimer = enraged ? 0.28 : 0.38;
            _attackTimer = enraged ? 1.85 : 2.20;
            return;
        }

        _dashVelocity = new Vector2(_pendingDashDirection * (enraged ? 142.0f : 118.0f), enraged ? -245.0f : -215.0f);
        _dashTimer = enraged ? 0.50 : 0.46;
        _leaping = true;
        _attackTimer = enraged ? 1.45 : 1.72;
    }

    private void FireVolleyShot()
    {
        if (Player == null)
            return;

        var enraged = Health != null && Health.CurrentHealth <= MaxHealth / 2;
        var toPlayer = (Player.GlobalPosition + new Vector2(0, -8) - GlobalPosition).Normalized();
        var spread = (_volleyShots % 2 == 0 ? -0.10f : 0.10f) * (enraged ? 1.15f : 0.85f);
        SpawnProjectile(toPlayer.Rotated(spread), enraged ? 124.0f : 108.0f);
        _volleyShots--;
        _volleyTimer = enraged ? 0.34 : 0.44;
    }

    private void SpawnShockwave()
    {
        SpawnProjectile(Vector2.Left, 88.0f);
        SpawnProjectile(Vector2.Right, 88.0f);
    }

    private void SpawnProjectile(Vector2 direction, float speed)
    {
        var projectile = new EnemyProjectile();
        GetParent()?.AddChild(projectile);
        projectile.Configure(GlobalPosition + new Vector2(direction.X * 18.0f, -16), direction.Normalized() * speed, 2.2, 1, 135.0f);
    }

    public override void _Draw()
    {
        var enraged = Health != null && Health.CurrentHealth <= MaxHealth / 2;
        var armor = FlashTimer > 0.0 ? Colors.White : enraged ? new Color(0.93f, 0.34f, 0.31f) : new Color(0.76f, 0.72f, 0.60f);
        var shadow = new Color(0.08f, 0.06f, 0.055f);
        var core = enraged ? new Color(1.0f, 0.70f, 0.36f) : new Color(0.52f, 0.84f, 0.78f);
        var charging = _windupTimer > 0.0 || _volleyShots > 0;
        var windup = charging ? Mathf.Sin((float)(_windupTimer + _volleyTimer) * 38.0f) * 2.0f : 0.0f;
        var dashLean = _dashTimer > 0.0 ? Mathf.Sign(Velocity.X) * 5.0f : Mathf.Sign(Velocity.X) * 2.0f;
        var breathe = Mathf.Sin(AnimTime * 3.2f) * 1.0f;
        var lean = dashLean + windup;

        if (_dashTimer > 0.0)
            DrawFacingRect(new Vector2(-Mathf.Sign(Velocity.X) * 38.0f, -22), new Vector2(26.0f, 9), Mathf.Sign(Velocity.X), new Color(1.0f, 0.62f, 0.34f, 0.22f));

        DrawRect(new Rect2(new Vector2(-20 + lean, -31 + breathe), new Vector2(40, 34)), shadow);
        DrawRect(new Rect2(new Vector2(-18 + lean, -30 + breathe), new Vector2(36, 31)), armor.Darkened(0.06f));
        DrawRect(new Rect2(new Vector2(-15 + lean, -27 + breathe), new Vector2(30, 25)), armor);
        DrawRect(new Rect2(new Vector2(-12 + lean, -23 + breathe), new Vector2(24, 18)), armor.Darkened(0.18f));
        DrawRect(new Rect2(new Vector2(-5 + lean, -18 + breathe), new Vector2(10, 8)), core);
        DrawRect(new Rect2(new Vector2(-3 + lean, -16 + breathe), new Vector2(6, 4)), core.Lightened(0.28f));

        DrawRect(new Rect2(new Vector2(-16 + lean, -42 + breathe), new Vector2(7, 14)), shadow);
        DrawRect(new Rect2(new Vector2(9 + lean, -42 + breathe), new Vector2(7, 14)), shadow);
        DrawRect(new Rect2(new Vector2(-14 + lean, -43 + breathe), new Vector2(4, 14)), armor.Lightened(0.18f));
        DrawRect(new Rect2(new Vector2(10 + lean, -43 + breathe), new Vector2(4, 14)), armor.Lightened(0.18f));
        DrawRect(new Rect2(new Vector2(-22 + lean, -20 + breathe), new Vector2(8, 14)), armor.Darkened(0.22f));
        DrawRect(new Rect2(new Vector2(14 + lean, -20 + breathe), new Vector2(8, 14)), armor.Darkened(0.22f));
        DrawRect(new Rect2(new Vector2(-8 + lean, -22 + breathe), new Vector2(4, 4)), charging ? core.Lightened(0.25f) : shadow);
        DrawRect(new Rect2(new Vector2(4 + lean, -22 + breathe), new Vector2(4, 4)), charging ? core.Lightened(0.25f) : shadow);
        if (_volleyShots > 0)
            DrawCircle(new Vector2(lean, -14 + breathe), 18.0f, new Color(0.34f, 0.86f, 1.0f, 0.18f));
        DrawRect(new Rect2(new Vector2(-14 + lean, 1), new Vector2(11, 6)), shadow);
        DrawRect(new Rect2(new Vector2(3 + lean, 1), new Vector2(11, 6)), shadow);
        DrawRect(new Rect2(new Vector2(-18 + lean, -4 + breathe), new Vector2(36, 3)), armor.Lightened(0.10f));
    }

    private void DrawFacingRect(Vector2 origin, Vector2 size, float side, Color color)
    {
        var position = side >= 0.0f ? origin : new Vector2(origin.X - size.X, origin.Y);
        DrawRect(new Rect2(position, size), color);
    }
}
