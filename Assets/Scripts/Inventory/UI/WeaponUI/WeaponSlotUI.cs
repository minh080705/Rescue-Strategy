// Assets/Scripts/Inventory/UI/WeaponSlotUI.cs
// Ô trang bị vũ khí trong túi đồ.
// Click vào → popup danh sách weapon có trong túi để chọn.
// Tích hợp với SharedWeaponInventory.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSlotUI : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Owner")]
    [Tooltip("Slot này thuộc về Player hay Hostage")]
    public bool isPlayerSlot = true;

    [Header("Current Weapon Display")]
    public Image weaponIcon;
    public TextMeshProUGUI weaponNameText;
    public Button weaponSlotButton;

    [Header("Weapon Popup")]
    public GameObject popupPanel;
    public Transform popupContent;    // parent của các weapon option button
    public GameObject weaponOptionPrefab; // prefab 1 nút chọn weapon

    [Header("Icons")]
    public Sprite bareHandIcon;
    public Sprite spearIcon;
    public Sprite gunIcon;

    // ── Internal ──────────────────────────────────────────
    private bool isPopupOpen = false;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        weaponSlotButton?.onClick.AddListener(TogglePopup);
        popupPanel?.SetActive(false);

        // Lắng nghe thay đổi inventory và weapon
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.onInventoryChanged.AddListener(RefreshCurrentWeapon);

        if (SharedWeaponInventory.Instance != null)
        {
            SharedWeaponInventory.Instance.onPlayerWeaponChanged.AddListener(_ => RefreshCurrentWeapon());
            SharedWeaponInventory.Instance.onHostageWeaponChanged.AddListener(_ => RefreshCurrentWeapon());
        }
    }

    void OnDestroy()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.onInventoryChanged.RemoveListener(RefreshCurrentWeapon);

        if (SharedWeaponInventory.Instance != null)
            SharedWeaponInventory.Instance.onPlayerWeaponChanged.RemoveListener(_ => RefreshCurrentWeapon());
    }

    void Start()
    {
        RefreshCurrentWeapon();
    }

    // ── Popup ─────────────────────────────────────────────
    public void TogglePopup()
    {
        if (isPopupOpen) ClosePopup();
        else OpenPopup();
    }

    void OpenPopup()
    {
        isPopupOpen = true;
        popupPanel?.SetActive(true);
        BuildWeaponOptions();
    }

    public void ClosePopup()
    {
        isPopupOpen = false;
        popupPanel?.SetActive(false);
    }

    // ── Build danh sách weapon ────────────────────────────
    void BuildWeaponOptions()
    {
        if (popupContent == null || weaponOptionPrefab == null) return;
        // Xóa options cũ
        foreach (Transform child in popupContent)
            Destroy(child.gameObject);
        // Lấy danh sách weapon có trong túi
        List<WeaponType> availableWeapons = InventorySystem.Instance != null
            ? InventorySystem.Instance.GetWeaponsInBag()
            : new List<WeaponType> { WeaponType.None };
        WeaponType currentWeapon = SharedWeaponInventory.Instance != null
            ? (isPlayerSlot
                ? SharedWeaponInventory.Instance.PlayerWeapon
                : SharedWeaponInventory.Instance.HostageWeapon)
            : WeaponType.None;
        foreach (WeaponType weaponType in availableWeapons)
        {
            // Bỏ qua vũ khí đang trang bị, chỉ hiện các lựa chọn còn lại
            if (weaponType == currentWeapon)
                continue;

            // Grey out nếu không đủ số lượng để trang bị
            bool takenByOther = SharedWeaponInventory.Instance != null
                && !(isPlayerSlot
                    ? SharedWeaponInventory.Instance.IsAvailableForPlayer(weaponType)
                    : SharedWeaponInventory.Instance.IsAvailableForHostage(weaponType));
            GameObject optionObj = Instantiate(weaponOptionPrefab, popupContent);
            WeaponOptionUI option = optionObj.GetComponent<WeaponOptionUI>();
            if (option != null)
            {
                option.Setup(
                    weaponType,
                    GetWeaponName(weaponType) + (takenByOther ? (isPlayerSlot ? " (Hostage)" : " (Player)") : ""),
                    GetWeaponIcon(weaponType),
                    weaponType == currentWeapon,
                    takenByOther ? null : OnWeaponSelected  // null = disable
                );
            }
        }
    }

    // ── Chọn weapon ───────────────────────────────────────
    void OnWeaponSelected(WeaponType type)
    {
        if (SharedWeaponInventory.Instance != null)
        {
            if (isPlayerSlot)
                SharedWeaponInventory.Instance.AssignToPlayer(type);
            else
                SharedWeaponInventory.Instance.AssignToHostage(type);
        }

        ClosePopup();
        RefreshCurrentWeapon();
    }

    // ── Refresh ô hiện tại ────────────────────────────────
    void RefreshCurrentWeapon()
    {
        WeaponType current = SharedWeaponInventory.Instance != null
            ? (isPlayerSlot
                ? SharedWeaponInventory.Instance.PlayerWeapon
                : SharedWeaponInventory.Instance.HostageWeapon)
            : WeaponType.None;

        if (weaponIcon != null)
        {
            weaponIcon.sprite = GetWeaponIcon(current);
            weaponIcon.enabled = weaponIcon.sprite != null;
        }

        if (weaponNameText != null)
            weaponNameText.text = GetWeaponName(current);
    }

    // ── Helpers ───────────────────────────────────────────
    string GetWeaponName(WeaponType type)
    {
        return type switch
        {
            WeaponType.None => "Tay không",
            WeaponType.Spear => "Thương",
            WeaponType.Gun => "Súng",
            _ => "?"
        };
    }

    Sprite GetWeaponIcon(WeaponType type)
    {
        return type switch
        {
            WeaponType.None => bareHandIcon,
            WeaponType.Spear => spearIcon,
            WeaponType.Gun => gunIcon,
            _ => null
        };
    }
}