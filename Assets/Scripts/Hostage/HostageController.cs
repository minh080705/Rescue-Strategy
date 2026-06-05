// Assets/Scripts/Hostage/HostageController.cs

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthComponent))]
public class HostageController : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────
    [Header("Rescue")]
    public float   rescueRange = 1.5f;
    public KeyCode rescueKey   = KeyCode.E;

    [Header("Follow — khoảng cách")]
    [Tooltip("Gần hơn mức này → dừng lại, không áp sát player")]
    public float stopDistanceClose = 0.9f;

    [Tooltip("Xa hơn mức này → dừng lại, đứng chờ player quay lại đón")]
    public float stopDistanceFar   = 5f;

    [Tooltip("Khoảng cách bắt đầu chạy catch-up (phải < stopDistanceFar)")]
    public float runDistance       = 2.2f;

    [Header("Follow — tốc độ")]
    public float followSpeed       = 3.5f;
    public float catchUpMultiplier = 1.6f;

    [Header("Goal Zone")]
    [Tooltip("Kéo GoalZone GameObject vào đây, hoặc để trống sẽ tự tìm qua tag 'GoalZone'")]
    public Transform goalZone;
    public float     goalRadius = 1.2f;

    [Header("UI")]
    public GameObject rescuePromptUI;

    // ── State ────────────────────────────────────────────
    public HostageState State { get; private set; } = HostageState.Locked;

    // ── Private ──────────────────────────────────────────
    private Rigidbody2D     rb;
    private HealthComponent health;
    private Transform       playerTransform;

    // ── Unity lifecycle ──────────────────────────────────
    void Awake()
    {
        rb              = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints  = RigidbodyConstraints2D.FreezeRotation;

        health = GetComponent<HealthComponent>();
        health.onDeath.AddListener(OnHostageDeath);

        SetPrompt(false);
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;
        else Debug.LogError("[HostageController] Không tìm thấy tag 'Player'!");

        if (goalZone == null)
        {
            GameObject gz = GameObject.FindGameObjectWithTag("GoalZone");
            if (gz != null) goalZone = gz.transform;
            else Debug.LogWarning("[HostageController] Không tìm thấy GoalZone!");
        }
    }

    void Update()
    {
        if (!health.IsAlive || playerTransform == null) return;

        switch (State)
        {
            case HostageState.Locked:    UpdateLocked();    break;
            case HostageState.Following: UpdateFollowing(); break;
            case HostageState.Rescued:                      break;
        }
    }

    // ── Locked ───────────────────────────────────────────
    void UpdateLocked()
    {
        float dist    = Vector2.Distance(transform.position, playerTransform.position);
        bool  inRange = dist <= rescueRange;

        SetPrompt(inRange);

        if (inRange && Input.GetKeyDown(rescueKey))
        {
            State = HostageState.Following;
            SetPrompt(false);
            Debug.Log("[Hostage] Đã giải cứu! Con tin đang đi theo player.");
        }

        rb.velocity = Vector2.zero;
    }

    // ── Following ────────────────────────────────────────
    void UpdateFollowing()
    {
        // Kiểm tra về đích
        if (goalZone != null)
        {
            float distGoal = Vector2.Distance(transform.position, goalZone.position);
            if (distGoal <= goalRadius)
            {
                SetRescued();
                return;
            }
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist <= stopDistanceClose)
        {
            // Quá gần — đứng yên, không áp sát
            rb.velocity = Vector2.zero;
        }
        else if (dist >= stopDistanceFar)
        {
            // Quá xa — đứng yên, chờ player quay lại đón
            // (player phải đi ngược lại đến runDistance thì hostage mới bắt đầu chạy theo)
            rb.velocity = Vector2.zero;
        }
        else if (dist >= runDistance)
        {
            // Xa vừa — chạy catch-up
            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            rb.velocity = dir * (followSpeed * catchUpMultiplier);
        }
        else
        {
            // Khoảng cách bình thường — đi bộ
            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            rb.velocity = dir * followSpeed;
        }
    }

    // ── Về đích ──────────────────────────────────────────
    public void SetRescued()
    {
        if (State == HostageState.Rescued) return;
        State       = HostageState.Rescued;
        rb.velocity = Vector2.zero;
        SetPrompt(false);
        Debug.Log("[Hostage] Con tin an toàn! → WIN");
        GameManager.Instance?.TriggerWin();
    }

    // ── Hostage bị giết → Lose ───────────────────────────
    void OnHostageDeath(DamageSource source)
    {
        if (State == HostageState.Rescued) return;
        Debug.Log($"[Hostage] Bị {source} giết! → LOSE");
        rb.velocity = Vector2.zero;
        GameManager.Instance?.TriggerLose();
    }

    // ── Helper ───────────────────────────────────────────
    void SetPrompt(bool active)
    {
        if (rescuePromptUI != null) rescuePromptUI.SetActive(active);
    }

    // ── Gizmos ───────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        // Xanh lá = rescue range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rescueRange);

        // Vàng = stop close (không áp sát)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistanceClose);

        // Cam = run distance (bắt đầu catch-up)
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, runDistance);

        // Đỏ = stop far (quá xa, đứng chờ)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistanceFar);

        // Xanh lá = goal radius
        if (goalZone != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(goalZone.position, goalRadius);
        }
    }
}