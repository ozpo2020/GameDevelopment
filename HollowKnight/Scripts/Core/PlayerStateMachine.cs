using Godot;
using System.Collections.Generic;

public partial class PlayerStateMachine : Node
{
    public IState CurrentState { get; private set; }
    public string CurrentStateName { get; private set; } = string.Empty;
    private readonly Dictionary<string, IState> _states = new();

    public void AddState(string name, IState state)
    {
        _states[name] = state;
    }

    public void ChangeState(string name)
    {
        if (CurrentStateName == name || !_states.ContainsKey(name))
            return;

        CurrentState?.Exit();
        CurrentState = _states[name];
        CurrentStateName = name;
        CurrentState.Enter();
    }

    public void Update(double delta) => CurrentState?.Update(delta);
    public void PhysicsUpdate(double delta) => CurrentState?.PhysicsUpdate(delta);
    public void HandleInput(InputEvent @event) => CurrentState?.HandleInput(@event);
}