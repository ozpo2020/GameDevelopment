using Godot;

public partial class PlayerAttackState : PlayerStateBase
{
    public PlayerAttackState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        Player.Animation.SetState(PlayerStateNames.Attack);
    }

    public override void Update(double delta)
    {
        if (!Player.IsPerformingAttack)
            TransitionToMovementState();
    }
}