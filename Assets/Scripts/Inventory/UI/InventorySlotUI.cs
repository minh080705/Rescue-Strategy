// Assets/Scripts/Inventory/UI/InventorySlotUI.cs
// Hiển thị 1 ô trong túi đồ.
// Gắn lên từng slot GameObject trong grid.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("References")]
    public Image iconImage;
    public TextMeshProUGUI quantityText;
    public GameObject emptyOverlay;  // hiện khi ô trống
    public GameObject notEmptyOverlay;

    // ── Internal ──────────────────────────────────────────
    private InventorySlot currentSlot;

    // ── Public API ────────────────────────────────────────
    public void Refresh(InventorySlot slot)
    {
        currentSlot = slot;

        if (slot == null || slot.IsEmpty)
        {
            ShowEmpty();
            return;
        }

        // Hiện icon
        if (iconImage != null)
        {
            iconImage.sprite = slot.item.icon;
            iconImage.enabled = slot.item.icon != null;
        }

        // Hiện số lượng
        if (quantityText != null)
        {
            quantityText.gameObject.SetActive(slot.item.isStackable && slot.quantity > 1);
            quantityText.text = slot.quantity.ToString();
        }

        if (notEmptyOverlay != null) {
            notEmptyOverlay.SetActive(true);
        }

        if (emptyOverlay != null)
            emptyOverlay.SetActive(false);
    }

    void ShowEmpty()
    {
        if (iconImage != null) iconImage.enabled = false;
        if (quantityText != null) quantityText.gameObject.SetActive(false);
        if (notEmptyOverlay != null) notEmptyOverlay.SetActive(false);
        if (emptyOverlay != null) emptyOverlay.SetActive(true);
        
    }
}