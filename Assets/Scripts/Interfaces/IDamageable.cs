// IDamageable.cs
public interface IDamageable
{
    void TakeDamage(int amount);
    void Heal(int amount);
    bool IsAlive { get; }
}