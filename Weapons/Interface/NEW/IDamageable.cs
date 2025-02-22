public interface IDamageable
{
    void TakeDamage(int damageAmount);
    void TakeDamageOverTime(int damagePerTick, float duration, float tickRate);
}
