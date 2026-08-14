// Assets/Scripts/Inventory/ItemDefinition.cs
// ScriptableObject định nghĩa thông tin của 1 loại item.
// Tạo bằng cách: chuột phải trong Project → Create → Inventory → Item Definition

using UnityEngine;

public enum ItemType
{
    Coin,
    Key,
    Weapon,
    Consumable
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    // ── Thông tin cơ bản ──────────────────────────────────
    [Header("Info")]
    [Tooltip("ID duy nhất — dùng để tìm kiếm trong inventory")]
    public string itemID;
    public string displayName;
    [TextArea]
    public string description;
    public Sprite icon;
    public ItemType itemType;

    // ── Stack ─────────────────────────────────────────────
    [Header("Stack")]
    public bool isStackable = true;
    [Tooltip("Số lượng tối đa trong 1 ô — chỉ dùng khi isStackable = true")]
    public int maxStack = 99;

    // ── Weapon (chỉ dùng khi itemType = Weapon) ──────────
    [Header("Weapon (nếu là vũ khí)")]
    public WeaponType weaponType = WeaponType.None;

    // ── Pickup ────────────────────────────────────────────
    [Header("Pickup")]
    [Tooltip("Thời gian tồn tại trên map trước khi biến mất (giây)")]
    public float lifetime = 10f;
    [Tooltip("Thời gian bắt đầu nhấp nháy trước khi biến mất")]
    public float flickerTime = 3f;
}