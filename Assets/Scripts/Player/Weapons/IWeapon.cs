// Assets/Scripts/Player2/Weapons/IWeapon.cs
// Interface chung cho tất cả weapon.
// Player2Combat chỉ gọi qua interface này — không biết weapon cụ thể là gì.

using UnityEngine;

public enum WeaponType { None = 0, Spear = 1, Gun = 2 }

public interface IWeapon
{
    WeaponType Type { get; }

    void OnEquip();                          // gọi khi chuyển sang weapon này
    void OnUnequip();                        // gọi khi chuyển sang weapon khác

    void OnAttackHeld(Vector2 facing);       // giữ phím attack
    void OnAttackReleased(Vector2 facing);   // thả phím attack

    void OnReload();                         // bấm phím R (chỉ Gun dùng)

    bool CanAttack { get; }                  // Player2Combat hỏi trước khi cho attack
}