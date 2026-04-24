using Godot;

public partial class InvincibilityComponent : Node
{
    private double _timer;

    public bool IsInvincible => _timer > 0.0;

    public override void _Process(double delta)
    {
        if (_timer > 0.0)
            _timer -= delta;
    }

    public void Start(float duration)
    {
        _timer = Mathf.Max((float)_timer, duration);
    }
}

