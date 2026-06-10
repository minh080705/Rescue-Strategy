// Assets/Scripts/Enemy/EnemyPathfinder.cs
// Component riêng xử lý pathfinding bằng A*.
// Attach cùng GameObject với EnemyController.
// EnemyController vẫn giữ nguyên — EnemyPathfinder override velocity khi cần.

using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(Seeker))]
public class EnemyPathfinder : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Pathfinding")]
    [Tooltip("Tần suất tính lại đường (giây) — thấp hơn = mượt hơn nhưng nặng hơn")]
    public float repathRate = 0.5f;
    [Tooltip("Khoảng cách đến waypoint tiếp theo thì chuyển sang waypoint kế")]
    public float waypointDist = 0.5f;
    [Tooltip("Khoảng cách đến target thì dừng tính path")]
    public float stopDist = 1.5f;

    // ── Internal ──────────────────────────────────────────
    private EnemyController controller;
    private Seeker seeker;
    private Rigidbody2D rb;

    private Pathfinding.Path currentPath;
    private int waypointIndex = 0;
    private float repathTimer = 0f;
    private bool isPathReady = false;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        controller = GetComponent<EnemyController>();
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Chỉ pathfind khi đang Chase
        if (controller.State != EnemyController.EnemyState.Chase)
        {
            isPathReady = false;
            return;
        }

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathRate;
            RequestPath();
        }
    }

    void FixedUpdate()
    {
        // Chỉ override velocity khi Chase và có path
        if (controller.State != EnemyController.EnemyState.Chase) return;
        if (!isPathReady || currentPath == null) return;

        FollowPath();
    }

    // ── Pathfinding ───────────────────────────────────────
    void RequestPath()
    {
        Transform target = controller.GetCurrentTarget();
        if (target == null) return;

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist <= stopDist) return; // đủ gần rồi, không cần path

        seeker.StartPath(transform.position, target.position, OnPathComplete);
    }

    void OnPathComplete(Pathfinding.Path p)
    {
        if (p.error)
        {
            Debug.LogWarning($"[EnemyPathfinder] Path error: {p.errorLog}");
            return;
        }

        currentPath = p;
        waypointIndex = 0;
        isPathReady = true;
    }

    void FollowPath()
    {
        if (waypointIndex >= currentPath.vectorPath.Count)
        {
            isPathReady = false;
            return;
        }

        Vector2 waypoint = currentPath.vectorPath[waypointIndex];
        Vector2 dir = (waypoint - (Vector2)transform.position).normalized;
        float dist = Vector2.Distance(transform.position, waypoint);

        // Đến waypoint → chuyển sang waypoint tiếp theo
        if (dist <= waypointDist)
        {
            waypointIndex++;
            return;
        }

        // Override velocity từ EnemyController
        rb.velocity = dir * controller.moveSpeed;
    }
}