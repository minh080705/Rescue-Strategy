// Assets/Scripts/Hostage/HostageAI.cs
// AI cho Hostage — Follow player hoặc Attack enemy tùy tình huống.
// Tái sử dụng Player2Animator, WeaponManager, SpearWeapon, GunWeapon.
// Dùng A* Pathfinding để di chuyển.
//
// HostageState.cs đã bị xoá — enum được gộp thẳng vào file này.

using UnityEngine;
using UnityEngine.UI;
using Pathfinding;

public enum HostageState
{
    Locked,     // Bị nhốt, chờ player giải cứu
    Following,  // Đang đi theo player
    Attack,     // Sub-state của Following — đang tấn công enemy
    Rescued     // Đã về điểm đích an toàn
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(WeaponManager))]
[RequireComponent(typeof(Player2Animator))]
[RequireComponent(typeof(Seeker))]
public class HostageAI : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Rescue")]
    public float rescueRange = 1.5f;
    public KeyCode rescueKey = KeyCode.E;
    public GameObject rescuePromptUI;
    public Button rescueButton;

    [Header("Goal Zone")]
    public Transform goalZone;
    public float goalRadius = 1.2f;

    [Header("Follow")]
    [Tooltip("Xa hơn giá trị này → ưu tiên follow player")]
    public float followThreshold = 4f;
    [Tooltip("Khoảng cách dừng lại khi đã đủ gần player")]
    public float followStopDist = 1.5f;
    public float moveSpeed = 4f;

    [Header("Attack")]
    [Tooltip("Tầm phát hiện enemy để tấn công")]
    public float attackDetectionRange = 6f;
    public string enemyTag = "Enemy";

    [Header("Pathfinding")]
    public float repathRate = 0.5f;
    public float waypointDist = 0.4f;

    // ── Internal ──────────────────────────────────────────
    private Rigidbody2D rb;
    private HealthComponent health;
    private WeaponManager weaponManager;
    private Player2Animator p2anim;
    private Seeker seeker;
    private GunWeapon gunWeapon;
    private SpearWeapon spearWeapon;

    private Transform playerTransform;

    // ── State (Locked / Following / Attack / Rescued) ─────
    public HostageState HostageState { get; private set; } = HostageState.Locked;

    // Pathfinding
    private Pathfinding.Path currentPath;
    private int waypointIndex = 0;
    private float repathTimer = 0f;
    private bool isPathReady = false;
    private Transform pathTarget;   // player hoặc enemy

    // Attack
    private Transform currentEnemy;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        health = GetComponent<HealthComponent>();
        weaponManager = GetComponent<WeaponManager>();
        p2anim = GetComponent<Player2Animator>();
        seeker = GetComponent<Seeker>();
        gunWeapon = GetComponent<GunWeapon>();
        spearWeapon = GetComponentInChildren<SpearWeapon>();

        health.onDeath.AddListener(OnDeath);

        // Wire up rescue button
        rescueButton?.onClick.AddListener(OnRescueButtonPressed);
        rescueButton?.gameObject.SetActive(false);

        // Lắng nghe inventory để tự đổi weapon
        if (SharedWeaponInventory.Instance != null)
            SharedWeaponInventory.Instance.onHostageWeaponChanged.AddListener(OnWeaponAssigned);
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;

        if (goalZone == null)
        {
            GameObject gz = GameObject.FindGameObjectWithTag("GoalZone");
            if (gz != null) goalZone = gz.transform;
        }

        SetPrompt(false);
    }

    void OnDestroy()
    {
        if (SharedWeaponInventory.Instance != null)
            SharedWeaponInventory.Instance.onHostageWeaponChanged.RemoveListener(OnWeaponAssigned);
    }

    void Update()
    {
        if (!health.IsAlive) return;

        switch (HostageState)
        {
            case HostageState.Locked:
                UpdateLocked();
                return; // không chạy AI khi locked

            case HostageState.Following:
            case HostageState.Attack:
                UpdateState();
                UpdatePathfinding();
                UpdateAnimator();
                CheckGoalZone();
                break;

            case HostageState.Rescued:
                rb.velocity = Vector2.zero;
                return;
        }
    }

    void FixedUpdate()
    {
        if (!health.IsAlive) { rb.velocity = Vector2.zero; return; }

        // Chỉ di chuyển khi đang Following hoặc Attack (2 sub-state sau khi được giải cứu)
        if (HostageState != HostageState.Following && HostageState != HostageState.Attack)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        FollowPath();
    }

    private int lastPhysicsLayer = -1;

    void LateUpdate()
    {
        if (gameObject.layer == lastPhysicsLayer) return;

        lastPhysicsLayer = gameObject.layer;

        // Sync physics layer cho tất cả children
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = gameObject.layer;
    }

    // ── Locked ───────────────────────────────────────────
    void UpdateLocked()
    {
        rb.velocity = Vector2.zero;
        p2anim.IsMoving = false;

        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= rescueRange;
        SetPrompt(inRange);

        if (inRange && Input.GetKeyDown(rescueKey))
            Rescue();
    }

    // ── Rescue button callback ────────────────────────────
    void OnRescueButtonPressed()
    {
        if (HostageState != HostageState.Locked || playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist > rescueRange) return;

        Rescue();
    }

    // ── Rescue logic (dùng chung cho phím E và nút UI) ───
    void Rescue()
    {
        SetState(HostageState.Following);
        SetPrompt(false);
        Debug.Log("[HostageAI] Đã giải cứu! Hostage đang follow player.");
    }

    // ── Check Goal Zone ───────────────────────────────────
    void CheckGoalZone()
    {
        if (goalZone == null) return;
        float dist = Vector2.Distance(transform.position, goalZone.position);
        if (dist <= goalRadius)
        {
            SetState(HostageState.Rescued);
            rb.velocity = Vector2.zero;
            SetPrompt(false);
            Debug.Log("[HostageAI] Hostage an toàn! → WIN");
            GameManager.Instance?.TriggerWin();
        }
    }

    // ── Prompt UI ─────────────────────────────────────────
    void SetPrompt(bool active)
    {
        if (rescuePromptUI != null)
            rescuePromptUI.SetActive(active);

        rescueButton?.gameObject.SetActive(active);      // ← nút hiện/ẩn theo rescuePromptUI
    }

    // ── State machine (Following ⇄ Attack) ────────────────
    void UpdateState()
    {
        WeaponType weapon = weaponManager.CurrentType;

        // BareHand → chỉ Follow
        if (weapon == WeaponType.None)
        {
            StopAttack();
            SetState(HostageState.Following);
            return;
        }

        // Xa player quá → Follow
        if (playerTransform != null)
        {
            float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distToPlayer > followThreshold)
            {
                StopAttack();
                SetState(HostageState.Following);
                return;
            }
        }

        // Tìm enemy gần nhất
        currentEnemy = FindClosestEnemy();

        if (currentEnemy != null)
            SetState(HostageState.Attack);
        else
            SetState(HostageState.Following);
    }

    void SetState(HostageState newState)
    {
        if (HostageState == newState) return;
        HostageState = newState;

        if (newState == HostageState.Following)
            StopAttack();
    }

    // ── Attack ────────────────────────────────────────────
    void ExecuteAttack()
    {
        if (currentEnemy == null) return;

        Vector2 dir = ((Vector2)currentEnemy.position - (Vector2)transform.position).normalized;
        UpdateFacing(dir);

        switch (weaponManager.CurrentType)
        {
            case WeaponType.Spear:
                spearWeapon?.OnAttackHeld(dir);
                break;

            case WeaponType.Gun:
                p2anim.IsFiring = true;
                gunWeapon.FacingOverride = dir;//
                gunWeapon?.OnAttackHeld(dir);
                break;
        }
    }

    void StopAttack()
    {
        p2anim.IsFiring = false;

        Vector2 dir = currentEnemy != null
            ? ((Vector2)currentEnemy.position - (Vector2)transform.position).normalized
            : Vector2.down;

        spearWeapon?.OnAttackReleased(dir);
        gunWeapon?.OnAttackReleased(dir);
        currentEnemy = null;
    }

    // ── Pathfinding ───────────────────────────────────────
    void UpdatePathfinding()
    {
        // Chọn target theo state
        pathTarget = HostageState == HostageState.Attack && currentEnemy != null
            ? currentEnemy
            : playerTransform;

        if (pathTarget == null) return;

        // Attack bằng Gun — đứng yên bắn nếu đủ gần
        if (HostageState == HostageState.Attack && weaponManager.CurrentType == WeaponType.Gun)
        {
            float distToEnemy = Vector2.Distance(transform.position, currentEnemy.position);
            if (distToEnemy <= attackDetectionRange * 0.6f)
            {
                isPathReady = false; // dừng di chuyển
                ExecuteAttack();
                return;
            }
        }

        // Attack bằng Spear — tiến vào gần
        if (HostageState == HostageState.Attack)
            ExecuteAttack();

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathRate;
            RequestPath();
        }
    }

    void RequestPath()
    {
        if (pathTarget == null) return;

        float dist = Vector2.Distance(transform.position, pathTarget.position);

        if (HostageState == HostageState.Following && dist <= followStopDist)
        {
            isPathReady = false;
            return;
        }

        int graphMask = GetGraphMask();
        seeker.StartPath(transform.position, pathTarget.position, OnPathComplete, graphMask);
    }

    int GetGraphMask()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Layer 1")) return 1 << 0;
        if (gameObject.layer == LayerMask.NameToLayer("Layer 2")) return 1 << 1;
        return -1;
    }

    void OnPathComplete(Pathfinding.Path p)
    {
        if (p.error) return;
        currentPath = p;
        waypointIndex = 0;
        isPathReady = true;
    }

    void FollowPath()
    {
        if (!isPathReady || currentPath == null)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (waypointIndex >= currentPath.vectorPath.Count)
        {
            isPathReady = false;
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 waypoint = currentPath.vectorPath[waypointIndex];
        Vector2 dir = (waypoint - (Vector2)transform.position).normalized;
        float dist = Vector2.Distance(transform.position, waypoint);

        if (dist <= waypointDist)
        {
            waypointIndex++;
            return;
        }

        rb.velocity = dir * moveSpeed;
        UpdateFacing(dir);
    }

    // ── Facing ────────────────────────────────────────────
    void UpdateFacing(Vector2 dir)
    {
        p2anim.FacingIndex = Player2Controller.ToIndex(dir);
    }

    // ── Animator ──────────────────────────────────────────
    void UpdateAnimator()
    {
        p2anim.IsMoving = rb.velocity.magnitude > 0.1f;
        p2anim.IsDashing = false;
    }

    // ── Find enemy ────────────────────────────────────────
    Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        Transform closest = null;
        float minDist = attackDetectionRange;

        foreach (GameObject e in enemies)
        {
            // Chỉ tấn công enemy cùng physics layer
            if (e.layer != gameObject.layer) continue;

            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = e.transform;
            }
        }

        return closest;
    }

    // ── Inventory callback ────────────────────────────────
    void OnWeaponAssigned(WeaponType type)
    {
        weaponManager.EquipWeapon(type);
        StopAttack();
        Debug.Log($"[HostageAI] Weapon → {type}");
    }

    // ── Death ─────────────────────────────────────────────
    void OnDeath(DamageSource source)
    {
        if (HostageState == HostageState.Rescued) return;
        StopAttack();
        rb.velocity = Vector2.zero;
        p2anim.SetDead(true);
        Debug.Log($"[HostageAI] Bị {source} giết! → LOSE");
        GameManager.Instance?.TriggerLose();
    }
}