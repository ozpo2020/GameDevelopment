using Godot;

public partial class PlayerAbilities : Node
{
    [Export] public bool HasDash { get; set; }
    [Export] public bool HasDoubleJump { get; set; }
    [Export] public bool HasWallClimb { get; set; }
}

