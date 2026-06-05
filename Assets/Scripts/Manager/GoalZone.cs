// Assets/Scripts/Managers/GoalZone.cs
// Đặt ở góc xuất phát của player.
// Khi con tin (state=Following) bước vào → SetRescued() → Win.

using UnityEngine;

public class GoalZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log($"[GoalZone] Có gì đó chạm vào: {other.gameObject.name} tag={other.tag}");

    if (!other.CompareTag("Hostage")) return;

    HostageController hostage = other.GetComponent<HostageController>();
    if (hostage == null) return;

    if (hostage.State == HostageState.Following)
        hostage.SetRescued();
}

    // Hiển thị vùng xanh trong Scene view
    void OnDrawGizmos()
    {
        BoxCollider2D col  = GetComponent<BoxCollider2D>();
        Vector3       size = col != null ? (Vector3)(col.size) : Vector3.one;

        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawCube(transform.position, size);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, size);
    }
}