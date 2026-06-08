// Assets/Scripts/Player2/Weapons/GunWeapon.cs
// Bắn liên khi giữ phím, có ammo và reload.
// Tự động reload khi hết đạn.
// Reload có thể thủ công bằng phím R.

using UnityEngine;
using UnityEngine.Events;

public class GunWeapon : MonoBehaviour, IWeapon
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Ammo")]
    public int maxAmmo = 10;
    public float reloadDuration = 0.7f;

    [Header("Fire")]
    public float fireRate = 0.15f;   // giây giữa 2 phát
    public KeyCode reloadKey = KeyCode.R;

    [Header("Spawn Point")]
    [Tooltip("Transform điểm spawn đạn — đặt ở đầu nòng súng")]
    [Header("Muzzle Points")]
    public Transform[] muzzlePoints; // gán đủ 8 theo thứ tự 0-7

    

    [Header("References")]
    public BulletPool bulletPool;

    [Header("Events — UI lắng nghe")]
    public UnityEvent<int, int> onAmmoChanged;   // (current, max)
    public UnityEvent onReloadStart;
    public UnityEvent onReloadEnd;

    // ── IWeapon ───────────────────────────────────────────
    public WeaponType Type => WeaponType.Gun;
    public bool CanAttack => currentAmmo > 0 && !IsReloading;

    // ── Properties ────────────────────────────────────────
    public int CurrentAmmo { get; private set; }
    public bool IsReloading { get; private set; }

    // ── Internal ──────────────────────────────────────────
    private bool isFireHeld = false;
    private float fireTimer = 0f;
    private float reloadTimer = 0f;
    private bool isOnCooldown = false;

    private Animator anim;
    private Player2Controller controller;

    private static readonly int HashIsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int HashIsReloading = Animator.StringToHash("IsReloading");

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<Player2Controller>();
        CurrentAmmo = maxAmmo;
    }

    void Update()
    {
        if (!IsReloading && Input.GetKeyDown(reloadKey))
        {
            if (CurrentAmmo < maxAmmo)
                StartReload();
        }

        TickReload();
        TickFire();
    }

    // ── IWeapon impl ──────────────────────────────────────
    public void OnEquip()
    {
        isFireHeld = false;
        NotifyAmmoUI();
    }

    public void OnUnequip()
    {
        isFireHeld = false;
        anim?.SetBool(HashIsAttacking, false);
        anim?.SetBool(HashIsReloading, false);
    }

    public void OnAttackHeld(Vector2 facing)
    {
        if (IsReloading) return;
        isFireHeld = true;
    }

    public void OnAttackReleased(Vector2 facing)
    {
        isFireHeld = false;
        anim?.SetBool(HashIsAttacking, false);
    }

    public void OnReload()
    {
        if (!IsReloading && CurrentAmmo < maxAmmo)
            StartReload();
    }

    // ── Fire ──────────────────────────────────────────────
    void TickFire()
    {
        fireTimer -= Time.deltaTime;

        if (!isFireHeld || IsReloading) return;
        if (fireTimer > 0f) return;

        if (currentAmmo <= 0)
        {
            StartReload();
            return;
        }

        FireBullet();
        fireTimer = fireRate;
    }

    Transform GetMuzzle()
    {
        int i = Mathf.Clamp(controller.FacingIndex, 0, muzzlePoints.Length - 1);
        return muzzlePoints[i];
    }

    void FireBullet()
    {
        if (bulletPool == null) return;

        Vector2 facing = controller != null ? controller.LastFacing : Vector2.down;
        Transform muzzle = GetMuzzle();
        Vector2 spawnPos = muzzle != null
            ? (Vector2)muzzle.position
            : (Vector2)transform.position;

        Bullet bullet = bulletPool.Get(spawnPos, facing, DamageSource.Player);
        bullet.gameObject.layer = gameObject.layer;
        SpriteRenderer playerSR = GetComponent<SpriteRenderer>();
        SpriteRenderer bulletSR = bullet.GetComponent<SpriteRenderer>();
        if (playerSR != null && bulletSR != null)
        {
            bulletSR.sortingLayerID = playerSR.sortingLayerID;
            bulletSR.sortingOrder = playerSR.sortingOrder;
        }
        currentAmmo--;
        NotifyAmmoUI();
        anim?.SetBool(HashIsAttacking, true);

        Debug.Log($"[Gun] Bắn → {facing} | Đạn còn: {currentAmmo}/{maxAmmo}");
    }

    // ── Reload ────────────────────────────────────────────
    void StartReload()
    {
        if (IsReloading) return;

        IsReloading = true;
        reloadTimer = reloadDuration;
        isFireHeld = false;

        anim?.SetBool(HashIsAttacking, false);
        anim?.SetBool(HashIsReloading, true);

        onReloadStart?.Invoke();
        Debug.Log("[Gun] Bắt đầu reload...");
    }

    void TickReload()
    {
        if (!IsReloading) return;

        reloadTimer -= Time.deltaTime;
        if (reloadTimer <= 0f)
            EndReload();
    }

    void EndReload()
    {
        IsReloading = false;
        currentAmmo = maxAmmo;

        anim?.SetBool(HashIsReloading, false);

        NotifyAmmoUI();
        onReloadEnd?.Invoke();
        Debug.Log("[Gun] Reload xong!");
    }

    // ── UI ────────────────────────────────────────────────
    void NotifyAmmoUI()
    {
        onAmmoChanged?.Invoke(currentAmmo, maxAmmo);
    }

    // fix typo — dùng property thống nhất
    private int currentAmmo
    {
        get => CurrentAmmo;
        set => CurrentAmmo = value;
    }
}