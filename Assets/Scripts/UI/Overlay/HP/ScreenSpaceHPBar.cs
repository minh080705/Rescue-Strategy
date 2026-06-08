// Assets/Scripts/UI/ScreenSpaceHPBar.cs
// Thanh HP cố định góc màn hình.
// Có thể dùng cho cả Player1 và Player2 bằng cách gán targetHealth thủ công.
// Canvas phải để Render Mode = Screen Space - Overlay.

using UnityEngine;
using UnityEngine.UI;

public class ScreenSpaceHPBar : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────
    [Header("Target")]
    [Tooltip("Kéo Player1 hoặc Player2 vào đây. Để trống = tự tìm HealthComponent trên GameObject này")]
    public HealthComponent targetHealth;

    [Header("References")]
    [Tooltip("Kéo Image (fill) của thanh HP vào đây")]
    public Image fillImage;
    [Tooltip("Kéo TextMeshPro hiện số HP vào đây (tuỳ chọn)")]
    public TMPro.TextMeshProUGUI hpText;

    // ── Private ──────────────────────────────────────────
    private HealthComponent health;

    // ── Unity lifecycle ──────────────────────────────────
    void Awake()
    {
        health = targetHealth != null
            ? targetHealth
            : GetComponent<HealthComponent>();

        if (health == null)
        {
            Debug.LogError($"[ScreenSpaceHPBar] {gameObject.name}: Không tìm thấy HealthComponent!");
            return;
        }

        health.onHPChanged.AddListener(OnHPChanged);
    }

    void Start()
    {
        // Start đảm bảo tất cả Awake đã chạy xong
        // → CurrentHP đã được set đúng bởi HealthComponent.Awake
        if (health != null)
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
        if (fillImage != null)
        {
            float ratio = max > 0 ? (float)current / max : 0f;
            fillImage.fillAmount = ratio;
            fillImage.color = Color.Lerp(Color.red, Color.green, ratio);
        }

        if (hpText != null)
            hpText.text = $"{current} / {max}";
    }
}