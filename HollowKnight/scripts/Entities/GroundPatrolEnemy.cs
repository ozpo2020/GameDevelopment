using Godot;

public partial class GroundPatrolEnemy : EnemyBase
{
    private float _leftX;
    private float _rightX;
    private int _direction = -1;
    private bool _alerted;

    public GroundPatrolEnemy()
    {
        EntityId = "GroundPatrol";
        MaxHealth = 3;
        MoveSpeed = 44.0f;
    }

    public void SetPatrol(float leftX, float rightX)
    {
        _leftX = leftX;
        _rightX = rightX;
    }

    protected override Vector2 ComputeVelocity(double delta)
    {
        if (Player != null && GlobalPosition.DistanceTo(Player.GlobalPosition) < 86.0f)
        {
            _direction = Player.GlobalPosition.X > GlobalPosition.X ? 1 : -1;
            _alerted = true;
        }
        else if (GlobalPosition.X < _leftX)
        {
            _direction = 1;
            _alerted = false;
        }
        else if (GlobalPosition.X > _rightX)
        {
            _direction = -1;
            _alerted = false;
        }

        var velocity = new Vector2(_direction * MoveSpeed, Velocity.Y + 760.0f * (float)delta);
        if (IsOnFloor() && Mathf.Abs(velocity.Y) < 1.0f)
            velocity.Y = 0.0f;
        return velocity;
    }

    public override void _Draw()
    {
        var shell = FlashTimer > 0.0 ? Colors.White : new Color(0.52f, 0.78f, 0.43f);
        var belly = new Color(0.30f, 0.44f, 0.30f);
        var outline = new Color(0.04f, 0.07f, 0.05f);
        var step = Mathf.Sin(AnimTime * 12.0f);
        var footOffset = step > 0.0f ? 1 : -1;
        var breathe = Mathf.Sin(AnimTime * 5.0f) * 1.0f;
        var lunge = _alerted ? Mathf.Abs(Mathf.Sin(AnimTime * 7.0f)) * 2.0f : 0.0f;
        var lean = _direction * lunge;

        DrawRect(new Rect2(new Vector2(-11 + lean, -12 + breathe), new Vector2(22, 14)), outline);
        DrawRect(new Rect2(new Vector2(-10 + lean, -15 + breathe), new Vector2(20, 5)), shell.Lightened(0.14f));
        DrawRect(new Rect2(new Vector2(-9 + lean, -10 + breathe), new Vector2(18, 11)), shell);
        DrawRect(new Rect2(new Vector2(-7 + lean, -7 + breathe), new Vector2(14, 6)), belly);

        for (var i = 0; i < 4; i++)
        {
            var x = -8 + i * 5 + lean;
            DrawRect(new Rect2(new Vector2(x, -14 + breathe), new Vector2(2, 4)), shell.Lightened(0.22f));
            DrawRect(new Rect2(new Vector2(x + 1, -13 + breathe), new Vector2(1, 3)), outline.Lightened(0.08f));
        }

        var eyeX = _direction > 0 ? 4 : -6;
        DrawRect(new Rect2(new Vector2(eyeX + lean, -8 + breathe), new Vector2(3, 2)), _alerted ? new Color(0.95f, 0.46f, 0.20f) : outline);
        DrawRect(new Rect2(new Vector2(_direction > 0 ? 9 + lean : -12 + lean, -5 + breathe), new Vector2(4, 5)), outline);
        DrawRect(new Rect2(new Vector2(_direction > 0 ? 10 + lean : -11 + lean, -4 + breathe), new Vector2(2, 3)), shell.Darkened(0.16f));

        DrawRect(new Rect2(new Vector2(-9 + lean, 0 + footOffset), new Vector2(5, 4)), outline);
        DrawRect(new Rect2(new Vector2(-2 + lean, 1 - footOffset), new Vector2(4, 3)), outline);
        DrawRect(new Rect2(new Vector2(4 + lean, 0 + footOffset), new Vector2(5, 4)), outline);
        DrawRect(new Rect2(new Vector2(-10 + lean, -1 + breathe), new Vector2(20, 2)), shell.Darkened(0.25f));
    }
}
