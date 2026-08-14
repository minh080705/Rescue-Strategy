// Assets/Scripts/Player2/Player2Combat.cs
// Điều phối attack và reload — không biết weapon cụ thể là gì,
// chỉ gọi qua IWeapon interface.

using UnityEngine;

[RequireComponent(typeof(Player2Controller))]
[RequireComponent(typeof(WeaponManager))]
[RequireComponent(typeof(HealthComponent))]
public class Player2Combat : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Keys")]
    public KeyCode attackKey = KeyCode.J;
    public KeyCode reloadKey = KeyCode.R;

    // ── Internal ──────────────────────────────────────────
    private Player2Controller controller;
    private WeaponManager weaponManager;
    private HealthComponent health;
    private Animator anim;
    private Player2Animator p2anim;

    private bool isAttackHeld = false;

    private static readonly int HashIsDead = Animator.StringToHash("IsDead");

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        controller = GetComponent<Player2Controller>();
        weaponManager = GetComponent<WeaponManager>();
        health = GetComponent<HealthComponent>();
        anim = GetComponent<Animator>();
        p2anim = GetComponent<Player2Animator>();

        health.onDeath.AddListener(OnPlayerDeath);
    }

    void Update()
    {
        if (!health.IsAlive) return;

        HandleAttackInput();
        HandleReloadInput();
        UpdateAnimatorState();
    }

    void UpdateAnimatorState()
    {
        if (p2anim == null) return;
        p2anim.FacingIndex = controller.FacingIndex;
        p2anim.IsMoving = controller.IsMoving;
        p2anim.IsDashing = controller.IsDashing;
        p2anim.IsFiring = isAttackHeld && weaponManager.CurrentType == WeaponType.Gun;
    }

    // ── Attack input ──────────────────────────────────────
    void HandleAttackInput()
    {
        IWeapon weapon = weaponManager.CurrentWeapon;
        if (weapon == null || !weapon.CanAttack)
        {
            // Nếu đang giữ nhưng weapon không thể attack thì release
            if (isAttackHeld)
            {
                weapon?.OnAttackReleased(controller.LastFacing);
                isAttackHeld = false;
            }
            return;
        }

        // Bắt đầu giữ phím
        if (Input.GetKeyDown(attackKey) && !isAttackHeld)
        {
            isAttackHeld = true;
            weapon.OnAttackHeld(controller.LastFacing);
        }

        // Đang giữ phím
        if (Input.GetKey(attackKey) && isAttackHeld)
        {
            weapon.OnAttackHeld(controller.LastFacing);
        }

        // Thả phím
        if (Input.GetKeyUp(attackKey) && isAttackHeld)
        {
            isAttackHeld = false;
            weapon.OnAttackReleased(controller.LastFacing);
        }
    }

    // ── Reload input ──────────────────────────────────────
    void HandleReloadInput()
    {
        if (Input.GetKeyDown(reloadKey))
            weaponManager.CurrentWeapon?.OnReload();
    }

    /// <summary>Gọi từ MobileInputHandler — lấy weapon hiện tại.</summary>
    public IWeapon GetCurrentWeapon() => weaponManager?.CurrentWeapon;

    // ── Death ─────────────────────────────────────────────
    void OnPlayerDeath(DamageSource source)
    {
        isAttackHeld = false;
        weaponManager.CurrentWeapon?.OnAttackReleased(Vector2.zero);
        weaponManager.CurrentWeapon?.OnUnequip();

        anim?.SetBool(HashIsDead, true);
        controller.SetInputLocked(true);    
        Debug.Log($"[Player2] Chết vì {source}");
        GameManager.Instance?.TriggerLose();
    }
}