// Assets/Scripts/Player2/WeaponManager.cs
// Thêm set cả WeaponTypeF (Float) khi đổi weapon

using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Keys")]
    public KeyCode keyBareHand = KeyCode.Alpha1;
    public KeyCode keySpear = KeyCode.Alpha2;
    public KeyCode keyGun = KeyCode.Alpha3;

    [Header("Weapon Components")]
    public MonoBehaviour bareHandWeapon;
    public MonoBehaviour spearWeapon;
    public MonoBehaviour gunWeapon;

    public WeaponType CurrentType { get; private set; } = WeaponType.None;
    public IWeapon CurrentWeapon { get; private set; }

    private Animator anim;
    private static readonly int HashWeaponType = Animator.StringToHash("WeaponType");
    private static readonly int HashWeaponTypeF = Animator.StringToHash("WeaponTypeF"); // ← thêm

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        EquipWeapon(WeaponType.None);
    }

    void Update()
    {
        HandleWeaponSwitch();
    }

    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(keyBareHand)) EquipWeapon(WeaponType.None);
        if (Input.GetKeyDown(keySpear)) EquipWeapon(WeaponType.Spear);
        if (Input.GetKeyDown(keyGun)) EquipWeapon(WeaponType.Gun);
    }

    public void EquipWeapon(WeaponType type)
    {
        if (CurrentType == type) return;

        CurrentWeapon?.OnUnequip();
        CurrentType = type;
        CurrentWeapon = GetWeapon(type);
        CurrentWeapon?.OnEquip();

        if (anim != null)
        {
            anim.SetInteger(HashWeaponType, (int)type); // cho Transition
            anim.SetFloat(HashWeaponTypeF, (int)type); // cho Blend Tree ← thêm
        }

        Debug.Log($"[WeaponManager] Đổi sang {type}");
    }

    IWeapon GetWeapon(WeaponType type)
    {
        return type switch
        {
            WeaponType.None => bareHandWeapon as IWeapon,
            WeaponType.Spear => spearWeapon as IWeapon,
            WeaponType.Gun => gunWeapon as IWeapon,
            _ => null
        };
    }
}