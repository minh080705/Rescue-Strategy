// Assets/Scripts/Player2/Weapons/SpearWeapon.cs
// Tấn công bằng thương — hitbox 8 hướng, giữ phím để đâm liên tục.
// Khi đang attack → khóa hướng di chuyển (Player2Controller.SetInputLocked).
// Hostage không có Player2Controller → bỏ qua lock.

using UnityEngine;

public class SpearWeapon : MonoBehaviour, IWeapon
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Attack")]
    public float attackCooldown = 1.2f;
    public float hitboxDuration = 1f;

    [Header("References")]
    public HitboxComponent hitbox;

    // ── IWeapon ───────────────────────────────────────────
    public WeaponType Type => WeaponType.Spear;
    public bool CanAttack => !isOnCooldown;
    public bool IsAttacking => isAttacking;

    // ── Internal ──────────────────────────────────────────
    private bool isOnCooldown = false;
    private bool isAttackHeld = false;
    private bool isHitboxActive = false;
    private bool isAttacking = false;
    private float cooldownTimer = 0f;
    private float hitboxTimer = 0f;
    private float attackingTimer = 0f;  // fallback timeout
    private const float ATTACK_TIMEOUT = 3f; // lớn hơn clip dài nhất

    private Animator anim;
    private Player2Controller controller; // null nếu là Hostage

    private static readonly int HashIsAttacking = Animator.StringToHash("IsAttacking");

    // ── Hitbox angles & positions 8 hướng ────────────────
    private static readonly float[] HitboxAngles = new float[]
    {
        180f,   // 0 Down
          0f,   // 1 Up
         90f,   // 2 Left
        -90f,   // 3 Right
       -135f,   // 4 RightDown
        -45f,   // 5 RightUp
        135f,   // 6 LeftDown
         45f,   // 7 LeftUp
    };

    private static readonly Vector2[] HitboxPositions = new Vector2[]
    {
        new Vector2(-0.2f, 0.2f), // 0 Down
        new Vector2( 0.2f, 0f),   // 1 Up
        new Vector2( 0f,   0.5f), // 2 Left
        new Vector2( 0f,   0.5f), // 3 Right
        new Vector2( 0f,   0f),   // 4 RightDown
        new Vector2( 0f,   0f),   // 5 RightUp
        new Vector2( 0f,   0f),   // 6 LeftDown
        new Vector2( 0f,   0f),   // 7 LeftUp
    };

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        anim = GetComponentInParent<Animator>();
        controller = GetComponentInParent<Player2Controller>(); // null nếu Hostage
        hitbox?.SetActive(false);
    }

    void Update()
    {
        TickCooldown();
        TickHitbox();

        // Hitbox đang bật → cập nhật rotation theo hướng hiện tại
        if (isHitboxActive && controller != null)
            RotateHitbox(controller.FacingIndex);

        // Fallback: nếu OnSpearHitEnd không fire sau ATTACK_TIMEOUT → tự reset
        if (isAttacking)
        {
            attackingTimer -= Time.deltaTime;
            if (attackingTimer <= 0f)
            {
                Debug.LogWarning("[Spear] Attack timeout — OnSpearHitEnd chưa được gán?");
                OnSpearHitEnd();
            }
        }

        // Giữ phím → tự trigger lại khi hết cooldown và animation xong
        if (isAttackHeld && !isOnCooldown && !isAttacking && controller != null)
            TriggerAttack(controller.LastFacing);

        if (isHitboxActive)
        {
            int facingIndex = controller != null
                ? controller.FacingIndex
                : GetComponentInParent<Player2Animator>()?.FacingIndex ?? 0;
            RotateHitbox(facingIndex);
        }
    }

    // ── IWeapon impl ──────────────────────────────────────
    public void OnEquip()
    {
        hitbox?.SetActive(false);
        cooldownTimer = 0f;
        isOnCooldown = false;
        isAttacking = false;
        AudioManager.Instance.Play("SpearEquip");
    }

    public void OnUnequip()
    {
        isAttackHeld = false;
        isAttacking = false;
        isHitboxActive = false;
        hitbox?.SetActive(false);
        anim?.SetBool(HashIsAttacking, false);
        SetMovementLock(false); // mở khóa khi đổi weapon
    }

    public void OnAttackHeld(Vector2 facing)
    {
        isAttackHeld = true;
        if (!isOnCooldown && !isAttacking)
            TriggerAttack(facing);
    }

    public void OnAttackReleased(Vector2 facing)
    {
        isAttackHeld = false;
    }

    public void OnReload() { }

    // ── Attack logic ──────────────────────────────────────
    void TriggerAttack(Vector2 facing)
    {
        if (isAttacking) return;

        isAttacking = true;
        cooldownTimer = attackCooldown;
        isOnCooldown = true;

        attackingTimer = ATTACK_TIMEOUT; // bắt đầu đếm ngược fallback

        // Khóa hướng di chuyển khi attack (chỉ Player)
        SetMovementLock(true);

        anim?.SetBool(HashIsAttacking, true);
        AudioManager.Instance.Play("SpearAttack");
        Invoke(nameof(PlaySecondAttack), 0.3f);
        Debug.Log($"[Spear] Đâm hướng {Player2Controller.ToIndex(facing)} ({facing})");
    }
    void PlaySecondAttack()
    {
        AudioManager.Instance.Play("SpearAttack");
    }
    // ── Tick ──────────────────────────────────────────────
    void TickCooldown()
    {
        if (!isOnCooldown) return;
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            isOnCooldown = false;
            // KHÔNG reset isAttacking hay IsAttacking Bool ở đây
            // OnSpearHitEnd() mới là nơi reset
        }
    }

    void TickHitbox()
    {
        if (hitboxTimer <= 0f) return;
        hitboxTimer -= Time.deltaTime;
        if (hitboxTimer <= 0f)
        {
            isHitboxActive = false;
            hitbox?.SetActive(false);
        }
    }

    // ── Animation Events ──────────────────────────────────
    public void OnSpearHit()
    {
        isHitboxActive = true;
        hitboxTimer = hitboxDuration;

        // Player dùng controller, Hostage dùng p2anim
        int facingIndex = controller != null
            ? controller.FacingIndex
            : GetComponentInParent<Player2Animator>()?.FacingIndex ?? 0;

        RotateHitbox(facingIndex);
        hitbox?.SetActive(true);
    }

    public void OnSpearHitEnd()
    {
        isAttacking = false;
        isHitboxActive = false;
        hitbox?.SetActive(false);
        anim?.SetBool(HashIsAttacking, false);

        // Mở khóa di chuyển sau khi animation xong
        SetMovementLock(false);
    }

    // ── Movement lock ─────────────────────────────────────
    void SetMovementLock(bool locked)
    {
        // Chỉ lock khi là Player — Hostage không có Player2Controller
        controller?.SetInputLocked(locked);
    }

    // ── Helpers ───────────────────────────────────────────
    void RotateHitbox(int facingIndex)
    {
        if (hitbox == null) return;
        int i = Mathf.Clamp(facingIndex, 0, HitboxAngles.Length - 1);
        hitbox.transform.localRotation = Quaternion.Euler(0f, 0f, HitboxAngles[i]);
        hitbox.transform.localPosition = HitboxPositions[i];
    }
}