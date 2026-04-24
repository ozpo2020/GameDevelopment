using Godot;

public partial class SettingsPresenter : Node
{
    [Export] public float MasterVolume { get; set; } = 1.0f;
    [Export] public bool ShowDebugOverlay { get; set; } = true;
}

