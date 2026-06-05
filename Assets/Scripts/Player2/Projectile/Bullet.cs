// Assets/Scripts/Player2/Projectile/Bullet.cs
// Viên đạn bay theo hướng, va chạm enemy gây damage.
// Dùng object pool — không tự Destroy, gọi ReturnToPool.

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Stats")]
    public int damage = 1;
    public float speed = 12f;
    public float lifetime = 2f;    // tự pool về sau n giây

    [Header("Tags")]
    public string targetTag = "Enemy";

    // ── Internal ──────────────────────────────────────────
    private Rigidbody2D rb;
    private float lifeTimer;
    private BulletPool pool;
    private DamageSource source = DamageSource.Player;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    // Gọi mỗi lần lấy từ pool
    public void Launch(Vector2 direction, BulletPool ownerPool, DamageSource dmgSource = DamageSource.Player)
    {
        pool = ownerPool;
        source = dmgSource;
        lifeTimer = lifetime;

        rb.velocity = direction.normalized * speed;

        // Xoay sprite theo hướng bay
        // Giả sử sprite gốc nhìn sang phải (Right = 0 độ)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
            ReturnToPool();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag)) return;

        HealthComponent health = other.GetComponent<HealthComponent>();
        if (health != null && health.IsAlive)
        {
            // Knockback về phía đạn bay
            Vector2 knockDir = rb.velocity.normalized * 3f;
            health.TakeDamage(damage, source, knockDir);
        }

        ReturnToPool();
    }

    void ReturnToPool()
    {
        rb.velocity = Vector2.zero;
        pool?.Return(gameObject);
    }
}