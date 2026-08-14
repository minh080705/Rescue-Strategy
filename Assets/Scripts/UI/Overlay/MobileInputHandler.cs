// Assets/Scripts/UI/MobileInputHandler.cs
// Nhận input từ Joystick và các nút UI.
// Feed vào Player2Controller và Player2Combat thay cho keyboard.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileInputHandler : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("References")]
    public FloatingJoystick joystick;
    public Button attackButton;
    public Button dashButton;
    public Button weaponButton;
    [Header("Reload")]
    public Button reloadButton;
    [Header("Inventory")]
    public Button inventoryButton;
    public InventoryUI inventoryUI;

    [Header("Player")]
    public Player2Controller controller;
    public Player2Combat combat;
    public WeaponManager weaponManager;

    // ── Internal ──────────────────────────────────────────
    private bool isAttackHeld = false;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
       

        SetupButtons();
        if (weaponManager != null)
            weaponManager.OnWeaponEquipped += OnWeaponChanged;

        // Ẩn reload ngay từ đầu (weapon mặc định là None/Spear)
        RefreshReloadButton(weaponManager != null ? weaponManager.CurrentType : WeaponType.None);
    }

    void Update()
    {
        HandleJoystick();
        HandleAttackHeld();
    }

    // ── Setup buttons ─────────────────────────────────────
    void SetupButtons()
    {
        // Dash
        dashButton?.onClick.AddListener(OnDashPressed);

        reloadButton?.onClick.AddListener(OnReloadPressed);
        // Weapon
        weaponButton?.onClick.AddListener(OnWeaponPressed);

        inventoryButton?.onClick.AddListener(OnInventoryPressed);

        // Attack — dùng EventTrigger để detect giữ và thả
        if (attackButton != null)
        {
            EventTrigger trigger = attackButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = attackButton.gameObject.AddComponent<EventTrigger>();

            // Pointer Down — bắt đầu attack
            EventTrigger.Entry downEntry = new EventTrigger.Entry();
            downEntry.eventID = EventTriggerType.PointerDown;
            downEntry.callback.AddListener(_ => isAttackHeld = true);
            trigger.triggers.Add(downEntry);

            // Pointer Up — dừng attack
            EventTrigger.Entry upEntry = new EventTrigger.Entry();
            upEntry.eventID = EventTriggerType.PointerUp;
            upEntry.callback.AddListener(_ =>
            {
                isAttackHeld = false;

                // Báo weapon dừng
                IWeapon weapon = weaponManager?.CurrentWeapon;
                if (weapon != null && controller != null)
                    weapon.OnAttackReleased(controller.LastFacing);
            });
            trigger.triggers.Add(upEntry);
        }
        
    }

    // ── Joystick → Controller ─────────────────────────────
    void HandleJoystick()
    {
        if (controller == null || joystick == null) return;

        Vector2 input = new Vector2(joystick.Horizontal, joystick.Vertical);

        if (input.magnitude > 0.1f)
            controller.SetJoystickInput(input);
        else
            controller.SetJoystickInput(Vector2.zero);
    }

    // ── Attack held mỗi frame ────────────────────────────
    void HandleAttackHeld()
    {
        if (!isAttackHeld || controller == null || weaponManager == null) return;

        IWeapon weapon = weaponManager.CurrentWeapon;
        if (weapon == null) return;

        // Feed OnAttackHeld mỗi frame khi đang giữ nút
        // SpearWeapon có guard isAttacking → không bị loop animation
        // GunWeapon chỉ set isFireHeld = true → bắn theo fireRate
        weapon.OnAttackHeld(controller.LastFacing);
    }

    // ── Button callbacks ──────────────────────────────────

    void OnInventoryPressed()
    {
        inventoryUI?.Toggle();
    }

    void OnDashPressed()
    {
        controller?.TriggerDash();
    }

    void OnWeaponPressed()
    {
        FindObjectOfType<WeaponSlotUI>()?.TogglePopup();
    }

    void OnDestroy()
    {
        if (weaponManager != null)
            weaponManager.OnWeaponEquipped -= OnWeaponChanged;
    }

    void OnWeaponChanged(WeaponType type)
    {
        RefreshReloadButton(type);
    }

    void RefreshReloadButton(WeaponType type)
    {
        if (reloadButton == null) return;
        reloadButton.gameObject.SetActive(type == WeaponType.Gun);
    }

    void OnReloadPressed()
    {
        weaponManager?.CurrentWeapon?.OnReload();
    }

}