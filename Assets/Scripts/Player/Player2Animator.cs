// Assets/Scripts/Player2/Player2Animator.cs
// Set cả Int lẫn Float cho từng parameter.
// IsFiring được set từ bên ngoài (keyboard, button, joystick, AI)
// thay vì đọc Input.GetKey trực tiếp.

using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(WeaponManager))]
[RequireComponent(typeof(HealthComponent))]
public class Player2Animator : MonoBehaviour
{
    // ── Animator parameter hashes ─────────────────────────
    private static readonly int HashFacingIndex = Animator.StringToHash("FacingIndex");
    private static readonly int HashWeaponType = Animator.StringToHash("WeaponType");
    private static readonly int HashFacingIndexF = Animator.StringToHash("FacingIndexF");
    private static readonly int HashWeaponTypeF = Animator.StringToHash("WeaponTypeF");
    private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");
    private static readonly int HashIsDashing = Animator.StringToHash("IsDashing");
    private static readonly int HashIsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int HashIsReloading = Animator.StringToHash("IsReloading");
    private static readonly int HashIsDead = Animator.StringToHash("IsDead");

    // ── Public API — set từ bên ngoài ────────────────────
    /// <summary>
    /// Đang giữ phím/button bắn — set từ Player2Combat, UI button, joystick, hoặc AI.
    /// </summary>
    public bool IsFiring { get; set; } = false;

    /// <summary>
    /// Đang di chuyển — set từ Player2Controller hoặc AI.
    /// </summary>
    public bool IsMoving { get; set; } = false;

    /// <summary>
    /// Đang dash — set từ Player2Controller hoặc AI.
    /// </summary>
    public bool IsDashing { get; set; } = false;

    /// <summary>
    /// FacingIndex hiện tại (0-7) — set từ Player2Controller hoặc AI.
    /// </summary>
    public int FacingIndex { get; set; } = 0;

    // ── Internal refs ─────────────────────────────────────
    private Animator anim;
    private WeaponManager weaponManager;
    private GunWeapon gunWeapon;
    private HealthComponent health;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        anim = GetComponent<Animator>();
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
        int weaponType = (int)weaponManager.CurrentType;

        // Int — cho Transition conditions
        anim.SetInteger(HashFacingIndex, FacingIndex);
        anim.SetInteger(HashWeaponType, weaponType);

        // Float — cho Blend Tree 1D
        anim.SetFloat(HashFacingIndexF, FacingIndex);
        anim.SetFloat(HashWeaponTypeF, weaponType);

        // Bool chung
        anim.SetBool(HashIsMoving, IsMoving);
        anim.SetBool(HashIsDashing, IsDashing);

        // Attack / Reload theo weapon
        bool isAttacking = false;
        bool isReloading = false;

        switch (weaponManager.CurrentType)
        {
            case WeaponType.Gun:
                if (gunWeapon != null)
                {
                    isReloading = gunWeapon.IsReloading;
                    // Dùng IsFireHeld thay vì Input.GetKey
                    isAttacking = !isReloading
                                  && gunWeapon.CurrentAmmo > 0
                                  && (gunWeapon.IsFireHeld || Input.GetKey(KeyCode.J));
                }
                anim.SetBool(HashIsAttacking, isAttacking);
                anim.SetBool(HashIsReloading, isReloading);
                break;

            case WeaponType.Spear:
                // SpearWeapon tự set IsAttacking — không override
                anim.SetBool(HashIsReloading, false);
                break;

            case WeaponType.None:
                anim.SetBool(HashIsAttacking, false);
                anim.SetBool(HashIsReloading, false);
                break;
        }
    }

    // ── Death ─────────────────────────────────────────────
    public void SetDead(bool isDead)
    {
        anim.SetBool(HashIsDead, isDead);
    }
}