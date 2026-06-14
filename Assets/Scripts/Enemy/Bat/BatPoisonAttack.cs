// Assets/Scripts/Enemy/Bat/BatPoisonAttack.cs
// Đòn tấn công của Bat — object riêng với animation và collider.
// Xuất hiện tại vị trí đã lock trước, tự disable sau khi animation xong.

using UnityEngine;

public class BatPoisonAttack : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Stats")]
    public int damage = 1;
    public float lifetime = 1.0f;   // tự tắt sau n giây
    public string targetTag = "Player";
    public DamageSource source = DamageSource.Enemy;

    // ── Internal ──────────────────────────────────────────
    private float lifeTimer;
    private bool hasHit = false;

    // ── Public API ────────────────────────────────────────
    /// <summary>Gọi từ BatController để đặt vị trí và kích hoạt.</summary>
    public void Spawn(Vector2 position)
    {
        transform.position = position;
        hasHit = false;
        lifeTimer = lifetime;
        gameObject.SetActive(true);
    }

    // ── Unity lifecycle ───────────────────────────────────
    void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
            gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (!other.CompareTag(targetTag)) return;

        HealthComponent health = other.GetComponentInParent<HealthComponent>();
        if (health != null && health.IsAlive)
        {
            health.TakeDamage(damage, source);
            hasHit = true;
        }

        gameObject.SetActive(false);
    }
}