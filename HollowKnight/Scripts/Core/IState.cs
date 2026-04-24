using Godot;

public interface IState
{
    void Enter();
    void Exit();
    void Update(double delta);
    void PhysicsUpdate(double delta);
    void HandleInput(InputEvent @event);
}