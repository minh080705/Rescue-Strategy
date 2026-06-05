// Assets/Scripts/Player2/Player2Animator.cs
// Set cả Int lẫn Float cho từng parameter:
//   Int  → dùng trong Transition conditions
//   Float → dùng trong Blend Tree 1D

using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Player2Controller))]
[RequireComponent(typeof(WeaponManager))]
public class Player2Animator : MonoBehaviour
{
    // ── Animator parameter hashes ─────────────────────────
    // Int — dùng cho Transition conditions
    private static readonly int HashFacingIndex = Animator.StringToHash("FacingIndex");
    private static readonly int HashWeaponType = Animator.StringToHash("WeaponType");

    // Float — dùng cho Blend Tree 1D
    private static readonly int HashFacingIndexF = Animator.StringToHash("FacingIndexF");
    private static readonly int HashWeaponTypeF = Animator.StringToHash("WeaponTypeF");

    // Bool
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashIsDashing = Animator.StringToHash("IsDashing");
    private static readonly int HashIsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int HashIsReloading = Animator.StringToHash("IsReloading");
    private static readonly int HashIsDead = Animator.StringToHash("IsDead");

    // ── Internal refs ─────────────────────────────────────
    private Animator anim;
    private Player2Controller controller;
    private WeaponManager weaponManager;
    private GunWeapon gunWeapon;
    private HealthComponent health;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<Player2Controller>();
        weaponManager = GetComponent<WeaponManager>();
        gunWeapon = GetComponent<GunWeapon>();
        health = GetComponent<HealthComponent>();
    }

    void Update()
    {
        if (!health.IsAlive) return;
        UpdateParameters();
    }

    // ── Update tất cả parameters mỗi frame ───────────────
    void UpdateParameters()
    {
        int facingIndex = controller.FacingIndex;
        int weaponType = (int)weaponManager.CurrentType;

        // Int — cho Transition conditions
        anim.SetInteger(HashFacingIndex, facingIndex);
        anim.SetInteger(HashWeaponType, weaponType);

        // Float — cho Blend Tree 1D
        anim.SetFloat(HashFacingIndexF, facingIndex);
        anim.SetFloat(HashWeaponTypeF, weaponType);

        // Bool
        anim.SetBool(HashIsMoving, controller.IsMoving);
        anim.SetBool(HashIsDashing, controller.IsDashing);

        // Attack / Reload — đọc từ GunWeapon nếu đang cầm súng
        bool isAttacking = false;
        bool isReloading = false;

        if (weaponManager.CurrentType == WeaponType.Gun && gunWeapon != null)
        {
            isReloading = gunWeapon.IsReloading;
            isAttacking = !isReloading
                          && gunWeapon.CurrentAmmo > 0
                          && Input.GetKey(KeyCode.J);
        }
        else if (weaponManager.CurrentType == WeaponType.Spear)
        {
            // SpearWeapon tự set IsAttacking — không override ở đây
            return;
        }

        anim.SetBool(HashIsAttacking, isAttacking);
        anim.SetBool(HashIsReloading, isReloading);
    }
}