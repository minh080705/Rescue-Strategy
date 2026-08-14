// Assets/Scripts/Enemy/Warrior/WarriorController.cs
// Đổi tên từ EnemyController → WarriorController.
// Kế thừa BaseEnemy — chỉ chứa logic riêng của Warrior.

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(WarriorAnimator))]
public class WarriorController : BaseEnemy
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Attack Timeout")]
    public float attackTimeout = 2.0f;

    // ── Internal ──────────────────────────────────────────
    private WarriorAnimator anim;
    private EnemyPathfinder pathfinder;
    private float attackTimeoutTimer = 0f;

    // ── State ─────────────────────────────────────────────
    public enum WarriorState { Idle, Chase, Attack, Dead, Returning }
    public WarriorState State { get; private set; } = WarriorState.Idle;

    // ── Unity lifecycle ───────────────────────────────────
    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<WarriorAnimator>();

        // Sync HP bar canvas với sorting layer của Warrior
        SpriteRenderer warriorRenderer = GetComponentInChildren<SpriteRenderer>();
        Canvas hpCanvas = GetComponentInChildren<Canvas>();
        pathfinder = GetComponent<EnemyPathfinder>();

        if (warriorRenderer != null && hpCanvas != null)
        {
            hpCanvas.overrideSorting = true;
            hpCanvas.sortingLayerID = warriorRenderer.sortingLayerID;
            hpCanvas.sortingOrder = 3;//
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        attackTimeoutTimer = 0f;
        SetState(WarriorState.Idle);
    }

    void FixedUpdate()
    {
        if (State == WarriorState.Dead) { rb.velocity = Vector2.zero; return; }
        if (HandleKnockback()) return;

        attackCooldownTimer -= Time.fixedDeltaTime;

        switch (State)
        {
            case WarriorState.Idle: UpdateIdle(); break;
            case WarriorState.Chase: UpdateChase(); break;
            case WarriorState.Attack: UpdateAttack(); break;
            case WarriorState.Returning: UpdateReturning(); break;
        }
    }

    // ── State updates ─────────────────────────────────────
    void UpdateIdle()
    {
        rb.velocity = Vector2.zero;
        ApplyLastFacing();

        if (idleAfterAttackTimer > 0f)
        {
            idleAfterAttackTimer -= Time.fixedDeltaTime;
            return;
        }

        Transform target = GetClosestTarget();
        if (target == null) return;

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist <= GetEffectiveDetectionRange())
            SetState(WarriorState.Chase);
    }

    void UpdateChase()
    {
        Transform target = GetClosestTarget();
        if (target == null) { SetState(WarriorState.Idle); return; }

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist > GetEffectiveDetectionRange()) { SetState(WarriorState.Idle); return; }

        Vector2 dirToTarget = ((Vector2)target.position - (Vector2)transform.position).normalized;

        if (dist <= attackRange)
        {
            UpdateFacingDirection(dirToTarget);
            rb.velocity = Vector2.zero;
            if (attackCooldownTimer <= 0f) SetState(WarriorState.Attack);
            return;
        }

        // Dùng hướng đi theo A* từ EnemyPathfinder, thay vì đâm thẳng vào target
        Vector2 moveDir = pathfinder.CurrentMoveDirection;
        if (moveDir != Vector2.zero)
        {
            rb.velocity = moveDir * moveSpeed;
            UpdateFacingDirection(moveDir);   // facing theo hướng path, không theo hướng target thẳng
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    void UpdateAttack()
    {
        rb.velocity = Vector2.zero;

        Transform target = GetClosestTarget();
        if (target != null)
            UpdateFacingDirection(((Vector2)target.position - (Vector2)transform.position).normalized);
        else
            ApplyLastFacing();

        attackTimeoutTimer -= Time.fixedDeltaTime;
        if (attackTimeoutTimer <= 0f)
        {         
            OnAttackEnd();
        }
    }

    void UpdateReturning()
    {
        float dist = Vector2.Distance(transform.position, SpawnPosition);
        if (dist <= 0.5f) { rb.velocity = Vector2.zero; SetState(WarriorState.Idle); return; }

        Vector2 dir = ((Vector2)SpawnPosition - (Vector2)transform.position).normalized;
        rb.velocity = dir * moveSpeed;
        UpdateFacingDirection(dir);
    }

    // ── Animation Events ──────────────────────────────────
    public void OnAttackHit() => anim.EnableHitbox(true);

    public void OnAttackEnd()
    {
        anim.EnableHitbox(false);
        attackCooldownTimer = attackCooldown;
        idleAfterAttackTimer = idleAfterAttackDuration;
        SetState(WarriorState.Idle);
    }

    // ── BaseEnemy abstract impl ───────────────────────────
    protected override void OnFacingChanged(int direction) => anim?.UpdateDirection(direction);

    protected override void OnDeath(DamageSource source) => SetState(WarriorState.Dead);

    public override void ForceChase()
    {
        if (State == WarriorState.Dead || State == WarriorState.Chase || State == WarriorState.Attack) return;
        SetState(WarriorState.Chase);
    }

    public override void ForceReturn()
    {
        if (State == WarriorState.Dead || State == WarriorState.Returning) return;
        SetState(WarriorState.Returning);
    }

    // ── State helper ──────────────────────────────────────
    void SetState(WarriorState newState)
    {
        if (State == newState) return;
        State = newState;
        IsChasing = (newState == WarriorState.Chase || newState == WarriorState.Attack);
        anim?.OnStateChanged(newState);

        if (newState == WarriorState.Attack)
        {
            attackTimeoutTimer = attackTimeout;
            Transform target = GetClosestTarget();
            if (target != null)
                UpdateFacingDirection(((Vector2)target.position - (Vector2)transform.position).normalized);
            else
                ApplyLastFacing();
        }       
    }
}