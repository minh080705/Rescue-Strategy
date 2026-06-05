// Assets/Scripts/Combat/HitboxComponent.cs
// Hitbox dùng chung. Attach vào bất kỳ GameObject nào cần gây damage.
// Hoạt động bằng cách bật/tắt Collider2D — không biết owner là ai.
//
// Cách dùng:
//   1. Tạo child GameObject "Hitbox" dưới Player/Enemy
//   2. Add Collider2D (thường BoxCollider2D) → Is Trigger = ON
//   3. Add HitboxComponent vào child đó
//   4. Gán owner, damage, source từ parent script
//   5. Gọi SetActive(true/false) để bật/tắt hitbox

using UnityEngine;

public class HitboxComponent : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────
    [Header("Damage")]
    public int damage = 1;

    [Header("Source — ai sở hữu hitbox này")]
    public DamageSource source = DamageSource.Player;

    [Header("Tag của mục tiêu bị damage")]
    [Tooltip("Ví dụ: 'Enemy' nếu đây là hitbox của Player")]
    public string targetTag = "Enemy";

    [Header("Knockback")]
    [Tooltip("Lực đẩy lùi khi trúng đòn")]
    public float knockbackForce = 5f;
    // ── Private ──────────────────────────────────────────
    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();

        if (col == null)
        {
            Debug.LogError("[HitboxComponent] Không tìm thấy Collider2D trên " + gameObject.name);
            return;
        }

        col.isTrigger = true;
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        if (col == null) return;
        col.enabled = active;
        
    }

    // ── Xử lý va chạm ────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag)) return;

        IDamageable target = other.GetComponent<IDamageable>();
        if (target == null || !target.IsAlive) return;

        // Tính hướng knockback: từ hitbox đẩy ra xa
        Vector2 knockbackDir = ((Vector2)other.transform.position
                                - (Vector2)transform.position).normalized;

        // Gọi overload có knockback
        HealthComponent health = other.GetComponent<HealthComponent>();
        if (health != null)
            health.TakeDamage(damage, source, knockbackDir * knockbackForce);
        else
            target.TakeDamage(damage, source);
    }
}