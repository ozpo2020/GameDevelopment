using Godot;

public static class CameraBounds
{
    public static void ApplyTo(Camera2D camera, Rect2 bounds)
    {
        camera.LimitLeft = Mathf.RoundToInt(bounds.Position.X);
        camera.LimitTop = Mathf.RoundToInt(bounds.Position.Y);
        camera.LimitRight = Mathf.RoundToInt(bounds.End.X);
        camera.LimitBottom = Mathf.RoundToInt(bounds.End.Y);
    }
}

