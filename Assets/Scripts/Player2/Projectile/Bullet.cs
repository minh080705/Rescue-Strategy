// Assets/Scripts/Player2/Projectile/Bullet.cs
// Viên đạn bay theo hướng, va chạm enemy gây damage.
// Dùng object pool — không tự Destroy, gọi ReturnToPool.
// Va chạm với targetTag → gây damage → biến mất.
// Va chạm với object khác → không biến mất, tiếp tục bay.

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Stats")]
    public int damage = 1;
    public float speed = 12f;
    public float lifetime = 2f;

    [Header("Tags")]
    public string targetTag = "Enemy";

    // ── Internal ──────────────────────────────────────────
    private Rigidbody2D rb;
    private float lifeTimer;
    private BulletPool pool;
    private DamageSource source = DamageSource.Player;
    private bool hasHit = false; // tránh gây damage 2 lần cùng frame

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public void Launch(Vector2 direction, BulletPool ownerPool, DamageSource dmgSource = DamageSource.Player)
    {
        pool = ownerPool;
        source = dmgSource;
        lifeTimer = lifetime;
        hasHit = false;

        Vector2 dir = direction.normalized;
        rb.velocity = dir * speed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
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
        if (hasHit) return;

        if (other.CompareTag(targetTag))
        {
            // Trúng enemy → gây damage → biến mất
            HealthComponent health = other.GetComponent<HealthComponent>();
            if (health != null && health.IsAlive)
            {
                Vector2 knockDir = rb.velocity.normalized * 3f;
                health.TakeDamage(damage, source, knockDir);
            }

            hasHit = true;
            ReturnToPool();
            return;
        }

        // Va chạm với object khác (tường, props...) → tiếp tục bay, không làm gì
    }

    void ReturnToPool()
    {
        hasHit = false;
        rb.velocity = Vector2.zero;
        pool?.Return(gameObject);
    }
}