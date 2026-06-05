// Assets/Scripts/Enemy/EnemyAnimator.cs
// Xử lý toàn bộ animation và hitbox toggle cho enemy.
// Tách khỏi EnemyController để mỗi loại enemy chỉ cần swap script này
// mà không cần đụng vào logic AI.
//
// Animator Parameters cần có:
//   int  "Direction"  — 0=South 1=North 2=East 3=West
//   bool "IsMoving"
//   bool "IsAttacking"
//   bool "IsDead"
//   trigger "Hit"     — khi nhận damage
//
// Animation Events trên clip Attack:
//   Frame bắt đầu hitbox active  → gọi OnAttackHit()
//   Frame cuối clip               → gọi OnAttackEnd()
//
// Animation Event trên clip Death (tuỳ chọn):
//   Frame cuối                    → gọi OnDeathEnd()

using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyAnimator : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Hitbox")]
    [Tooltip("HitboxComponent dùng khi Attack (gán trong Inspector)")]
    public HitboxComponent attackHitbox;

    // ── Internal refs ─────────────────────────────────────
    private Animator animator;
    private SpriteRenderer sr;
    private EnemyController controller;

    // ── Animator parameter hashes (tối ưu hơn dùng string) ──
    private static readonly int HashDirection = Animator.StringToHash("Direction");
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashIsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int HashIsDead = Animator.StringToHash("IsDead");
    private static readonly int HashHit = Animator.StringToHash("Hit");

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        controller = GetComponent<EnemyController>();

        // Đảm bảo hitbox tắt khi spawn
        EnableHitbox(false);

        // Lắng nghe event nhận damage để play Hit animation
        HealthComponent health = GetComponent<HealthComponent>();
        if (health != null)
            health.onDamaged.AddListener(OnDamaged);
    }

    // ── Gọi từ EnemyController ────────────────────────────
    public void OnStateChanged(EnemyController.EnemyState state)
    {
        // Guard: Animator chưa sẵn sàng
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
            return;

        switch (state)
        {
            case EnemyController.EnemyState.Idle:
                animator.SetBool(HashIsMoving, false);
                animator.SetBool(HashIsAttacking, false);
                animator.SetBool(HashIsDead, false);
                break;

            case EnemyController.EnemyState.Chase:
                animator.SetBool(HashIsMoving, true);
                animator.SetBool(HashIsAttacking, false);
                animator.SetBool(HashIsDead, false);
                break;

            case EnemyController.EnemyState.Attack:
                animator.SetBool(HashIsMoving, false);
                animator.SetBool(HashIsAttacking, true);
                EnableHitbox(false); // hitbox chỉ bật qua Animation Event
                break;

            case EnemyController.EnemyState.Dead:
                animator.SetBool(HashIsMoving, false);
                animator.SetBool(HashIsAttacking, false);
                animator.SetBool(HashIsDead, true);
                EnableHitbox(false);
                break;
        }
    }

    // Cập nhật hướng nhìn — gọi mỗi frame khi Chase
    public void UpdateDirection(int direction)
    {
        animator.SetFloat(HashDirection, direction);

        // Không dùng flip — có đủ animation trái/phải riêng
        sr.flipX = false;
    }

    // ── Hitbox toggle ─────────────────────────────────────
    public void EnableHitbox(bool active)
    {
        if (attackHitbox != null)
            attackHitbox.SetActive(active);
    }

    // ── Animation Events (đặt trên clip trong Unity) ──────
    // Clip Attack → frame đầu hitbox cần bật
    public void OnAttackHit()
    {
        EnableHitbox(true);
        controller.OnAttackHit();
    }

    // Clip Attack → frame cuối
    public void OnAttackEnd()
    {
        EnableHitbox(false);
        controller.OnAttackEnd();
    }

    // Clip Death → frame cuối (tuỳ chọn, thay cho Invoke fallback)a
    public void OnDeathEnd()
    {
        controller.ReturnToPool();
    }

    // ── Nhận damage → play Hit animation ─────────────────
    void OnDamaged(int damage, DamageSource source)
    {
        if (controller.State == EnemyController.EnemyState.Dead) return;
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) return;
        animator.SetTrigger(HashHit);
    }
}