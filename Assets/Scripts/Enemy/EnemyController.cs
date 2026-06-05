// Assets/Scripts/Enemy/EnemyController.cs
// Enemy chung dùng cho nhiều loại kẻ địch.
// State machine: Idle → Chase → Attack → Idle → Chase...
// Hitbox chỉ bật trong state Attack (qua Animation Event).
// Animation 4 hướng N/S/E/W, sprite flip theo trục X khi đi Tây.

using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(EnemyAnimator))]
public class EnemyController : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Movement")]
    public float moveSpeed = 2.5f;

    [Header("Detection")]
    [Tooltip("Tầm phát hiện target — ngoài vùng này enemy đứng Idle")]
    public float detectionRange = 5f;

    [Header("Attack")]
    [Tooltip("Tầm tấn công")]
    public float attackRange = 1.2f;
    [Tooltip("Hồi chiêu giữa 2 lần attack (giây)")]
    public float attackCooldown = 2.0f;
    [Tooltip("Thời gian Idle sau attack trước khi chase lại (giây)")]
    public float idleAfterAttackDuration = 1.0f;

    [Header("Knockback")]
    public float knockbackDuration = 0.3f;

    // ── Internal refs ─────────────────────────────────────
    [HideInInspector] public EnemySpawner spawner;

    private Rigidbody2D rb;
    private HealthComponent health;
    private EnemyAnimator anim;

    private Transform playerTransform;
    private Transform hostageTransform;

    // ── State machine ─────────────────────────────────────
    public enum EnemyState { Idle, Chase, Attack, Dead }
    public EnemyState State { get; private set; } = EnemyState.Idle;

    private float attackCooldownTimer = 0f;
    private float idleAfterAttackTimer = 0f;

    // ── Knockback ─────────────────────────────────────────
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;

    // ── Attack timeout (fallback nếu Animation Event không fire) ──
    [Header("Attack")]
    [Tooltip("Timeout tối đa cho 1 lần attack — fallback nếu Animation Event lỗi")]
    public float attackTimeout = 2.0f;
    private float attackTimeoutTimer = 0f;

    // ── Direction (dùng chung với EnemyAnimator) ──────────
    // 0 = South, 1 = North, 2 = East, 3 = West
    // LastFacingDirection giữ hướng cuối cùng — không bao giờ reset về 0
    public int FacingDirection { get; private set; } = 0;
    private int lastFacingDirection = 0;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        health = GetComponent<HealthComponent>();
        anim = GetComponent<EnemyAnimator>();

        health.onDeath.AddListener(OnEnemyDeath);
        health.onKnockback.AddListener(OnKnockback);
    }

    void OnEnable()
    {
        health?.ResetHP();

        isKnockedBack = false;
        knockbackTimer = 0f;
        attackCooldownTimer = 0f;
        idleAfterAttackTimer = 0f;

        SetState(EnemyState.Idle);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        GameObject h = GameObject.FindGameObjectWithTag("Hostage");
        if (p != null) playerTransform = p.transform;
        if (h != null) hostageTransform = h.transform;
    }

    void FixedUpdate()
    {
        if (State == EnemyState.Dead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // Knockback override mọi state
        if (isKnockedBack)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
                rb.velocity = Vector2.zero;
            }
            return;
        }

        attackCooldownTimer -= Time.fixedDeltaTime;

        switch (State)
        {
            case EnemyState.Idle: UpdateIdle(); break;
            case EnemyState.Chase: UpdateChase(); break;
            case EnemyState.Attack: UpdateAttack(); break;
        }
    }

    // ── State updates ─────────────────────────────────────
    void UpdateIdle()
    {
        rb.velocity = Vector2.zero;
        ApplyLastFacing(); // giữ hướng cuối khi đứng yên

        // Chờ hết idle timer sau attack
        if (idleAfterAttackTimer > 0f)
        {
            idleAfterAttackTimer -= Time.fixedDeltaTime;
            return;
        }

        // Chỉ chase khi target trong detectionRange
        Transform target = GetClosestTarget();
        if (target != null && Vector2.Distance(transform.position, target.position) <= detectionRange)
            SetState(EnemyState.Chase);
    }

    void UpdateChase()
    {
        Transform target = GetClosestTarget();

        if (target == null)
        {
            SetState(EnemyState.Idle);
            return;
        }

        float dist = Vector2.Distance(transform.position, target.position);

        // Target chạy ra ngoài detectionRange → về Idle
        if (dist > detectionRange)
        {
            SetState(EnemyState.Idle);
            return;
        }

        // Luôn update facing về phía target
        Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        UpdateFacing(dir);

        // Vào tầm đánh
        if (dist <= attackRange)
        {
            rb.velocity = Vector2.zero;
            if (attackCooldownTimer <= 0f)
                SetState(EnemyState.Attack);
            return;
        }

        // Di chuyển về phía target
        rb.velocity = dir * moveSpeed;
    }

    void UpdateAttack()
    {
        rb.velocity = Vector2.zero;

        // Quay mặt về target, fallback lastFacing nếu không có target
        Transform target = GetClosestTarget();
        if (target != null)
        {
            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            UpdateFacing(dir);
        }
        else
        {
            ApplyLastFacing();
        }

        // Fallback: nếu Animation Event không fire thì tự thoát sau timeout
        attackTimeoutTimer -= Time.fixedDeltaTime;
        if (attackTimeoutTimer <= 0f)
        {
            Debug.LogWarning($"[Enemy:{name}] Attack timeout! Animation Event OnAttackEnd chưa được gán?");
            OnAttackEnd();
        }
    }

    // ── Facing direction ──────────────────────────────────
    void UpdateFacing(Vector2 dir)
    {
        // Chọn hướng dominant
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            FacingDirection = dir.x >= 0 ? 2 : 3; // East / West
        else
            FacingDirection = dir.y >= 0 ? 1 : 0; // North / South

        lastFacingDirection = FacingDirection;
        anim.UpdateDirection(lastFacingDirection);
        Debug.Log($"[Enemy:{name}] Facing → {lastFacingDirection} (dir={dir})");
    }

    // Dùng khi không có dir mới — giữ nguyên hướng cũ
    void ApplyLastFacing()
    {
        anim.UpdateDirection(lastFacingDirection);
    }

    // ── Animation Events (gọi từ Animation clip) ──────────
    // Đặt event tên "OnAttackHit" vào frame hitbox cần bật trong clip Attack
    // Đặt event tên "OnAttackEnd" vào frame cuối clip Attack
    public void OnAttackHit()
    {
        anim.EnableHitbox(true);
    }

    public void OnAttackEnd()
    {
        anim.EnableHitbox(false);

        attackCooldownTimer = attackCooldown;
        idleAfterAttackTimer = idleAfterAttackDuration;

        SetState(EnemyState.Idle);
    }

    // ── State machine helper ──────────────────────────────
    void SetState(EnemyState newState)
    {
        if (State == newState) return; // không spam khi state không đổi
        State = newState;
        anim.OnStateChanged(newState);

        if (newState == EnemyState.Attack)
        {
            attackTimeoutTimer = attackTimeout;

            // Quay mặt về target, fallback lastFacing nếu không có target
            Transform target = GetClosestTarget();
            if (target != null)
            {
                Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
                UpdateFacing(dir);
            }
            else
            {
                ApplyLastFacing();
            }
        }

        Debug.Log($"[Enemy:{name}] State → {newState}");
    }

    // ── Knockback ─────────────────────────────────────────
    void OnKnockback(Vector2 force)
    {
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
        rb.velocity = force;
    }

    // ── Target selection ──────────────────────────────────
    Transform GetClosestTarget()
    {
        if (DistractionManager.Instance != null)
        {
            Transform distraction = DistractionManager.Instance.GetClosest(transform.position);
            if (distraction != null) return distraction;
        }

        HostageController hostage = hostageTransform?.GetComponent<HostageController>();
        bool hostageAlive = hostage != null && hostage.State != HostageState.Rescued;

        if (playerTransform == null)
            return hostageAlive ? hostageTransform : null;

        if (!hostageAlive)
            return playerTransform;

        float distPlayer = Vector2.Distance(transform.position, playerTransform.position);
        float distHostage = Vector2.Distance(transform.position, hostageTransform.position);

        return distPlayer <= distHostage ? playerTransform : hostageTransform;
    }

    // ── Death ─────────────────────────────────────────────
    void OnEnemyDeath(DamageSource source)
    {
        if (State == EnemyState.Dead) return; // tránh gọi 2 lần

        rb.velocity = Vector2.zero;
        SetState(EnemyState.Dead); // SetBool trên Animator còn sống

        Debug.Log($"[Enemy:{name}] Bị {source} tiêu diệt.");

        // Animation Event "OnDeathEnd" trên clip Death sẽ gọi ReturnToPool()
        // Fallback nếu không dùng event:
        Invoke(nameof(ReturnToPool), 1.5f);
    }

    void OnBecameInvisible()
    {
        // Không pool khi đang chết — để animation Death chạy hết
        if (State != EnemyState.Dead)
            ReturnToPool();
    }

    public void ReturnToPool()
    {
        CancelInvoke(nameof(ReturnToPool));
        anim.EnableHitbox(false);
        rb.velocity = Vector2.zero;
        spawner?.ReturnToPool(gameObject);
    }
}