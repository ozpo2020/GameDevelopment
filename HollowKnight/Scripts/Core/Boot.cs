using Godot;

public partial class Boot : Node
{
    public override void _Ready()
    {
        InputBindings.EnsureDefaults();
        Callable.From(() =>GetTree().ChangeSceneToFile("res://scenes/GameRoot.tscn")).CallDeferred();
    }
}

