// Assets/Scripts/Shared/SharedWeaponInventory.cs
// Quản lý weapon trang bị cho Player và Hostage.
// Rule: cùng loại weapon có thể trang bị cho cả 2 nếu số lượng trong túi >= 2.
// Nếu chỉ có 1 thì không cho 2 người cùng cầm.

using UnityEngine;
using UnityEngine.Events;

public class SharedWeaponInventory : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────
    public static SharedWeaponInventory Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────
    [Header("Initial Weapons")]
    public WeaponType playerStartWeapon = WeaponType.None;
    public WeaponType hostageStartWeapon = WeaponType.None;

    // ── Properties ────────────────────────────────────────
    public WeaponType PlayerWeapon { get; private set; } = WeaponType.None;
    public WeaponType HostageWeapon { get; private set; } = WeaponType.None;

    // ── Events ────────────────────────────────────────────
    public UnityEvent<WeaponType> onPlayerWeaponChanged;
    public UnityEvent<WeaponType> onHostageWeaponChanged;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        AssignToPlayer(playerStartWeapon);
        AssignToHostage(hostageStartWeapon);
    }

    // ── Public API ────────────────────────────────────────

    public bool AssignToPlayer(WeaponType type)
    {
        if (!CanEquip(type, isPlayer: true))
        {
            Debug.Log($"[SharedWeaponInventory] Không đủ {type} để trang bị cho Player!");
            return false;
        }

        SetPlayerWeapon(type);
        return true;
    }

    public bool AssignToHostage(WeaponType type)
    {
        if (!CanEquip(type, isPlayer: false))
        {
            Debug.Log($"[SharedWeaponInventory] Không đủ {type} để trang bị cho Hostage!");
            return false;
        }

        SetHostageWeapon(type);
        return true;
    }

    /// <summary>
    /// Kiểm tra có thể trang bị weapon này không.
    /// BareHand luôn cho phép.
    /// Cùng loại với bên kia → cần số lượng >= 2.
    /// Khác loại → cần số lượng >= 1.
    /// </summary>
    public bool CanEquip(WeaponType type, bool isPlayer)
    {
        if (type == WeaponType.None) return true; // BareHand luôn OK

        WeaponType otherWeapon = isPlayer ? HostageWeapon : PlayerWeapon;
        int count = GetWeaponCount(type);

        if (type == otherWeapon)
            return count >= 2; // cả 2 cùng cầm → cần 2 cái
        else
            return count >= 1; // chỉ 1 người cầm → cần 1 cái
    }

    /// <summary>Dùng cho WeaponSlotUI để grey out option không hợp lệ.</summary>
    public bool IsAvailableForPlayer(WeaponType type) => CanEquip(type, isPlayer: true);
    public bool IsAvailableForHostage(WeaponType type) => CanEquip(type, isPlayer: false);

    // ── Internal ──────────────────────────────────────────
    void SetPlayerWeapon(WeaponType type)
    {
        PlayerWeapon = type;
        onPlayerWeaponChanged?.Invoke(type);
        Debug.Log($"[SharedWeaponInventory] Player → {type}");
    }

    void SetHostageWeapon(WeaponType type)
    {
        HostageWeapon = type;
        onHostageWeaponChanged?.Invoke(type);
        Debug.Log($"[SharedWeaponInventory] Hostage → {type}");
    }

    /// <summary>Đếm số lượng weapon loại này trong túi đồ.</summary>
    int GetWeaponCount(WeaponType type)
    {
        if (InventorySystem.Instance == null) return 0;

        // Map WeaponType sang itemID
        string itemID = type switch
        {
            WeaponType.Spear => "spear",
            WeaponType.Gun => "gun",
            _ => ""
        };

        if (string.IsNullOrEmpty(itemID)) return 0;
        return InventorySystem.Instance.GetItemCount(itemID);
    }
}