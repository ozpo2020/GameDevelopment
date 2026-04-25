using Godot;

public partial class SignalBus : Node
{
    public static SignalBus Instance { get; private set; }

    [Signal] public delegate void RoomChangedEventHandler(string roomId);
    [Signal] public delegate void PlayerHealthChangedEventHandler(int current, int maximum);
    [Signal] public delegate void AbilityUnlockedEventHandler(string abilityId);
    [Signal] public delegate void CheckpointActivatedEventHandler(string roomId, Vector2 position);
    [Signal] public delegate void BossDefeatedEventHandler();

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }
}

