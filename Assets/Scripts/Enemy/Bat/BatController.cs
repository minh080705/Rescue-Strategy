// Assets/Scripts/Enemy/Bat/BatController.cs
// Kế thừa BaseEnemy — chỉ chứa logic riêng của Bat.

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(BatAnimator))]
public class BatController : BaseEnemy
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Bat Attack")]
    public float attackDelay = 2.0f;
    public GameObject poisonAttackPrefab;
    public GameObject warningIndicatorPrefab;

    // ── Internal ──────────────────────────────────────────
    private BatAnimator anim;
    private BatPoisonAttack pooledAttack;
    private BatWarningIndicator pooledWarning;

    private float attackDelayTimer = 0f;
    private Vector2 lockedTargetPosition;

    // ── State ─────────────────────────────────────────────
    public enum BatState { Idle, Chase, PreAttack, Attack, Dead, Returning }
    public BatState State { get; private set; } = BatState.Idle;

    // ── Unity lifecycle ───────────────────────────────────
    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<BatAnimator>();

        if (poisonAttackPrefab != null)
        {
            GameObject obj = Instantiate(poisonAttackPrefab);
            obj.SetActive(false);
            pooledAttack = obj.GetComponent<BatPoisonAttack>();
        }

        if (warningIndicatorPrefab != null)
        {
            GameObject obj = Instantiate(warningIndicatorPrefab);
            obj.SetActive(false);
            pooledWarning = obj.GetComponent<BatWarningIndicator>();//
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        attackDelayTimer = 0f;
        SetState(BatState.Idle);
    }

    void FixedUpdate()
    {
        if (State == BatState.Dead) { rb.velocity = Vector2.zero; return; }
        if (HandleKnockback()) return;

        attackCooldownTimer -= Time.fixedDeltaTime;

        switch (State)
        {
            case BatState.Idle: UpdateIdle(); break;
            case BatState.Chase: UpdateChase(); break;
            case BatState.PreAttack: UpdatePreAttack(); break;
            case BatState.Attack: UpdateAttack(); break;
            case BatState.Returning: UpdateReturning(); break;
        }
    }

    // ── State updates ─────────────────────────────────────
    void UpdateIdle()
    {
        rb.velocity = Vector2.zero;
        ApplyLastFacing();

        if (idleAfterAttackTimer > 0f) { idleAfterAttackTimer -= Time.fixedDeltaTime; return; }

        Transform target = GetClosestTarget();
        if (target == null) return;

        if (Vector2.Distance(transform.position, target.position) <= GetEffectiveDetectionRange())
            SetState(BatState.Chase);
    }

    void UpdateChase()
    {
        Transform target = GetClosestTarget();
        if (target == null) { SetState(BatState.Idle); return; }

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist > GetEffectiveDetectionRange()) { SetState(BatState.Idle); return; }

        Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        UpdateFacingDirection(dir);

        if (dist <= attackRange && attackCooldownTimer <= 0f)
        {
            lockedTargetPosition = target.position;
            SetState(BatState.PreAttack);
            return;
        }

        rb.velocity = dir * moveSpeed;
    }

    void UpdatePreAttack()
    {
        rb.velocity = Vector2.zero;
        UpdateFacingDirection((lockedTargetPosition - (Vector2)transform.position).normalized);

        attackDelayTimer -= Time.fixedDeltaTime;
        if (attackDelayTimer <= 0f)
            SetState(BatState.Attack);
    }

    void UpdateAttack()
    {
        rb.velocity = Vector2.zero;
        // Animation Events OnAttackSpawn() và OnAttackEnd() xử lý
    }

    void UpdateReturning()
    {
        float dist = Vector2.Distance(transform.position, SpawnPosition);
        if (dist <= 0.5f) { rb.velocity = Vector2.zero; SetState(BatState.Idle); return; }

        Vector2 dir = ((Vector2)SpawnPosition - (Vector2)transform.position).normalized;
        rb.velocity = dir * moveSpeed;
        UpdateFacingDirection(dir);
    }

    // ── Animation Events ──────────────────────────────────
    public void OnAttackSpawn()
    {
        pooledAttack?.Spawn(lockedTargetPosition);
    }

    public void OnAttackEnd()
    {
        attackCooldownTimer = attackCooldown;
        idleAfterAttackTimer = idleAfterAttackDuration;
        SetState(BatState.Idle);
    }

    // ── BaseEnemy abstract impl ───────────────────────────
    protected override void OnFacingChanged(int direction) => anim?.UpdateDirection(direction);

    protected override void OnDeath(DamageSource source) => SetState(BatState.Dead);

    public override void ForceChase()
    {
        if (State == BatState.Dead || State == BatState.Chase || State == BatState.Attack) return;
        SetState(BatState.Chase);
    }

    public override void ForceReturn()
    {
        if (State == BatState.Dead || State == BatState.Returning) return;
        SetState(BatState.Returning);
    }

    // ── State helper ──────────────────────────────────────
    void SetState(BatState newState)
    {
        if (State == newState) return;
        State = newState;
        IsChasing = (newState == BatState.Chase || newState == BatState.PreAttack || newState == BatState.Attack);
        anim?.OnStateChanged(newState);

        if (newState == BatState.PreAttack)
        {
            attackDelayTimer = attackDelay;

            // Hiện cảnh báo tại vị trí đã lock
            pooledWarning?.Show(lockedTargetPosition, attackDelay);
        }

        Debug.Log($"[Bat:{name}] State → {newState}");
    }
}