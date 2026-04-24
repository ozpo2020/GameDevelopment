using Godot;

public abstract partial class PlayerStateBase : IState
{
    protected PlayerController Player { get; }
    protected PlayerMotor Motor { get; }
    protected PlayerCombat Combat { get; }
    protected PlayerStateMachine Machine { get; }

    protected PlayerStateBase(PlayerController player)
    {
        Player = player;
        Motor = player.GetNode<PlayerMotor>("PlayerMotor");
        Combat = player.GetNode<PlayerCombat>("PlayerCombat");
        Machine = player.StateMachine;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(double delta) { }
    public virtual void PhysicsUpdate(double delta) { }
    public virtual void HandleInput(InputEvent @event) { }

    protected bool HasMoveInput()
    {
        return Mathf.Abs(Input.GetAxis(InputBindings.MoveLeft, InputBindings.MoveRight)) > 0.1f;
    }

    protected void TransitionToMovementState()
    {
        if (!Player.IsOnFloor())
        {
            Machine.ChangeState(PlayerStateNames.Jump);
            return;
        }

        if (HasMoveInput())
            Machine.ChangeState(PlayerStateNames.Run);
        else
            Machine.ChangeState(PlayerStateNames.Idle);
    }
}