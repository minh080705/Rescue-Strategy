// Assets/Scripts/Player2/Weapons/BareHandWeapon.cs
// Tay không — không có attack, chỉ implement interface cho đủ.

using UnityEngine;

public class BareHandWeapon : MonoBehaviour, IWeapon
{
    public WeaponType Type => WeaponType.None;
    public bool CanAttack => false;

    public void OnEquip() { }
    public void OnUnequip() { }
    public void OnAttackHeld(Vector2 facing) { }
    public void OnAttackReleased(Vector2 facing) { }
    public void OnReload() { }
}