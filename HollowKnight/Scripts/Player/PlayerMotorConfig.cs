using Godot;

[Tool]
public partial class PlayerMotorConfig : Resource
{
    [Export] public float MaxSpeed { get; set; } = 126.0f;
    [Export] public float Acceleration { get; set; } = 1120.0f;
    [Export] public float Friction { get; set; } = 1280.0f;
    [Export] public float JumpVelocity { get; set; } = -252.0f;
    [Export] public float Gravity { get; set; } = 760.0f;
    [Export] public float FallGravityMultiplier { get; set; } = 1.28f;
    [Export] public float CoyoteTime { get; set; } = 0.12f;
    [Export] public float JumpBufferTime { get; set; } = 0.13f;
    [Export] public float DashSpeed { get; set; } = 292.0f;
    [Export] public float DashTime { get; set; } = 0.15f;
    [Export] public float DashCooldown { get; set; } = 0.48f;
}