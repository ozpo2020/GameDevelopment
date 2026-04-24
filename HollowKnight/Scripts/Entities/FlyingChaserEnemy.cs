using Godot;

public partial class FlyingChaserEnemy : EnemyBase
{
    private float _hoverPhase;

    public FlyingChaserEnemy()
    {
        EntityId = "FlyingChaser";
        MaxHealth = 2;
        MoveSpeed = 52.0f;
        ContactKnockback = 155.0f;
    }

    protected override Vector2 ComputeVelocity(double delta)
    {
        _hoverPhase += (float)delta * 4.0f;

        if (Player == null)
            return new Vector2(0, Mathf.Sin(_hoverPhase) * 10.0f);

        var toPlayer = Player.GlobalPosition - GlobalPosition;
        var steering = toPlayer.Length() < 180.0f ? toPlayer.Normalized() * MoveSpeed : Vector2.Zero;
        steering.Y += Mathf.Sin(_hoverPhase) * 18.0f;
        return steering;
    }

    public override void _Draw()
    {
        var core = FlashTimer > 0.0 ? Colors.White : new Color(0.72f, 0.48f, 0.94f);
        var wing = new Color(0.38f, 0.52f, 0.82f);
        var outline = new Color(0.07f, 0.04f, 0.11f);
        var flap = Mathf.Sin(_hoverPhase * 4.0f);
        var wingLift = Mathf.Round(flap * 3.0f);
        var glow = 0.18f + Mathf.Abs(flap) * 0.12f;
        var facing = Player != null && Player.GlobalPosition.X < GlobalPosition.X ? -1 : 1;

        DrawCircle(Vector2.Zero, 14.0f, new Color(0.42f, 0.45f, 0.95f, glow));
        DrawRect(new Rect2(new Vector2(-18, -5 - wingLift), new Vector2(11, 8)), outline);
        DrawRect(new Rect2(new Vector2(7, -5 + wingLift), new Vector2(11, 8)), outline);
        DrawRect(new Rect2(new Vector2(-17, -4 - wingLift), new Vector2(9, 6)), wing);
        DrawRect(new Rect2(new Vector2(8, -4 + wingLift), new Vector2(9, 6)), wing);
        DrawRect(new Rect2(new Vector2(-15, 1 - wingLift), new Vector2(6, 2)), wing.Darkened(0.22f));
        DrawRect(new Rect2(new Vector2(9, 1 + wingLift), new Vector2(6, 2)), wing.Darkened(0.22f));

        DrawRect(new Rect2(new Vector2(-10, -10), new Vector2(20, 18)), outline);
        DrawRect(new Rect2(new Vector2(-8, -9), new Vector2(16, 16)), core);
        DrawRect(new Rect2(new Vector2(-6, -7), new Vector2(12, 4)), core.Lightened(0.22f));
        DrawRect(new Rect2(new Vector2(-5, 2), new Vector2(10, 4)), core.Darkened(0.25f));
        DrawRect(new Rect2(new Vector2(facing > 0 ? 1 : -5, -3), new Vector2(5, 4)), new Color(0.08f, 0.04f, 0.12f));
        DrawRect(new Rect2(new Vector2(facing > 0 ? 3 : -4, -2), new Vector2(2, 1)), new Color(0.84f, 0.95f, 1.0f, 0.75f));
        DrawRect(new Rect2(new Vector2(-2, 7), new Vector2(4, 5)), outline);
    }
}
