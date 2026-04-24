using Godot;

public partial class MenuController : Control
{
    [Signal] public delegate void ResumeRequestedEventHandler();

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed(InputBindings.Pause))
            EmitSignal(SignalName.ResumeRequested);
    }
}

