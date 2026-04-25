using Godot;

public partial class Boot : Node
{
    public override void _Ready()
    {
        InputBindings.EnsureDefaults();
        GetTree().ChangeSceneToFile("res://scenes/GameRoot.tscn");
    }
}

