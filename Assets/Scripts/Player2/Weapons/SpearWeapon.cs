// Assets/Scripts/Player2/Weapons/SpearWeapon.cs
// Tấn công bằng thương — hitbox 8 hướng, giữ phím để đâm liên tục.
// Hitbox position theo LastFacing, có cooldown giữa các đòn.

using UnityEngine;

public class SpearWeapon : MonoBehaviour, IWeapon
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Attack")]
    public float attackCooldown = 0.4f;
    public float hitboxDuration = 0.15f;   // hitbox bật bao lâu mỗi đòn

    [Header("References")]
    public HitboxComponent hitbox;

    // ── IWeapon ───────────────────────────────────────────
    public WeaponType Type => WeaponType.Spear;
    public bool CanAttack => !isOnCooldown;

    // ── Internal ──────────────────────────────────────────
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private float hitboxTimer = 0f;
    private bool isAttackHeld = false;

    private Animator anim;
    private static readonly int HashIsAttacking = Animator.StringToHash("IsAttacking");

    // ── Hitbox offsets 8 hướng ────────────────────────────
    // Thứ tự theo FacingIndex: 0=Down 1=Up 2=Left 3=Right
    //                          4=RightDown 5=RightUp 6=LeftDown 7=LeftUp
    private static readonly Vector2[] HitboxOffsets = new Vector2[]
    {
        new Vector2( 0f,   -1.0f),  // 0 Down
        new Vector2( 0f,    1.0f),  // 1 Up
        new Vector2(-1.0f,  0.5f),  // 2 Left
        new Vector2( 1.0f,  0.5f),  // 3 Right
        new Vector2( 0.8f, -0.8f),  // 4 RightDown
        new Vector2( 0.8f,  0.8f),  // 5 RightUp
        new Vector2(-0.8f, -0.8f),  // 6 LeftDown
        new Vector2(-0.8f,  0.8f),  // 7 LeftUp
    };

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        anim = GetComponent<Animator>();
        hitbox?.SetActive(false);
    }

    void Update()
    {
        TickCooldown();
        TickHitbox();

        // Giữ phím → tự động đâm lại khi hết cooldown
        if (isAttackHeld && !isOnCooldown)
            TriggerAttack(GetCurrentFacing());
    }

    // ── IWeapon impl ──────────────────────────────────────
    public void OnEquip()
    {
        hitbox?.SetActive(false);
        cooldownTimer = 0f;
        isOnCooldown = false;
    }

    public void OnUnequip()
    {
        isAttackHeld = false;
        hitbox?.SetActive(false);
        anim?.SetBool(HashIsAttacking, false);
    }

    public void OnAttackHeld(Vector2 facing)
    {
        isAttackHeld = true;
        if (!isOnCooldown)
            TriggerAttack(facing);
    }

    public void OnAttackReleased(Vector2 facing)
    {
        isAttackHeld = false;
    }

    public void OnReload() { } // Thương không reload

    // ── Attack logic ──────────────────────────────────────
    void TriggerAttack(Vector2 facing)
    {
        int index = Player2Controller.ToIndex(facing);

        PositionHitbox(index);
        hitbox?.SetActive(true);

        hitboxTimer = hitboxDuration;
        cooldownTimer = attackCooldown;
        isOnCooldown = true;

        anim?.SetBool(HashIsAttacking, true);
        anim?.SetTrigger(Animator.StringToHash("Attack"));

        Debug.Log($"[Spear] Đâm hướng {index} ({facing})");
    }

    // ── Tick ──────────────────────────────────────────────
    void TickCooldown()
    {
        if (!isOnCooldown) return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            isOnCooldown = false;
            anim?.SetBool(HashIsAttacking, false);
        }
    }

    void TickHitbox()
    {
        if (hitboxTimer <= 0f) return;

        hitboxTimer -= Time.deltaTime;
        if (hitboxTimer <= 0f)
            hitbox?.SetActive(false);
    }

    // ── Helpers ───────────────────────────────────────────
    void PositionHitbox(int facingIndex)
    {
        if (hitbox == null) return;
        int i = Mathf.Clamp(facingIndex, 0, HitboxOffsets.Length - 1);
        hitbox.transform.localPosition = HitboxOffsets[i];
    }

    Vector2 GetCurrentFacing()
    {
        var controller = GetComponent<Player2Controller>();
        return controller != null ? controller.LastFacing : Vector2.down;
    }
}