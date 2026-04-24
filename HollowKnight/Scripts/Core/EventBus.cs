public partial class EventBus : Node {
    public static EventBus Instance { get; private set; }
    [Signal] public delegate void PlayerHealthChangedEventHandler(int newHealth);
    
    public override void _Ready() { Instance = this; }
}