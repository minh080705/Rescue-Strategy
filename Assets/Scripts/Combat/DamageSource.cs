// Assets/Scripts/Combat/DamageSource.cs
// Phân loại nguồn gây damage.
// Dùng để xử lý hậu quả khác nhau:
//   ví dụ Enemy đánh Hostage → TriggerLose()
//         Player đánh Enemy  → ReturnToPool()

public enum DamageSource
{
    Player,
    Enemy,
    Environment,  // bẫy, lửa... dùng sau
    Hostage
}