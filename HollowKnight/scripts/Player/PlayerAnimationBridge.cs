using Godot;

public partial class PlayerAnimationBridge : Node
{
    public string CurrentState { get; private set; } = "Idle";

    public void SetState(string state)
    {
        CurrentState = state;
    }
}

