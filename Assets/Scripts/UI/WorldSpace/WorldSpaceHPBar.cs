// Assets/Scripts/UI/WorldSpaceHPBar.cs
// Thanh HP nổi trên đầu nhân vật — dùng cho Enemy và Hostage.
// Attach vào cùng GameObject với HealthComponent.
// Tự tìm HealthComponent và đăng ký lắng nghe onHPChanged.

using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceHPBar : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────
    [Header("References")]
    [Tooltip("Kéo Image (fill) của thanh HP vào đây")]
    public Image fillImage;

    [Header("Position")]
    [Tooltip("Độ cao nổi trên đầu nhân vật (units)")]
    public float offsetY = 0.8f;

    [Header("Display")]
    [Tooltip("Ẩn thanh HP khi máu đầy")]
    public bool hideWhenFull = true;

    // ── Private ──────────────────────────────────────────
    private HealthComponent health;
    private Transform        barTransform; // Canvas/root của thanh HP

    // ── Unity lifecycle ──────────────────────────────────
    void Awake()
    {
        // barTransform là Canvas world space — là parent của fillImage
        barTransform = fillImage.transform.root;

        health = GetComponent<HealthComponent>();
        if (health == null)
        {
            Debug.LogError($"[WorldSpaceHPBar] Không tìm thấy HealthComponent trên {gameObject.name}!");
            return;
        }

        // Đăng ký lắng nghe event HP thay đổi
        health.onHPChanged.AddListener(OnHPChanged);

        // Khởi tạo đúng giá trị ngay từ đầu
        UpdateBar(health.CurrentHP, health.MaxHP);
    }

    void LateUpdate()
    {
        // Giữ thanh HP luôn nổi trên đầu nhân vật
        // Dùng LateUpdate để chạy sau khi physics di chuyển nhân vật xong
        barTransform.position = transform.position + Vector3.up * offsetY;
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
        if (fillImage == null) return;

        float ratio = max > 0 ? (float)current / max : 0f;
        fillImage.fillAmount = ratio;

        // Đổi màu theo % HP: xanh → vàng → đỏ
        fillImage.color = Color.Lerp(Color.red, Color.green, ratio);

        // Ẩn khi máu đầy (tuỳ chọn)
        if (hideWhenFull)
            barTransform.gameObject.SetActive(current < max);
    }
}