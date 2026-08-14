// Assets/Scripts/Enemy/BaseEnemy.cs
// Class nền cho tất cả loại enemy.
// Chứa logic dùng chung: movement, knockback, target selection,
// facing direction, death, pool, group.
// WarriorController và BatController kế thừa class này.

using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Movement")]
    public float moveSpeed = 2.5f;

    [Header("Detection")]
    public float detectionRange = 5f;

    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackCooldown = 2.0f;
    public float idleAfterAttackDuration = 1.0f;

    [Header("Knockback")]
    public float knockbackDuration = 0.3f;

    // ── Group ─────────────────────────────────────────────
    [HideInInspector] public EnemyGroup group;

    // ── Internal refs ─────────────────────────────────────
    protected Rigidbody2D rb;
    protected HealthComponent health;
    protected Transform playerTransform;
    protected Transform hostageTransform;

    // ── State ─────────────────────────────────────────────
    public Vector2 SpawnPosition { get; protected set; }
    public bool IsAlive => health != null && health.IsAlive;
    public bool IsChasing { get; protected set; } = false;

    // ── Direction ─────────────────────────────────────────
    public int FacingDirection { get; protected set; } = 0;
    protected int lastFacingDirection = 0;

    // ── Knockback ─────────────────────────────────────────
    protected bool isKnockedBack = false;
    protected float knockbackTimer = 0f;

    // ── Timers ────────────────────────────────────────────
    protected float attackCooldownTimer = 0f;
    protected float idleAfterAttackTimer = 0f;

    // ── Unity lifecycle ───────────────────────────────────
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        health = GetComponent<HealthComponent>();
        health.onDeath.AddListener(OnEnemyDeath);
        health.onKnockback.AddListener(OnKnockback);
    }

    protected virtual void OnEnable()
    {
        health?.ResetHP();
        var col = GetComponent<CapsuleCollider2D>();
        if (col != null) col.enabled = true;

        isKnockedBack = false;
        knockbackTimer = 0f;
        attackCooldownTimer = 0f;
        idleAfterAttackTimer = 0f;

        SpawnPosition = transform.position;
        group?.Register(this);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        GameObject h = GameObject.FindGameObjectWithTag("Hostage");
        if (p != null) playerTransform = p.transform;
        if (h != null) hostageTransform = h.transform;

    }

    protected virtual void OnDisable()
    {
        group?.Unregister(this);
    }

    // ── Knockback ─────────────────────────────────────────
    protected void OnKnockback(Vector2 force)
    {
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
        rb.velocity = force;
    }

    protected bool HandleKnockback()
    {
        if (!isKnockedBack) return false;

        knockbackTimer -= Time.fixedDeltaTime;
        if (knockbackTimer <= 0f)
        {
            isKnockedBack = false;
            rb.velocity = Vector2.zero;
        }
        return true;
    }

    // ── Target selection ──────────────────────────────────
    public Transform GetCurrentTarget() => GetClosestTarget();

    protected Transform GetClosestTarget()
    {
        HostageAI hostage = hostageTransform?.GetComponent<HostageAI>();
        bool hostageAlive = hostage != null && (hostage.HostageState == HostageState.Following || hostage.HostageState == HostageState.Attack);

        // Lọc theo layer
        bool playerSameLayer = playerTransform != null && IsSameLayer(playerTransform.gameObject);
        bool hostageSameLayer = hostageAlive && hostageTransform != null && IsSameLayer(hostageTransform.gameObject);

        if (!playerSameLayer && !hostageSameLayer) return null;
        if (!playerSameLayer) return hostageSameLayer ? hostageTransform : null;
        if (!hostageSameLayer) return playerSameLayer ? playerTransform : null;

        // Cả 2 cùng tầng → chọn cái gần hơn
        float dp = Vector2.Distance(transform.position, playerTransform.position);
        float dh = Vector2.Distance(transform.position, hostageTransform.position);
        return dp <= dh ? playerTransform : hostageTransform;
    }

    /// <summary>Kiểm tra target có cùng Physics Layer với enemy không.</summary>
    protected bool IsSameLayer(GameObject target)
    {
        return target.layer == gameObject.layer;
    }

    // ── Facing ────────────────────────────────────────────
    protected void UpdateFacingDirection(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            FacingDirection = dir.x >= 0 ? 2 : 3; // East / West
        else
            FacingDirection = dir.y >= 0 ? 1 : 0; // North / South

        lastFacingDirection = FacingDirection;
        OnFacingChanged(lastFacingDirection);
    }

    protected void ApplyLastFacing() => OnFacingChanged(lastFacingDirection);

    /// <summary>Override để cập nhật Animator direction.</summary>
    protected abstract void OnFacingChanged(int direction);

    // ── Death & Pool ──────────────────────────────────────
    protected virtual void OnEnemyDeath(DamageSource source)
    {
        if (!IsAlive && rb.velocity == Vector2.zero) return;
        rb.velocity = Vector2.zero;
        OnDeath(source);
        Invoke(nameof(ReturnToPool), 1.5f);
        var col = GetComponent<CapsuleCollider2D>();
        if (col != null) col.enabled = false;
        Debug.Log($"[{name}] Bị {source} tiêu diệt.");
    }

    /// <summary>Override để xử lý death state trong subclass.</summary>
    protected abstract void OnDeath(DamageSource source);

    protected void OnBecameInvisible()
    {
        if (IsAlive) ReturnToPool();
    }

    public virtual void ReturnToPool()
    {
        CancelInvoke(nameof(ReturnToPool));
        rb.velocity = Vector2.zero;

        var returnHelper = GetComponent<SpawnPointReturnHelper>();
        if (returnHelper != null)
            returnHelper.ReturnToPoolNow();   // trả về đúng pool sau 1.5s
        else
            gameObject.SetActive(false);      // fallback nếu không có helper
    }

    // ── Group API ─────────────────────────────────────────
    public abstract void ForceChase();
    public abstract void ForceReturn();

    // ── Effective detection range (tính alert) ────────────
    protected float GetEffectiveDetectionRange()
    {
        return (group != null && group.IsAlerted)
            ? group.alertDetectionRange
            : detectionRange;
    }
}