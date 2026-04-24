using Godot;

public partial class AttackProfile : Resource
{
	[Export] public int Damage { get; set; } = 1;
	[Export] public float Knockback { get; set; } = 170.0f;
	[Export] public float Cooldown { get; set; } = 0.28f;
	[Export] public float ActiveTime { get; set; } = 0.11f;
	[Export] public float HitStop { get; set; } = 0.035f;
}
