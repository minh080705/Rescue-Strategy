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
    

    // ── Unity lifecycle ───────────────────────────────────

    /// <summary>Gọi từ BatController khi vào PreAttack.</summary>
    public void Show(Vector2 position, float showDuration, GameObject target = null)
    {
        transform.position = position;
        duration = showDuration;
        timer = showDuration;

        if (target != null)
        {
            gameObject.layer = target.layer;

            SpriteRenderer targetRenderer = target.GetComponentInChildren<SpriteRenderer>();
            SpriteRenderer myRenderer = GetComponentInChildren<SpriteRenderer>();

            if (targetRenderer != null && myRenderer != null)
            {
                myRenderer.sortingLayerID = targetRenderer.sortingLayerID;
                myRenderer.sortingOrder = 2;
            }
        }
        gameObject.SetActive(true);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            gameObject.SetActive(false);
    }
}