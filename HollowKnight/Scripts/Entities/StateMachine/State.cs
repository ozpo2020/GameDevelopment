using Godot;

public abstract partial class State : Node
{
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Tick(double delta) { }
    public virtual void PhysicsTick(double delta) { }
}

