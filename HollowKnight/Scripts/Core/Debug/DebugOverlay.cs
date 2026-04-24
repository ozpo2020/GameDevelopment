using Godot;

public partial class DebugOverlay : CanvasLayer
{
    private Label _label;
    private PlayerController _player;
    private string _roomId = "Room_001_Start";

    public override void _Ready()
    {
        Layer = 20;
        _label = new Label
        {
            Position = new Vector2(8, 154),
            Size = new Vector2(304, 20),
            Text = "Debug"
        };
        _label.AddThemeFontSizeOverride("font_size", 9);
        _label.AddThemeColorOverride("font_color", new Color(0.76f, 0.86f, 0.92f, 0.8f));
        AddChild(_label);
    }

    public void Watch(PlayerController player)
    {
        _player = player;
    }

    public void SetRoom(string roomId)
    {
        _roomId = roomId;
    }

    public override void _Process(double delta)
    {
        if (_player == null)
            return;

        _label.Text = $"FPS {Engine.GetFramesPerSecond()} | {_roomId} | {_player.StateText}";
    }
}

