using Godot;

public partial class StateMachine : Node
{
    public State Current { get; private set; }

    public void Change(State next)
    {
        if (Current == next)
            return;

        Current?.Exit();
        Current = next;
        Current?.Enter();
    }

    public override void _Process(double delta)
    {
        Current?.Tick(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        Current?.PhysicsTick(delta);
    }
}

