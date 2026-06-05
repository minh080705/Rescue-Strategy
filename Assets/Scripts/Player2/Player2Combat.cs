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

    private bool isAttackHeld = false;

    private static readonly int HashIsDead = Animator.StringToHash("IsDead");

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        controller = GetComponent<Player2Controller>();
        weaponManager = GetComponent<WeaponManager>();
        health = GetComponent<HealthComponent>();
        anim = GetComponent<Animator>();

        health.onDeath.AddListener(OnPlayerDeath);
    }

    void Update()
    {
        if (!health.IsAlive) return;

        HandleAttackInput();
        HandleReloadInput();
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