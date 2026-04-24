using Godot;

public partial class Hurtbox : Area2D
{
	// 定义一个信号，让外部（比如血条、动画控制器）知道自己受伤了
	[Signal] public delegate void ReceivedDamageEventHandler(int damage);

	public void TakeDamage(int damage, Vector2 knockback)
	{
		// 1. 发射信号 (UI 或 逻辑层处理)
		EmitSignal(SignalName.ReceivedDamage, damage);
		
		// 2. 打印调试信息（上线后删掉）
		GD.Print($"受击盒收到伤害: {damage}");
		
		// 3. 可以在这里做一些“变红闪烁”或“击退”的处理
		// GetParent<CharacterBody2D>().Velocity += knockback; 
	}
}
