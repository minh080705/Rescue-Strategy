// Assets/Scripts/Enemy/Warrior/WarriorAnimator.cs
// Đổi tên từ EnemyAnimator → WarriorAnimator.

using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class WarriorAnimator : MonoBehaviour
{
    [Header("Hitbox")]
    public HitboxComponent attackHitbox;

    private Animator animator;
    private SpriteRenderer sr;
    private WarriorController controller;

    private static readonly int HashDirection = Animator.StringToHash("Direction");
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashIsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int HashIsDead = Animator.StringToHash("IsDead");
    private static readonly int HashHit = Animator.StringToHash("Hit");

    void Awake()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        controller = GetComponent<WarriorController>();

        EnableHitbox(false);

        HealthComponent health = GetComponent<HealthComponent>();
        if (health != null)
            health.onDamaged.AddListener(OnDamaged);
    }

    public void OnStateChanged(WarriorController.WarriorState state)
    {
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) return;

        switch (state)
        {
            case WarriorController.WarriorState.Idle:
                animator.SetBool(HashIsMoving, false);
                animator.SetBool(HashIsAttacking, false);
                animator.SetBool(HashIsDead, false);
                break;
            case WarriorController.WarriorState.Chase:
            case WarriorController.WarriorState.Returning:
                animator.SetBool(HashIsMoving, true);
                animator.SetBool(HashIsAttacking, false);
                animator.SetBool(HashIsDead, false);
                break;
            case WarriorController.WarriorState.Attack:
                animator.SetBool(HashIsMoving, false);
                animator.SetBool(HashIsAttacking, true);
                EnableHitbox(false);
                break;
            case WarriorController.WarriorState.Dead:
                animator.SetBool(HashIsMoving, false);
                animator.SetBool(HashIsAttacking, false);
                animator.SetBool(HashIsDead, true);
                EnableHitbox(false);
                break;
        }
    }

    public void UpdateDirection(int direction)
    {
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) return;
        animator.SetFloat(HashDirection, direction);
        sr.flipX = false;
    }

    public void EnableHitbox(bool active)
    {
        if (attackHitbox != null) attackHitbox.SetActive(active);
    }

    public void OnAttackHit() { EnableHitbox(true); controller.OnAttackHit(); }
    public void OnAttackEnd() { EnableHitbox(false); controller.OnAttackEnd(); }
    public void OnDeathEnd() => controller.ReturnToPool();

    void OnDamaged(int damage, DamageSource source)
    {
        if (controller.State == WarriorController.WarriorState.Dead) return;
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) return;
        animator.SetTrigger(HashHit);
    }
}