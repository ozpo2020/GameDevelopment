using Godot;

public partial class SaveData : Resource
{
    [Export] public string CheckpointRoom { get; set; } = "Room_001_Start";
    [Export] public Vector2 CheckpointPosition { get; set; } = new(56, 126);
    [Export] public bool HasDash { get; set; }
    [Export] public bool BossDefeated { get; set; }
}

