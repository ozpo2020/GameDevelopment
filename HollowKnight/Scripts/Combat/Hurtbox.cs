// Hurtbox.cs (附加在任何可受伤物体的节点上)
public partial class Hurtbox : Area2D, IDamageable {
	public void TakeDamage(int damage, Vector2 knockback) {
		// 触发受伤逻辑，比如播放动画、扣血
		EmitSignal(SignalName.ReceivedDamage, damage);
	}
}
