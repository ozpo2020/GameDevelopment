// IState.cs
public interface IState
{
    void Enter();
    void HandleInput(InputEvent @event);
    void Update(double delta);
    void PhysicsUpdate(double delta);
    void Exit();
}