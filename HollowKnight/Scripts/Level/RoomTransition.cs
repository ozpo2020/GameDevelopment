using Godot;

public partial class RoomTransition : Area2D
{
    private GameRoot _root;
    private string _targetRoom;
    private string _targetSpawn;
    private Vector2 _size;
    private float _pulse;

    public void Configure(GameRoot root, Vector2 position, Vector2 size, string targetRoom, string targetSpawn)
    {
        _root = root;
        _targetRoom = targetRoom;
        _targetSpawn = targetSpawn;
        _size = size;
        Position = position;
        CollisionLayer = CollisionLayers.Trigger;
        CollisionMask = CollisionLayers.PlayerBody;
        Monitoring = true;
        BodyEntered += OnBodyEntered;
        AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = size } });
    }

    public override void _Process(double delta)
    {
        _pulse += (float)delta;
        QueueRedraw();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is PlayerController)
            Callable.From(() => _root.TransitionTo(_targetRoom, _targetSpawn)).CallDeferred();
    }

    public override void _Draw()
    {
        var glow = 0.42f + Mathf.Sin(_pulse * 4.0f) * 0.16f;
        DrawRect(new Rect2(-_size * 0.5f - new Vector2(3, 3), _size + new Vector2(6, 6)), new Color(0.20f, 0.60f, 0.66f, 0.15f + glow * 0.12f));
        DrawRect(new Rect2(-_size * 0.5f, _size), new Color(0.42f, 0.82f, 0.80f, 0.22f + glow * 0.18f));
        DrawRect(new Rect2(new Vector2(-2, -_size.Y * 0.5f), new Vector2(4, _size.Y)), new Color(0.78f, 0.96f, 0.92f, 0.32f));
    }
}
