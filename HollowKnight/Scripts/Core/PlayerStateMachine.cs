// PlayerStateMachine.cs
public partial class PlayerStateMachine : Node {
    private IState _currentState;
    public void ChangeState(IState newState) {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }
}