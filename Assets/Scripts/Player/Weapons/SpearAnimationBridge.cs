// Assets/Scripts/Player2/Weapons/SpearAnimationBridge.cs
// Đặt trên cùng GameObject với Animator.
// Nhận Animation Event rồi forward xuống SpearWeapon ở child.

using UnityEngine;

public class SpearAnimationBridge : MonoBehaviour
{
    private SpearWeapon spearWeapon;

    void Awake()
    {
        spearWeapon = GetComponentInChildren<SpearWeapon>();
    }

    // Animation Event frame bắt đầu hitbox
    public void OnSpearHit() => spearWeapon?.OnSpearHit();

    // Animation Event frame cuối clipp
    public void OnSpearHitEnd() => spearWeapon?.OnSpearHitEnd();
}