using Godot;

public partial class Checkpoint : Area2D
{
    private GameRoot _root;
    private float _pulse;

    public void Configure(GameRoot root, Vector2 position)
    {
        _root = root;
        Position = position;
        CollisionLayer = CollisionLayers.Trigger;
        CollisionMask = CollisionLayers.PlayerBody;
        Monitoring = true;
        BodyEntered += OnBodyEntered;
        AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = 13.0f } });
    }

    public override void _Process(double delta)
    {
        _pulse += (float)delta;
        QueueRedraw();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is PlayerController)
            _root.ActivateCheckpoint(GlobalPosition);
    }

    public override void _Draw()
    {
        var glow = 0.16f + Mathf.Sin(_pulse * 5.0f) * 0.06f;
        DrawCircle(new Vector2(0, -14), 18.0f, new Color(0.55f, 0.86f, 0.94f, glow));
        DrawRect(new Rect2(new Vector2(-5, -20), new Vector2(10, 22)), new Color(0.54f, 0.78f, 0.86f));
        DrawRect(new Rect2(new Vector2(-9, -23), new Vector2(18, 5)), new Color(0.80f, 0.92f, 0.95f));
        DrawRect(new Rect2(new Vector2(-2, -30), new Vector2(4, 7)), new Color(0.96f, 0.86f, 0.52f));
        DrawRect(new Rect2(new Vector2(-12, 0), new Vector2(24, 4)), new Color(0.14f, 0.18f, 0.20f));
    }
}
