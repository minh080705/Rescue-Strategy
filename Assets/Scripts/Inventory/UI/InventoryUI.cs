// Assets/Scripts/Inventory/UI/InventoryUI.cs
// Giao diện túi đồ — mở/đóng bằng phím B hoặc nút UI.
// Tự động refresh khi inventory thay đổi.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Keys")]
    public KeyCode toggleKey = KeyCode.B;

    [Header("Panel")]
    public GameObject panel;
    

    
    [Tooltip("Kéo tất cả InventorySlotUI vào đây theo thứ tự")]
    public List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();

    [Header("Buttons")]
    public Button closeButton;

    [Header("tắt control khi mở inventory")]
    public GameObject mobileControls;
    // ── Internal ──────────────────────────────────────────
    private bool isOpen = false;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        closeButton?.onClick.AddListener(Close);
        panel?.SetActive(false);

        if (InventorySystem.Instance != null)
            InventorySystem.Instance.onInventoryChanged.AddListener(RefreshUI);
    }

    void OnDestroy()
    {
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.onInventoryChanged.RemoveListener(RefreshUI);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Debug.Log("[InventoryUI] B pressed");
            Toggle();
        }
            
    }

    // ── Show / Hide ───────────────────────────────────────
    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }
    ///

    public void Open()
    {
        isOpen = true;

        panel?.SetActive(true);

        mobileControls?.SetActive(false);

        RefreshUI();
    }

    public void Close()
    {
        isOpen = false;

        panel?.SetActive(false);

        mobileControls?.SetActive(true);
    }

    // ── Refresh ───────────────────────────────────────────
    void RefreshUI()
    {
        if (!isOpen || InventorySystem.Instance == null) return;

        List<InventorySlot> slots = InventorySystem.Instance.Slots;

        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (i < slots.Count)
                slotUIs[i].Refresh(slots[i]);
            else
                slotUIs[i].Refresh(null);
        }
    }
}