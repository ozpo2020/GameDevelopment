using Godot;

public partial class KnockbackComponent : Node
{
    [Export] public float Recovery { get; set; } = 900.0f;

    public Vector2 Velocity { get; private set; }

    public void Apply(Vector2 knockback)
    {
        Velocity = knockback;
    }

    public Vector2 Tick(double delta)
    {
        var current = Velocity;
        Velocity = Velocity.MoveToward(Vector2.Zero, Recovery * (float)delta);
        return current;
    }
}

