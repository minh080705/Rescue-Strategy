//// Assets/Scripts/Player/PlayerCombat.cs
//// Xử lý input tấn công của Player.
//// Bật hitbox đúng thời điểm, tắt sau attackDuration giây.
//// Hoàn toàn tách biệt với PlayerController (single responsibility).

//using UnityEngine;

//[RequireComponent(typeof(PlayerController))]
//[RequireComponent(typeof(HealthComponent))]
//public class PlayerCombatCopy : MonoBehaviour
//{
//    // ── Inspector ────────────────────────────────────────
//    [Header("Attack")]
//    public KeyCode attackKey = KeyCode.Space;
//    [Tooltip("Hitbox bật trong bao nhiêu giây")]
//    public float attackDuration = 0.2f;
//    [Tooltip("Thời gian chờ giữa 2 lần attack")]
//    public float attackCooldown = 0.5f;

//    [Header("References")]
//    [Tooltip("Child GameObject chứa HitboxComponent")]
//    public HitboxComponent hitbox;

//    // ── Private ──────────────────────────────────────────
//    private PlayerController controller;
//    private HealthComponent health;
//    private float attackTimer = 0f;  // đếm cooldown
//    private float hitboxTimer = 0f;  // đếm thời gian hitbox bật
//    private bool isAttacking = false;

//    // ── Unity lifecycle ──────────────────────────────────
//    void Awake()
//    {
//        controller = GetComponent<PlayerController>();
//        health = GetComponent<HealthComponent>();

//        // Đăng ký xử lý khi player chết
//        health.onDeath.AddListener(OnPlayerDeath);
//    }

//    void Update()
//    {
//        if (!health.IsAlive) return;

//        HandleAttackInput();
//        TickHitbox();
//    }

//    // ── Input ────────────────────────────────────────────
//    void HandleAttackInput()
//    {
//        attackTimer -= Time.deltaTime;

//        if (Input.GetKeyDown(attackKey) && attackTimer <= 0f && !isAttacking)
//        {
//            StartAttack();
//        }
//    }

//    // ── Attack lifecycle ─────────────────────────────────
//    void StartAttack()
//    {
//        isAttacking = true;
//        hitboxTimer = attackDuration;
//        attackTimer = attackCooldown;

//        // Đặt hitbox đúng hướng player đang nhìn
//        PositionHitbox(controller.LastFacing);
//        hitbox.SetActive(true);

//        // Khoá di chuyển trong lúc attack (tuỳ chọn — bỏ comment nếu muốn)
//        // controller.SetInputLocked(true);
//    }

//    void TickHitbox()
//    {
//        if (!isAttacking) return;

//        hitboxTimer -= Time.deltaTime;
//        if (hitboxTimer <= 0f)
//        {
//            hitbox.SetActive(false);
//            isAttacking = false;
//            // controller.SetInputLocked(false);
//        }
//    }

//    // ── Đặt hitbox theo hướng facing ─────────────────────
//    // Offset: hitbox xuất hiện phía trước player
//    void PositionHitbox(Vector2 facing)
//    {
//        float offset = 0.6f; // khoảng cách từ tâm player đến hitbox
//        hitbox.transform.localPosition = facing * offset;
//    }

//    // ── Player chết ──────────────────────────────────────
//    void OnPlayerDeath(DamageSource source)
//    {
//        Debug.Log($"[PlayerCombat] Player bị {source} giết! → LOSE");
//        hitbox.SetActive(false);
//        GameManager.Instance?.TriggerLose();
//    }
//}