using Godot;

public partial class SolidBlock : StaticBody2D
{
    private Vector2 _size;
    private Color _color = new(0.20f, 0.24f, 0.26f);

    public void Configure(Rect2 rect, Color color)
    {
        _size = rect.Size;
        _color = color;
        Position = rect.Position + rect.Size * 0.5f;
        CollisionLayer = CollisionLayers.World;
        CollisionMask = CollisionLayers.PlayerBody | CollisionLayers.EnemyBody;
        var newBlock = new CollisionShape2D 
        { 
            Shape = new RectangleShape2D 
            { 
                Size = rect.Size 
            } 
        };
        AddChild.CallDeferred(newBlock);
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(-_size * 0.5f, _size), _color);
        DrawRect(new Rect2(-_size * 0.5f, new Vector2(_size.X, 3)), _color.Lightened(0.2f));
        DrawRect(new Rect2(new Vector2(-_size.X * 0.5f, _size.Y * 0.5f - 2), new Vector2(_size.X, 2)), _color.Darkened(0.24f));

        for (var x = 6.0f; x < _size.X - 4.0f; x += 18.0f)
        {
            var localX = -_size.X * 0.5f + x;
            DrawRect(new Rect2(new Vector2(localX, -_size.Y * 0.5f + 4), new Vector2(5, 1)), _color.Lightened(0.12f));
            DrawRect(new Rect2(new Vector2(localX + 3, _size.Y * 0.5f - 7), new Vector2(7, 1)), _color.Darkened(0.18f));
        }
    }
}
