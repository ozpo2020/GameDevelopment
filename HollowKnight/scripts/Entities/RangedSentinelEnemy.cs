using Godot;

public partial class RangedSentinelEnemy : EnemyBase
{
    private Vector2 _aimDirection = Vector2.Left;
    private double _shootCooldown = 0.85;
    private double _windupTimer;

    public RangedSentinelEnemy()
    {
        EntityId = "RangedSentinel";
        MaxHealth = 4;
        MoveSpeed = 0.0f;
        ContactDamage = 1;
        ContactKnockback = 150.0f;
    }

    protected override Vector2 ComputeVelocity(double delta)
    {
        var velocity = new Vector2(0.0f, Velocity.Y + 760.0f * (float)delta);
        if (IsOnFloor() && Mathf.Abs(velocity.Y) < 1.0f)
            velocity.Y = 0.0f;

        if (Player == null)
            return velocity;

        var toPlayer = Player.GlobalPosition - GlobalPosition;
        if (toPlayer.Length() > 250.0f)
        {
            _shootCooldown = Mathf.Min(_shootCooldown, 0.45);
            return velocity;
        }

        _aimDirection = toPlayer.Normalized();

        if (_windupTimer > 0.0)
        {
            _windupTimer -= delta;
            if (_windupTimer <= 0.0)
                Fire();

            return velocity;
        }

        _shootCooldown -= delta;
        if (_shootCooldown <= 0.0)
            _windupTimer = 0.34;

        return velocity;
    }

    private void Fire()
    {
        var projectile = new EnemyProjectile();
        GetParent()?.AddChild(projectile);
        var muzzle = GlobalPosition + _aimDirection * 17.0f + new Vector2(0, -5);
        projectile.Configure(muzzle, _aimDirection * 132.0f, 2.8, 1, 175.0f);
        _shootCooldown = 1.35;
    }

    public override void _Draw()
    {
        var shell = FlashTimer > 0.0 ? Colors.White : new Color(0.58f, 0.73f, 0.76f);
        var trim = new Color(0.26f, 0.34f, 0.38f);
        var outline = new Color(0.035f, 0.045f, 0.052f);
        var charge = _windupTimer > 0.0 ? Mathf.Abs(Mathf.Sin((float)_windupTimer * 34.0f)) : 0.0f;
        var glow = new Color(0.34f, 0.88f, 1.0f, 0.18f + charge * 0.26f);
        var facing = _aimDirection.X >= 0.0f ? 1 : -1;
        var breathe = Mathf.Round(Mathf.Sin(AnimTime * 4.2f));

        DrawCircle(new Vector2(facing * 9, -11 + breathe), 13.0f, glow);
        DrawRect(new Rect2(new Vector2(-11, -18 + breathe), new Vector2(22, 24)), outline);
        DrawRect(new Rect2(new Vector2(-9, -17 + breathe), new Vector2(18, 22)), shell.Darkened(0.06f));
        DrawRect(new Rect2(new Vector2(-7, -15 + breathe), new Vector2(14, 17)), shell);
        DrawRect(new Rect2(new Vector2(-5, -9 + breathe), new Vector2(10, 7)), trim);
        DrawRect(new Rect2(new Vector2(-3, -7 + breathe), new Vector2(6, 3)), glow.Lightened(0.35f));

        DrawRect(new Rect2(new Vector2(facing > 0 ? 7 : -19, -13 + breathe), new Vector2(12, 7)), outline);
        DrawRect(new Rect2(new Vector2(facing > 0 ? 8 : -18, -12 + breathe), new Vector2(10, 5)), trim.Lightened(0.16f));
        DrawRect(new Rect2(new Vector2(facing > 0 ? 16 : -19, -11 + breathe), new Vector2(4, 3)), glow.Lightened(0.2f));

        DrawRect(new Rect2(new Vector2(-8, 4), new Vector2(5, 5)), outline);
        DrawRect(new Rect2(new Vector2(3, 4), new Vector2(5, 5)), outline);
        DrawRect(new Rect2(new Vector2(-12, -2 + breathe), new Vector2(24, 3)), shell.Darkened(0.22f));
    }
}
