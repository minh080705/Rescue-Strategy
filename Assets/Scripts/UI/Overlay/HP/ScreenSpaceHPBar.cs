// Assets/Scripts/UI/ScreenSpaceHPBar.cs
// Thanh HP cố định góc màn hình — chỉ dùng cho Player.
// Attach vào Player GameObject.
// Canvas phải để Render Mode = Screen Space - Overlay.

using UnityEngine;
using UnityEngine.UI;

public class ScreenSpaceHPBar : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────
    [Header("References")]
    [Tooltip("Kéo Image (fill) của thanh HP Player vào đây")]
    public Image fillImage;

    [Tooltip("Kéo Text hoặc TextMeshPro hiện số HP vào đây (tuỳ chọn)")]
    public TMPro.TextMeshProUGUI hpText;

    // ── Private ──────────────────────────────────────────
    private HealthComponent health;

    // ── Unity lifecycle ──────────────────────────────────
    void Awake()
    {
        health = GetComponent<HealthComponent>();
        if (health == null)
        {
            Debug.LogError("[ScreenSpaceHPBar] Không tìm thấy HealthComponent trên Player!");
            return;
        }

        health.onHPChanged.AddListener(OnHPChanged);
        UpdateBar(health.CurrentHP, health.MaxHP);
    }

    void OnDestroy()
    {
        if (health != null)
            health.onHPChanged.RemoveListener(OnHPChanged);
    }

    // ── Callback ─────────────────────────────────────────
    void OnHPChanged(int current, int max)
    {
        UpdateBar(current, max);
    }

    void UpdateBar(int current, int max)
    {
        // Fill
        if (fillImage != null)
        {
            float ratio = max > 0 ? (float)current / max : 0f;
            fillImage.fillAmount = ratio;
            fillImage.color = Color.Lerp(Color.red, Color.green, ratio);
        }

        // Text số HP (tuỳ chọn)
        if (hpText != null)
            hpText.text = $"{current} / {max}";
    }
}