// Assets/Scripts/UI/HostageWeaponUI.cs
// Popup chọn weapon cho Hostage — hiện giữa màn hình khi bấm Tab.
// Mỗi nút hiện weapon name, grey out nếu Player đang cầm.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HostageWeaponUI : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Keys")]
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("Panel")]
    [Tooltip("Root panel của popup — bật/tắt để show/hide")]
    public GameObject panel;

    [Header("Buttons")]
    public Button bareHandButton;
    public Button spearButton;
    public Button gunButton;

    [Header("Button Labels")]
    public TextMeshProUGUI bareHandLabel;
    public TextMeshProUGUI spearLabel;
    public TextMeshProUGUI gunLabel;

    [Header("Colors")]
    public Color availableColor = Color.white;
    public Color unavailableColor = new Color(0.5f, 0.5f, 0.5f, 1f); // grey
    public Color selectedColor = new Color(0.3f, 0.8f, 0.3f, 1f); // xanh lá

    // ── Internal ──────────────────────────────────────────
    private bool isOpen = false;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        // Gán onclick
        bareHandButton?.onClick.AddListener(() => SelectWeapon(WeaponType.None));
        spearButton?.onClick.AddListener(() => SelectWeapon(WeaponType.Spear));
        gunButton?.onClick.AddListener(() => SelectWeapon(WeaponType.Gun));

        // Lắng nghe inventory thay đổi để refresh UI
        if (SharedWeaponInventory.Instance != null)
        {
            SharedWeaponInventory.Instance.onPlayerWeaponChanged.AddListener(_ => RefreshUI());
            SharedWeaponInventory.Instance.onHostageWeaponChanged.AddListener(_ => RefreshUI());
        }

        panel?.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    // ── Show / Hide ───────────────────────────────────────
    void Toggle()
    {
        isOpen = !isOpen;
        panel?.SetActive(isOpen);

        if (isOpen)
            RefreshUI();
    }

    public void Close()
    {
        isOpen = false;
        panel?.SetActive(false);
    }

    // ── Refresh UI ────────────────────────────────────────
    void RefreshUI()
    {
        if (SharedWeaponInventory.Instance == null) return;

        WeaponType playerWeapon = SharedWeaponInventory.Instance.PlayerWeapon;
        WeaponType hostageWeapon = SharedWeaponInventory.Instance.HostageWeapon;

        UpdateButton(bareHandButton, bareHandLabel, WeaponType.None, "Tay không", playerWeapon, hostageWeapon);
        UpdateButton(spearButton, spearLabel, WeaponType.Spear, "Thương", playerWeapon, hostageWeapon);
        UpdateButton(gunButton, gunLabel, WeaponType.Gun, "Súng", playerWeapon, hostageWeapon);
    }

    void UpdateButton(Button btn, TextMeshProUGUI label, WeaponType type,
                      string displayName, WeaponType playerWeapon, WeaponType hostageWeapon)
    {
        if (btn == null) return;

        bool isPlayerUsing = type != WeaponType.None && type == playerWeapon;
        bool isHostageUsing = type == hostageWeapon;
        bool isAvailable = !isPlayerUsing;

        // Interactable
        btn.interactable = isAvailable;

        // Màu text
        if (label != null)
        {
            if (isHostageUsing)
                label.color = selectedColor;    // đang chọn → xanh lá
            else if (!isAvailable)
                label.color = unavailableColor; // Player đang cầm → grey
            else
                label.color = availableColor;   // có thể chọn → trắng

            // Thêm icon trạng thái vào label
            string suffix = isHostageUsing ? " ✓"
                          : isPlayerUsing ? " (Player)"
                          : "";
            label.text = displayName + suffix;
        }
    }

    // ── Select weapon ─────────────────────────────────────
    void SelectWeapon(WeaponType type)
    {
        if (SharedWeaponInventory.Instance == null) return;

        bool success = SharedWeaponInventory.Instance.AssignToHostage(type);

        if (success)
        {
            RefreshUI();
            Close(); // đóng popup sau khi chọn
            Debug.Log($"[HostageWeaponUI] Chọn {type} cho Hostage");
        }
        else
        {
            Debug.Log($"[HostageWeaponUI] {type} đang được Player dùng!");
        }
    }
}