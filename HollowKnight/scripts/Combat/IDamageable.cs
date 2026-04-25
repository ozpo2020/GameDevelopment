public interface IDamageable
{
    DamageTeam Team { get; }
    void TakeDamage(DamageInfo info);
}

