// Assets/Scripts/UI/ScreenSpaceHPBar.cs

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

    [Header("Bar Colors")]
    [Tooltip("Màu khi HP = 0%")]
    public Color lowHPColor = Color.red;

    [Tooltip("Màu khi HP = 100%")]
    public Color fullHPColor = Color.green;

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
            fillImage.color = Color.Lerp(lowHPColor, fullHPColor, ratio);
        }

        if (hpText != null)
            hpText.text = $"{current} / {max}";
    }
}