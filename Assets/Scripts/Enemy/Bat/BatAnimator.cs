// Assets/Scripts/Enemy/Bat/BatAnimator.cs
// Xử lý animation cho Bat.
// Giống EnemyAnimator nhưng có thêm state PreAttack.
//
// Animator Parameters:
//   float "Direction"    — 0=South 1=North 2=East 3=West
//   bool  "IsMoving"
//   bool  "IsPreAttack"  — đang chuẩn bị tấn công (warning animation)
//   bool  "IsAttacking"
//   bool  "IsDead"
//   trigger "Hit"

using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class BatAnimator : MonoBehaviour
{
    // ── Animator hashes ───────────────────────────────────
    private static readonly int HashDirection = Animator.StringToHash("Direction");
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashIsPreAttack = Animator.StringToHash("IsPreAttack");
    private static readonly int HashIsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int HashIsDead = Animator.StringToHash("IsDead");
    private static readonly int HashHit = Animator.StringToHash("Hit");

    // ── Internal ──────────────────────────────────────────
    private Animator animator;
    private SpriteRenderer sr;
    private BatController controller;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        controller = GetComponent<BatController>();

        HealthComponent health = GetComponent<HealthComponent>();
        if (health != null)
            health.onDamaged.AddListener(OnDamaged);
    }

    // ── Gọi từ BatController ──────────────────────────────
    public void OnStateChanged(BatController.BatState state)
    {
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
            return;

        switch (state)
        {
            case BatController.BatState.Idle:
                animator.SetBool(HashIsMoving, false);
                animator.SetBool(HashIsPreAttack, false);
                animator.SetBool(HashIsAttacking, false);
                animator.SetBool(HashIsDead, false);
                break;

            case BatController.BatState.Chase:
            case BatController.BatState.Returning:
                animator.SetBool(HashIsMoving, true);
                animator.SetBool(HashIsPreAttack, false);
                animator.SetBool(HashIsAttacking, false);
                animator.SetBool(HashIsDead, false);
                break;

            case BatController.BatState.PreAttack:
                // Animation cảnh báo — dơi mở miệng / phồng má chẳng hạn
                animator.SetBool(HashIsMoving, false);
                animator.SetBool(HashIsPreAttack, true);
                animator.SetBool(HashIsAttacking, false);
                break;

            case BatController.BatState.Attack:
                animator.SetBool(HashIsPreAttack, false);
                animator.SetBool(HashIsAttacking, true);
                break;

            case BatController.BatState.Dead:
                animator.SetBool(HashIsMoving, false);
                animator.SetBool(HashIsPreAttack, false);
                animator.SetBool(HashIsAttacking, false);
                animator.SetBool(HashIsDead, true);
                break;
        }
    }

    public void UpdateDirection(int direction)
    {
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
            return;

        animator.SetFloat(HashDirection, direction);
        sr.flipX = false; // bat có đủ animation 4 hướng
    }

    // ── Animation Events ──────────────────────────────────
    public void OnAttackSpawn() => controller.OnAttackSpawn();
    public void OnAttackEnd() => controller.OnAttackEnd();

    public void OnDeathEnd() => controller.ReturnToPool();

    // ── Nhận damage ───────────────────────────────────────
    void OnDamaged(int damage, DamageSource source)
    {
        Debug.Log($"[BatAnimator] OnDamaged fired | State={controller.State} | AnimatorActive={animator.isActiveAndEnabled}");

        if (controller.State == BatController.BatState.Dead) return;
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) return;
        animator.SetTrigger(HashHit);
    }
}