// Assets/Scripts/Player/PlayerCombat.cs

using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(HealthComponent))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    public KeyCode attackKey = KeyCode.J;
    public float attackDuration = 0.2f;
    public float attackCooldown = 0.5f;
    public float comboWindow = 0.6f;
    public float maxAttackDuration = 1.0f;

    [Header("References")]
    public HitboxComponent hitbox;

    private PlayerController controller;
    private HealthComponent health;
    private Animator anim;
    private BoneThrower boneThrower;   // ← thêm

    private float attackTimer = 0f;
    private float hitboxTimer = 0f;
    private float comboTimer = 0f;
    private float maxAttackTimer = 0f;
    private bool isAttacking = false;
    private bool comboQueued = false;
    private bool didAttack2 = false;

    private bool inputBuffered = false;
    private float bufferWindow = 0.15f;
    private float bufferTimer = 0f;

    private static readonly int HashAttack1 = Animator.StringToHash("Attack1");
    private static readonly int HashAttack2 = Animator.StringToHash("Attack2");
    private static readonly int HashIsAttacking = Animator.StringToHash("IsAttacking");

    private bool attackRequested = false;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        health = GetComponent<HealthComponent>();
        anim = GetComponent<Animator>();
        boneThrower = GetComponent<BoneThrower>();   // ← thêm
        health.onDeath.AddListener(OnPlayerDeath);
        hitbox?.SetActive(false);
    }

    void Update()
    {
        if (!health.IsAlive) return;
        HandleAttackInput();
        TickHitbox();
        TickCombo();
        TickMaxAttack();
        TickBuffer();
    }

    // ── Input ─────────────────────────────────────────────
    void HandleAttackInput()
    {
        attackTimer -= Time.deltaTime;

        bool attackPressed = Input.GetKeyDown(attackKey) || attackRequested;
        attackRequested = false;

        if (!attackPressed) return;

        // Không thể tấn công khi đang giữ xương để ném  ← thêm
        if (boneThrower != null && boneThrower.IsAiming) return;

        if (!isAttacking && attackTimer <= 0f)
        {
            StartAttack1();
        }
        else if (isAttacking && !didAttack2)
        {
            comboQueued = true;
        }
        else if (!isAttacking && attackTimer > 0f)
        {
            inputBuffered = true;
            bufferTimer = bufferWindow;
        }
    }

    // ── Buffer ────────────────────────────────────────────
    void TickBuffer()
    {
        if (!inputBuffered) return;

        bufferTimer -= Time.deltaTime;

        if (bufferTimer <= 0f)
        {
            inputBuffered = false;
            return;
        }

        if (!isAttacking && attackTimer <= 0f)
        {
            inputBuffered = false;
            StartAttack1();
        }
    }

    // ── Attack 1 ──────────────────────────────────────────
    void StartAttack1()
    {
        isAttacking = true;
        didAttack2 = false;
        comboQueued = false;
        hitboxTimer = attackDuration;
        comboTimer = comboWindow;
        maxAttackTimer = maxAttackDuration;

        controller.SetInputLocked(true);

        PositionHitbox(controller.LastFacing);
        hitbox?.SetActive(true);
        anim?.SetBool(HashIsAttacking, true);
        anim?.SetTrigger(HashAttack1);
    }

    // ── Attack 2 ──────────────────────────────────────────
    void StartAttack2()
    {
        didAttack2 = true;
        comboQueued = false;
        comboTimer = 0f;
        hitboxTimer = attackDuration;
        attackTimer = attackCooldown;
        maxAttackTimer = maxAttackDuration;

        PositionHitbox(controller.LastFacing);
        hitbox?.SetActive(true);
        anim?.SetTrigger(HashAttack2);
    }

    // ── Tick hitbox ───────────────────────────────────────
    void TickHitbox()
    {
        if (!isAttacking) return;
        if (hitboxTimer <= 0f) return;

        hitboxTimer -= Time.deltaTime;
        if (hitboxTimer <= 0f)
            hitbox?.SetActive(false);
    }

    // ── Tick combo window ─────────────────────────────────
    void TickCombo()
    {
        if (!isAttacking) return;
        if (comboTimer <= 0f) return;

        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0f)
        {
            if (comboQueued) StartAttack2();
            else EndAttack();
        }
    }

    // ── Fallback timer ────────────────────────────────────
    void TickMaxAttack()
    {
        if (!isAttacking) return;

        maxAttackTimer -= Time.deltaTime;
        if (maxAttackTimer <= 0f)
            EndAttack();
    }

    // ── End attack ────────────────────────────────────────
    void EndAttack()
    {
        isAttacking = false;
        hitbox?.SetActive(false);
        if (!didAttack2) attackTimer = 0f;

        controller.SetInputLocked(false);
        anim?.SetBool(HashIsAttacking, false);
    }

    // Called from Animation Event on last frame of Attack2
    public void OnAttack2Finished()
    {
        EndAttack();
    }

    // ── Position hitbox ───────────────────────────────────
    void PositionHitbox(Vector2 facing)
    {
        if (hitbox == null) return;

        if (facing == Vector2.up)
            hitbox.transform.localPosition = new Vector2(0f, 1.5f);
        else if (facing == Vector2.down)
            hitbox.transform.localPosition = new Vector2(0f, -0.213f);
        else if (facing == Vector2.left)
            hitbox.transform.localPosition = new Vector2(-1f, 0.646f);
        else if (facing == Vector2.right)
            hitbox.transform.localPosition = new Vector2(1f, 0.868f);
    }

    public void RequestAttack()
    {
        attackRequested = true;
    }

    // ── Player death ──────────────────────────────────────
    void OnPlayerDeath(DamageSource source)
    {
        hitbox?.SetActive(false);
        controller.SetInputLocked(false);
        GameManager.Instance?.TriggerLose();
    }
}