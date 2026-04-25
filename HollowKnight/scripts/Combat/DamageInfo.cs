using Godot;

public readonly struct DamageInfo
{
    public readonly int Amount;
    public readonly Vector2 Knockback;
    public readonly Node2D Source;
    public readonly DamageTeam Team;

    public DamageInfo(int amount, Vector2 knockback, Node2D source, DamageTeam team)
    {
        Amount = amount;
        Knockback = knockback;
        Source = source;
        Team = team;
    }
}

