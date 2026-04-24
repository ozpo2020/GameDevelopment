using Godot;

public partial class Hitbox : Area2D
{
    // 在编辑器里可以直接设置伤害值
    [Export] public int Damage { get; set; } = 10;
    
    // 击退向量（如果需要的话）
    [Export] public Vector2 Knockback { get; set; } = Vector2.Zero;

    public override void _Ready()
    {
        // 连接 Area 进入的信号
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        // 检查碰撞到的物体是否具备 Hurtbox 组件
        if (area is Hurtbox hurtbox)
        {
            // 如果是，告诉它受伤了
            hurtbox.TakeDamage(Damage, Knockback);
        }
    }
}