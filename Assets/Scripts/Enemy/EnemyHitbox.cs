// Assets/Scripts/Enemy/EnemyHitbox.cs
// Hitbox body của enemy — luôn bật, cham vào ai thì gây damage cho người đó.
// Dùng HitboxComponent với targetTag linh hoạt.
//
// Cách setup:
//   Attach trực tiếp lên Enemy GameObject (không cần child riêng).
//   targetTag = "Player"  → enemy body làm hại player khi chạm.
//
// Để enemy cũng làm hại hostage:
//   Tạo thêm HitboxComponent thứ 2 với targetTag = "Hostage".

using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    // Enemy body luôn là hitbox đang bật
    // → chỉ cần HitboxComponent với SetActive(true) từ đầu
    void Awake()
    {
        // Tìm tất cả HitboxComponent trên object này và bật hết
        foreach (HitboxComponent hb in GetComponents<HitboxComponent>())
        {
            hb.SetActive(true);
        }
    }
}