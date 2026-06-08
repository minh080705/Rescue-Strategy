// Assets/Scripts/Enemy/EnemyAlertSystem.cs
// - Khi 1 enemy phát hiện player → broadcast cho cả nhóm chase
// - Khi ít nhất 1 enemy đang chase → cả nhóm dùng groupChaseRange thay detectionRange
// - Khi ít nhất 1 enemy đi quá leashRadius → cả nhóm quay về spawn point

using System.Collections.Generic;
using UnityEngine;

public class EnemyAlertSystem : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────
    public static EnemyAlertSystem Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────
    [Header("Alert")]
    [Tooltip("Detection range khi cả nhóm trong trạng thái alert")]
    public float alertDetectionRange = 10f;

    [Header("Leash")]
    [Tooltip("Nếu bất kỳ enemy nào đi xa spawn point hơn giá trị này → cả nhóm quay về")]
    public float leashRadius = 15f;
    [Tooltip("Khoảng cách đến spawn point thì dừng quay về")]
    public float returnStopRadius = 1f;

    // ── Internal ──────────────────────────────────────────
    private List<EnemyController> allEnemies = new List<EnemyController>();

    public bool IsAlerted { get; private set; } = false;
    public bool IsReturning { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        UpdateGroupState();
    }

    // ── Register / Unregister ─────────────────────────────
    public void Register(EnemyController enemy)
    {
        if (!allEnemies.Contains(enemy))
            allEnemies.Add(enemy);
    }

    public void Unregister(EnemyController enemy)
    {
        allEnemies.Remove(enemy);
    }

    // ── Group state ───────────────────────────────────────
    void UpdateGroupState()
    {
        bool anyChasing = false;
        bool anyLeashing = false;

        foreach (EnemyController e in allEnemies)
        {
            if (e == null || !e.gameObject.activeInHierarchy) continue;
            if (e.State == EnemyController.EnemyState.Dead) continue;

            // Check có ai đang chase/attack không → alert cả nhóm
            if (e.State == EnemyController.EnemyState.Chase ||
                e.State == EnemyController.EnemyState.Attack)
                anyChasing = true;

            // Check có ai vượt leash không
            float distFromSpawn = Vector2.Distance(e.transform.position, e.SpawnPosition);
            if (distFromSpawn > leashRadius)
                anyLeashing = true;
        }

        // Cập nhật IsAlerted
        if (!IsAlerted && anyChasing)
        {
            IsAlerted = true;
            Debug.Log("[AlertSystem] Nhóm vào trạng thái Alert — detectionRange tăng lên " + alertDetectionRange);
        }
        else if (IsAlerted && !anyChasing && !IsReturning)
        {
            IsAlerted = false;
            Debug.Log("[AlertSystem] Nhóm hết alert");
        }

        // Cập nhật IsReturning
        if (anyLeashing && !IsReturning)
        {
            IsReturning = true;
            ForceReturnAll();
            Debug.Log("[AlertSystem] Leash triggered — cả nhóm quay về spawn");
        }

        // Reset IsReturning khi tất cả đã về gần spawn
        if (IsReturning && AllNearSpawn())
        {
            IsReturning = false;
            
            Debug.Log("[AlertSystem] Cả nhóm đã về spawn");
        }
    }

    bool AllNearSpawn()
    {
        foreach (EnemyController e in allEnemies)
        {
            if (e == null || !e.gameObject.activeInHierarchy) continue;
            if (e.State == EnemyController.EnemyState.Dead) continue;

            if (Vector2.Distance(e.transform.position, e.SpawnPosition) > returnStopRadius)
                return false;
        }
        return true;
    }

    // ── Alert broadcast ───────────────────────────────────
    public void Alert(Vector2 sourcePosition, float alertRadius)
    {
        foreach (EnemyController e in allEnemies)
        {
            if (e == null || !e.gameObject.activeInHierarchy) continue;
            if (e.State == EnemyController.EnemyState.Dead) continue;

            float dist = Vector2.Distance(sourcePosition, e.transform.position);
            if (dist <= alertRadius)
                e.ForceChase();
        }
    }

    // ── Force return all ──────────────────────────────────
    void ForceReturnAll()
    {
        foreach (EnemyController e in allEnemies)
        {
            if (e == null || !e.gameObject.activeInHierarchy) continue;
            if (e.State == EnemyController.EnemyState.Dead) continue;

            e.ForceReturn();
        }
    }

    // ── Gizmos ────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (allEnemies == null) return;
        foreach (EnemyController e in allEnemies)
        {
            if (e == null) continue;
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.1f);
            Gizmos.DrawWireSphere(e.SpawnPosition, leashRadius);
        }
    }
}