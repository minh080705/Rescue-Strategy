// Assets/Scripts/Combat/IDamageable.cs
// Interface dùng chung cho mọi thứ có thể nhận damage.
// Không hardcode Player/Enemy — bất kỳ GameObject nào implement
// interface này đều tham gia được vào combat system.

public interface IDamageable
{
    bool  IsAlive    { get; }
    int   CurrentHP  { get; }
    int   MaxHP      { get; }

    void TakeDamage(int amount, DamageSource source);
    void Die();
}