using Godot;

public partial class AbilityGate : Area2D
{
    private GameRoot _root;
    private string _abilityId;
    private string _targetRoom;
    private string _targetSpawn;
    private Vector2 _size;
    private SolidBlock _blocker;
    private float _pulse;

    public void Configure(GameRoot root, Rect2 rect, string abilityId, string targetRoom, string targetSpawn)
    {
        _root = root;
        _abilityId = abilityId;
        _targetRoom = targetRoom;
        _targetSpawn = targetSpawn;
        _size = rect.Size;
        Position = rect.Position + rect.Size * 0.5f;
        CollisionLayer = CollisionLayers.Trigger;
        CollisionMask = CollisionLayers.PlayerBody;
        Monitoring = true;
        BodyEntered += OnBodyEntered;
        AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = rect.Size } });

        _blocker = new SolidBlock();
        AddChild(_blocker);
        _blocker.Configure(new Rect2(-rect.Size * 0.5f, rect.Size), new Color(0.24f, 0.18f, 0.29f));
    }

    public override void _Process(double delta)
    {
        _pulse += (float)delta;
        if (_root.Player.Abilities.HasDash && IsInstanceValid(_blocker))
        {
            _blocker.QueueFree();
            _blocker = null;
        }
        QueueRedraw();
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not PlayerController)
            return;

        if (_root.Player.Abilities.HasDash)
            _root.TransitionTo(_targetRoom, _targetSpawn);
        else
            _root.Hud.ShowBanner("A SEALED PATH NEEDS DASH");
    }

    public override void _Draw()
    {
        var pulse = 0.5f + Mathf.Sin(_pulse * 5.0f) * 0.5f;
        var color = _root?.Player?.Abilities.HasDash == true
            ? new Color(0.28f, 0.82f, 0.72f, 0.28f)
            : new Color(0.76f, 0.42f, 0.94f, 0.35f);
        DrawRect(new Rect2(-_size * 0.5f, _size), color);
        DrawRect(new Rect2(new Vector2(-_size.X * 0.5f + 4, -_size.Y * 0.5f + 8), new Vector2(_size.X - 8, 4)), color.Lightened(0.28f + pulse * 0.18f));
        DrawRect(new Rect2(new Vector2(-_size.X * 0.5f + 4, _size.Y * 0.5f - 12), new Vector2(_size.X - 8, 4)), color.Lightened(0.18f));
    }
}
