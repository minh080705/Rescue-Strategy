// Assets/Scripts/UI/DashCooldownUI.cs
// Hiển thị cooldown vòng tròn + text đếm ngược trên nút Dash.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DashCooldownUI : MonoBehaviour
{
    [Header("Refs")]
    public Player2Controller player;

    [Header("UI Elements")]
    [Tooltip("Image dạng Filled (360°) làm vòng tròn cooldown")]
    public Image cooldownRing;

    [Tooltip("TextMeshPro hiển thị giây còn lại — ẩn khi sẵn sàng")]
    public TextMeshProUGUI cooldownText;

    [Tooltip("Icon bolt / sprite nút dash — ẩn khi đang cooldown")]
    public GameObject readyIcon;

    [Header("Colors")]
    public Color colorReady = new Color(0.31f, 0.76f, 0.97f); // xanh
    public Color colorCooling = new Color(0.47f, 0.52f, 0.80f); // tím
    public Color colorJustUsed = new Color(0.94f, 0.33f, 0.31f); // đỏ

    void Update()
    {
        if (player == null || cooldownRing == null) return;

        float remaining = player.CooldownTimer;          // giây còn lại
        float total = player.DashCooldown;           // tổng thời gian hồi

        bool onCooldown = remaining > 0f;

        // ── FillAmount ──────────────────────────────────
        // 1 = đầy (sẵn sàng), 0 = rỗng (vừa dùng)
        float fill = onCooldown ? (1f - remaining / total) : 1f;
        cooldownRing.fillAmount = fill;

        // ── Màu sắc theo trạng thái ────────────────────
        if (!onCooldown)
        {
            cooldownRing.color = colorReady;
        }
        else if (remaining > total * 0.5f)
        {
            // Vừa dùng: đỏ
            cooldownRing.color = colorJustUsed;
        }
        else
        {
            // Gần xong: tím
            cooldownRing.color = colorCooling;
        }

        // ── Text đếm ngược ─────────────────────────────
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(onCooldown);
            if (onCooldown)
                cooldownText.text = remaining.ToString("F1") + "s";
        }

        // ── Icon sẵn sàng ─────────────────────────────
        if (readyIcon != null)
            readyIcon.SetActive(!onCooldown);
    }
}