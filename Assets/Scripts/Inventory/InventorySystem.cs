// Assets/Scripts/Inventory/InventorySystem.cs
// Singleton quản lý túi đồ của Player.
// Tối đa 20 ô, item cùng loại stack vào nhau.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InventorySlot
{
    public ItemDefinition item;
    public int quantity;

    public bool IsEmpty => item == null;

    public InventorySlot()
    {
        item = null;
        quantity = 0;
    }
}

public class InventorySystem : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────
    public static InventorySystem Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────
    [Header("Settings")]
    public int maxSlots = 20;

    // ── Events — UI lắng nghe ─────────────────────────────
    public UnityEvent onInventoryChanged;
    public UnityEvent<string> onItemAdded;    // itemID
    public UnityEvent<string> onItemRemoved;  // itemID

    // ── Data ──────────────────────────────────────────────
    public List<InventorySlot> Slots { get; private set; }

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        InitSlots();
    }

    void InitSlots()
    {
        Slots = new List<InventorySlot>(maxSlots);
        for (int i = 0; i < maxSlots; i++)
            Slots.Add(new InventorySlot());
    }

    // ── Public API ────────────────────────────────────────

    /// <summary>Thêm item vào túi. Trả về true nếu thành công.</summary>
    public bool AddItem(ItemDefinition item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        int remaining = quantity;

        // Stack vào ô đã có cùng loại
        if (item.isStackable)
        {
            foreach (InventorySlot slot in Slots)
            {
                if (slot.IsEmpty || slot.item != item) continue;
                if (slot.quantity >= item.maxStack) continue;

                int canAdd = item.maxStack - slot.quantity;
                int adding = Mathf.Min(canAdd, remaining);

                slot.quantity += adding;
                remaining -= adding;

                if (remaining <= 0) break;
            }
        }

        // Thêm vào ô trống
        while (remaining > 0)
        {
            InventorySlot emptySlot = GetEmptySlot();
            if (emptySlot == null)
            {
                Debug.Log("[Inventory] Túi đầy!");
                return false;
            }

            int adding = item.isStackable ? Mathf.Min(item.maxStack, remaining) : 1;
            emptySlot.item = item;
            emptySlot.quantity = adding;
            remaining -= adding;
        }

        Debug.Log($"[Inventory] Nhặt: {item.displayName} x{quantity}");
        onItemAdded?.Invoke(item.itemID);
        onInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>Xóa item khỏi túi. Trả về true nếu thành công.</summary>
    public bool RemoveItem(string itemID, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemID) || quantity <= 0) return false;

        int remaining = quantity;

        for (int i = Slots.Count - 1; i >= 0; i--)
        {
            InventorySlot slot = Slots[i];
            if (slot.IsEmpty || slot.item.itemID != itemID) continue;

            int removing = Mathf.Min(slot.quantity, remaining);
            slot.quantity -= removing;
            remaining -= removing;

            if (slot.quantity <= 0)
            {
                slot.item = null;
                slot.quantity = 0;
            }

            if (remaining <= 0) break;
        }

        if (remaining > 0)
        {
            Debug.Log($"[Inventory] Không đủ {itemID}!");
            return false;
        }

        onItemRemoved?.Invoke(itemID);
        onInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>Kiểm tra có item không.</summary>
    public bool HasItem(string itemID, int quantity = 1)
    {
        return GetItemCount(itemID) >= quantity;
    }

    /// <summary>Đếm tổng số lượng của 1 loại item.</summary>
    public int GetItemCount(string itemID)
    {
        int total = 0;
        foreach (InventorySlot slot in Slots)
        {
            if (!slot.IsEmpty && slot.item.itemID == itemID)
                total += slot.quantity;
        }
        return total;
    }

    /// <summary>Lấy danh sách weapon đang có trong túi.</summary>
    public List<WeaponType> GetWeaponsInBag()
    {
        List<WeaponType> weapons = new List<WeaponType>();
        weapons.Add(WeaponType.None); // BareHand luôn có

        foreach (InventorySlot slot in Slots)
        {
            if (slot.IsEmpty) continue;
            if (slot.item.itemType != ItemType.Weapon) continue;
            if (!weapons.Contains(slot.item.weaponType))
                weapons.Add(slot.item.weaponType);
        }

        return weapons;
    }

    /// <summary>Lấy ItemDefinition theo ID.</summary>
    public ItemDefinition GetItemDefinition(string itemID)
    {
        foreach (InventorySlot slot in Slots)
        {
            if (!slot.IsEmpty && slot.item.itemID == itemID)
                return slot.item;
        }
        return null;
    }

    // ── Helper ────────────────────────────────────────────
    InventorySlot GetEmptySlot()
    {
        foreach (InventorySlot slot in Slots)
            if (slot.IsEmpty) return slot;
        return null;
    }

    public bool IsFull()
    {
        return GetEmptySlot() == null;
    }
}