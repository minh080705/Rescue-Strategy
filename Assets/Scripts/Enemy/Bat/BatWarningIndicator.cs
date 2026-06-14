// Assets/Scripts/Enemy/Bat/BatWarningIndicator.cs
// Hiện cảnh báo tại vị trí target đã lock — chạy animation rồi tự tắt.
// Spawn cùng lúc với PreAttack, tồn tại đúng bằng attackDelay.

using UnityEngine;

public class BatWarningIndicator : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Timing")]
    [Tooltip("Thời gian hiện cảnh báo — nên bằng attackDelay của BatController")]
    public float duration = 2.0f;

    // ── Internal ──────────────────────────────────────────
    private float timer;
    private Animator anim;

    private static readonly int HashShow = Animator.StringToHash("Show");

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    /// <summary>Gọi từ BatController khi vào PreAttack.</summary>
    public void Show(Vector2 position, float showDuration)
    {
        transform.position = position;
        duration = showDuration;
        timer = showDuration;

        gameObject.SetActive(true);
        anim?.SetTrigger(HashShow);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            gameObject.SetActive(false);
    }
}