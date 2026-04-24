using Godot;

public partial class MapMarker : Resource
{
    [Export] public string RoomId { get; set; } = "";
    [Export] public Vector2 MapPosition { get; set; }
    [Export] public bool HasCheckpoint { get; set; }
    [Export] public bool HasBoss { get; set; }
}

