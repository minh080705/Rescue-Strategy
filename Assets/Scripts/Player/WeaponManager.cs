// Assets/Scripts/Player2/WeaponManager.cs
// Quản lý đổi weapon bằng phím 1/2/3 hoặc qua Inventory UI.
// Lắng nghe SharedWeaponInventory.onPlayerWeaponChanged để tự đổi weapon.

using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Weapon Keys")]
    [Tooltip("Tắt nếu là Hostage — Hostage không nhận keyboard input")]
    public bool useKeyboardInput = true;
    public KeyCode keyBareHand = KeyCode.Alpha1;
    public KeyCode keySpear = KeyCode.Alpha2;
    public KeyCode keyGun = KeyCode.Alpha3;

    [Header("Weapon Components")]
    public MonoBehaviour bareHandWeapon;
    public MonoBehaviour spearWeapon;
    public MonoBehaviour gunWeapon;

    // ── Properties ────────────────────────────────────────
    public WeaponType CurrentType { get; private set; } = WeaponType.None;
    public IWeapon CurrentWeapon { get; private set; }

    public event System.Action<WeaponType> OnWeaponEquipped;
    // ── Internal ──────────────────────────────────────────
    private Animator anim;
    private static readonly int HashWeaponType = Animator.StringToHash("WeaponType");
    private static readonly int HashWeaponTypeF = Animator.StringToHash("WeaponTypeF");

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        // Lắng nghe SharedWeaponInventory
        if (SharedWeaponInventory.Instance != null)
        {
            if (useKeyboardInput) // Player
                SharedWeaponInventory.Instance.onPlayerWeaponChanged.AddListener(OnInventoryWeaponChanged);
            else                  // Hostage
                SharedWeaponInventory.Instance.onHostageWeaponChanged.AddListener(OnInventoryWeaponChanged);

            // Đồng bộ weapon ban đầu từ inventory
            WeaponType startWeapon = useKeyboardInput
                ? SharedWeaponInventory.Instance.PlayerWeapon
                : SharedWeaponInventory.Instance.HostageWeapon;

            EquipWeapon(startWeapon);
        }
        else
        {
            EquipWeapon(WeaponType.None);
        }
    }

    void OnDestroy()
    {
        if (SharedWeaponInventory.Instance != null)
        {
            SharedWeaponInventory.Instance.onPlayerWeaponChanged.RemoveListener(OnInventoryWeaponChanged);
            SharedWeaponInventory.Instance.onHostageWeaponChanged.RemoveListener(OnInventoryWeaponChanged);
        }
    }

    void Update()
    {
        HandleWeaponSwitch();
    }

    // ── Input keyboard ────────────────────────────────────
    void HandleWeaponSwitch()
    {
        if (!useKeyboardInput) return;

        if (Input.GetKeyDown(keyBareHand)) AssignWeapon(WeaponType.None);
        if (Input.GetKeyDown(keySpear)) AssignWeapon(WeaponType.Spear);
        if (Input.GetKeyDown(keyGun)) AssignWeapon(WeaponType.Gun);
    }

    /// <summary>Đổi weapon qua keyboard — báo cho SharedWeaponInventory.</summary>
    void AssignWeapon(WeaponType type)
    {
        if (SharedWeaponInventory.Instance != null)
            SharedWeaponInventory.Instance.AssignToPlayer(type);
        else
            EquipWeapon(type);
    }

    // ── Inventory callback ────────────────────────────────
    void OnInventoryWeaponChanged(WeaponType type)
    {
        EquipWeapon(type);
    }

    // ── Equip ─────────────────────────────────────────────
    public void EquipWeapon(WeaponType type)
    {
        if (CurrentType == type) return;

        CurrentWeapon?.OnUnequip();
        CurrentType = type;
        CurrentWeapon = GetWeapon(type);
        CurrentWeapon?.OnEquip();

        if (anim != null)
        {
            anim.SetInteger(HashWeaponType, (int)type);
            anim.SetFloat(HashWeaponTypeF, (int)type);
        }

        Debug.Log($"[WeaponManager] Đổi sang {type}");
        OnWeaponEquipped?.Invoke(type);
    }

    // ── Helper ────────────────────────────────────────────
    IWeapon GetWeapon(WeaponType type)
    {
        return type switch
        {
            WeaponType.None => bareHandWeapon as IWeapon,
            WeaponType.Spear => spearWeapon as IWeapon,
            WeaponType.Gun => gunWeapon as IWeapon,
            _ => null
        };
    }
}