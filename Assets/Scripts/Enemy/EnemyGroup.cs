// Assets/Scripts/Enemy/EnemyGroup.cs
// Quản lý alert và leash cho 1 nhóm enemy từ 1 SpawnPoint.
// SpawnPoint tạo EnemyGroup khi spawn, gán vào từng enemy trong nhóm.

using System.Collections.Generic;
using UnityEngine;

public class EnemyGroup : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Alert")]
    public float alertDetectionRange = 10f;

    [Header("Leash")]
    public float leashRadius = 15f;
    public float returnStopRadius = 1f;

    // ── Properties ────────────────────────────────────────
    public bool IsAlerted { get; private set; } = false;
    public bool IsReturning { get; private set; } = false;

    // ── Internal ──────────────────────────────────────────
    private List<BaseEnemy> members = new List<BaseEnemy>();

    // ── Unity lifecycle ───────────────────────────────────
    void Update()
    {
        UpdateGroupState();
    }

    // ── Member management ─────────────────────────────────
    public void Register(BaseEnemy enemy)
    {
        if (!members.Contains(enemy))
            members.Add(enemy);
    }

    public void Unregister(BaseEnemy enemy)
    {
        members.Remove(enemy);
    }

    // ── Group state ───────────────────────────────────────
    void UpdateGroupState()
    {
        bool anyChasing = false;
        bool anyLeashing = false;

        foreach (BaseEnemy e in members)
        {
            if (e == null || !e.gameObject.activeInHierarchy) continue;
            if (!e.IsAlive) continue;

            if (e.IsChasing)
                anyChasing = true;

            if (Vector2.Distance(e.transform.position, e.SpawnPosition) > leashRadius)
                anyLeashing = true;
        }

        // Alert
        if (!IsAlerted && anyChasing)
        {
            IsAlerted = true;
            Debug.Log($"[EnemyGroup:{name}] Alert!");

            foreach (BaseEnemy e in members)
            {
                if (e == null || !e.gameObject.activeInHierarchy) continue;
                if (!e.IsChasing)
                    e.ForceChase();
            }
        }
        else if (IsAlerted && !anyChasing && !IsReturning)
        {
            IsAlerted = false;
            Debug.Log($"[EnemyGroup:{name}] Hết alert");
        }

        // Leash
        if (anyLeashing && !IsReturning)
        {
            IsReturning = true;
            Debug.Log($"[EnemyGroup:{name}] Leash triggered");

            foreach (BaseEnemy e in members)
            {
                if (e == null || !e.gameObject.activeInHierarchy) continue;
                if (e.IsAlive)
                    e.ForceReturn();
            }
        }

        if (IsReturning && AllNearSpawn())
        {
            IsReturning = false;
            IsAlerted = false;
            Debug.Log($"[EnemyGroup:{name}] Cả nhóm đã về spawn");
        }
    }

    bool AllNearSpawn()
    {
        foreach (BaseEnemy e in members)
        {
            if (e == null || !e.gameObject.activeInHierarchy) continue;
            if (!e.IsAlive) continue;
            if (Vector2.Distance(e.transform.position, e.SpawnPosition) > returnStopRadius)
                return false;
        }
        return true;
    }

    // ── Gizmos ────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (members == null) return;
        foreach (BaseEnemy e in members)
        {
            if (e == null) continue;
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.15f);
            Gizmos.DrawWireSphere(e.SpawnPosition, leashRadius);
        }
    }
}