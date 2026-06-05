// Assets/Scripts/Combat/HealthComponent.cs
using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour, IDamageable
{
    // ── Inspector ────────────────────────────────────────
    [Header("HP")]
    public int maxHP = 3;

    [Header("Events")]
    public UnityEvent<int, int> onHPChanged;  // (currentHP, maxHP)
    public UnityEvent<DamageSource> onDeath;
    public UnityEvent<Vector2> onKnockback;
    public UnityEvent<int, DamageSource> onDamaged;

    // ── IDamageable ──────────────────────────────────────
    public bool IsAlive { get; private set; } = true;
    public int CurrentHP { get; private set; }
    public int MaxHP => maxHP;

    // ── Unity lifecycle ──────────────────────────────────
    void Awake()
    {
        CurrentHP = maxHP;
    }

    public void ResetHP()
    {
        IsAlive = true;
        CurrentHP = maxHP;
        onHPChanged?.Invoke(CurrentHP, maxHP);
    }

    // ── IDamageable impl ─────────────────────────────────
    public void TakeDamage(int amount, DamageSource source)
    {
        if (!IsAlive) return;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        onHPChanged?.Invoke(CurrentHP, maxHP);

        Debug.Log($"[Health] {gameObject.name} HP: {CurrentHP}/{maxHP}");

        if (CurrentHP == 0)
        {
            IsAlive = false;
            onDeath?.Invoke(source);  // fire death TRƯỚC
            Die();
            // onDamaged KHÔNG fire khi chết — tránh Hit animation chạy sau khi dead
            return;
        }

        onDamaged?.Invoke(amount, source); // chỉ fire khi còn sống
    }

    public void TakeDamage(int amount, DamageSource source, Vector2 knockbackDir)
    {
        if (!IsAlive) return;

        onKnockback?.Invoke(knockbackDir);
        TakeDamage(amount, source);
    }

    public void Die()
    {
        Debug.Log($"[Health] {gameObject.name} died.");
    }
}