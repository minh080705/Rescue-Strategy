// Assets/Scripts/Inventory/UI/WeaponOptionUI.cs
// 1 nút trong popup chọn weapon.
// Instantiate bởi WeaponSlotUI.

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponOptionUI : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    public Image icon;
    public TextMeshProUGUI nameText;
    public Image selectedHighlight; // viền xanh khi đang chọn
    public Button button;

    // ── Internal ──────────────────────────────────────────
    private WeaponType weaponType;
    private Action<WeaponType> onSelected;

    // ── Setup ─────────────────────────────────────────────
    public void Setup(WeaponType type, string displayName, Sprite weaponIcon,
                      bool isSelected, Action<WeaponType> callback)
    {
        weaponType = type;
        onSelected = callback;

        if (nameText != null) nameText.text = displayName;
        if (icon != null)
        {
            icon.sprite = weaponIcon;
            icon.enabled = weaponIcon != null;
        }
        if (selectedHighlight != null)
            selectedHighlight.gameObject.SetActive(isSelected);

        // Disable button nếu callback null (weapon đang được bên kia dùng)
        bool isDisabled = callback == null;
        if (button != null)
        {
            button.interactable = !isDisabled;
            if (!isDisabled)
                button.onClick.AddListener(() => onSelected?.Invoke(weaponType));
        }

        // Làm mờ nếu disabled
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = isDisabled ? 0.4f : 1f;
    }
}