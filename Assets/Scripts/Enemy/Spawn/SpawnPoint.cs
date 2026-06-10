// Assets/Scripts/Enemy/SpawnPoint.cs
// Đặt rải rác trên map. Mỗi SpawnPoint tự quản lý logic spawn.
//
// Immediate  — spawn ngay khi scene load
// Proximity  — spawn 1 lần khi player vào TriggerRadius
//
// Mỗi SpawnPoint có danh sách EnemyEntry:
//   - Prefab enemy muốn spawn
//   - Số lượng
//   - Phase: Normal (đi tới) hoặc Return (quay về với con tin)


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    // ── Enum ──────────────────────────────────────────────
    public enum SpawnType { Immediate, Proximity }
    public enum SpawnPhase { Normal, Return, Both }
    public enum SpawnFormation { Random, Grid, Circle }

    // ── Sub-class ─────────────────────────────────────────
    [System.Serializable]
    public class EnemyEntry
    {
        public GameObject prefab;
        public int count = 1;
        public SpawnPhase phase = SpawnPhase.Normal;
    }

    // ── Inspector ─────────────────────────────────────────
    [Header("Type")]
    public SpawnType spawnType = SpawnType.Proximity;

    [Header("Enemies")]
    public List<EnemyEntry> enemies = new List<EnemyEntry>();

    [Header("Proximity Settings")]
    [Tooltip("Khoảng cách player vào để trigger spawn (chỉ dùng khi Proximity)")]
    public float triggerRadius = 5f;

    [Header("Formation")]
    public SpawnFormation formation = SpawnFormation.Grid;
    [Tooltip("Khoảng cách giữa các enemy trong lưới / vòng tròn")]
    public float spacing = 1.2f;
    [Tooltip("Số cột trong lưới (chỉ dùng khi Grid)")]
    public int gridColumns = 3;

    [Header("Spawn Delay")]
    [Tooltip("Delay giữa mỗi enemy spawn (giây)")]
    public float spawnDelay = 0.3f;

    // ── Internal ──────────────────────────────────────────
    private bool hasSpawnedNormal = false;
    private bool hasSpawnedReturn = false;
    private bool isReturning = false;

    private EnemyGroup enemyGroup;
    private Transform playerTransform;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        EnemySpawnManager.Instance?.Register(this);
    }

    private HostageController hostage;

    void Start()
    {
        // Cache player
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;

        // Tạo EnemyGroup cho nhóm này
        enemyGroup = gameObject.AddComponent<EnemyGroup>();

        // Cache hostage
        hostage = FindObjectOfType<HostageController>();

        // Immediate → spawn ngay
        if (spawnType == SpawnType.Immediate && !hasSpawnedNormal)
        {
            StartCoroutine(SpawnEnemies(SpawnPhase.Normal));
            hasSpawnedNormal = true;
        }
    }

    void Update()
    {
        // Detect hostage Following → chuyển sang Return phase
        if (!isReturning && hostage != null && hostage.State == HostageState.Following)
        {
            isReturning = true;
            Debug.Log($"[SpawnPoint] {gameObject.name} chuyển sang Return phase (Hostage Following)");
        }

        if (spawnType != SpawnType.Proximity) return;
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        // Normal phase
        if (!hasSpawnedNormal && !isReturning && dist <= triggerRadius)
        {
            hasSpawnedNormal = true;
            StartCoroutine(SpawnEnemies(SpawnPhase.Normal));
        }

        // Return phase
        if (!hasSpawnedReturn && isReturning && dist <= triggerRadius)
        {
            hasSpawnedReturn = true;
            StartCoroutine(SpawnEnemies(SpawnPhase.Return));
        }
    }

    // ── Spawn ─────────────────────────────────────────────
    IEnumerator SpawnEnemies(SpawnPhase currentPhase)
    {
        foreach (EnemyEntry entry in enemies)
        {
            if (entry.phase != currentPhase && entry.phase != SpawnPhase.Both)
                continue;

            if (entry.prefab == null) continue;

            List<Vector2> positions = GetFormationPositions(entry.count);

            for (int i = 0; i < entry.count; i++)
            {
                Vector2 spawnPos = positions[i];
                GameObject obj = EnemySpawnManager.Instance.Get(entry.prefab, spawnPos);

                // Gán vào group của SpawnPoint này
                EnemyController ec = obj.GetComponent<EnemyController>();
                if (ec != null)
                    ec.group = enemyGroup;

                SpawnPointReturnHelper helper = obj.GetComponent<SpawnPointReturnHelper>();
                if (helper == null)
                    helper = obj.AddComponent<SpawnPointReturnHelper>();
                helper.Init(entry.prefab);

                yield return new WaitForSeconds(spawnDelay);
            }
        }

        Debug.Log($"[SpawnPoint] {gameObject.name} spawned phase={currentPhase}");
    }

    // ── Formation positions ───────────────────────────────
    List<Vector2> GetFormationPositions(int count)
    {
        List<Vector2> positions = new List<Vector2>();
        Vector2 center = transform.position;

        switch (formation)
        {
            case SpawnFormation.Grid:
                int cols = Mathf.Max(1, gridColumns);
                for (int i = 0; i < count; i++)
                {
                    int col = i % cols;
                    int row = i / cols;

                    // Căn giữa lưới theo tâm
                    int totalCols = Mathf.Min(count, cols);
                    float offsetX = (col - (totalCols - 1) / 2f) * spacing;
                    float offsetY = -row * spacing;

                    positions.Add(center + new Vector2(offsetX, offsetY));
                }
                break;

            case SpawnFormation.Circle:
                for (int i = 0; i < count; i++)
                {
                    float angle = i * (360f / count) * Mathf.Deg2Rad;
                    float radius = spacing * count / (2f * Mathf.PI);
                    radius = Mathf.Max(radius, spacing);
                    Vector2 pos = center + new Vector2(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius
                    );
                    positions.Add(pos);
                }
                break;

            case SpawnFormation.Random:
            default:
                for (int i = 0; i < count; i++)
                {
                    Vector2 rnd = center + Random.insideUnitCircle * spacing;
                    positions.Add(rnd);
                }
                break;
        }

        return positions;
    }

    // ── Gizmos ────────────────────────────────────────────
    void OnDrawGizmos()
    {
        Gizmos.color = spawnType == SpawnType.Immediate
            ? new Color(1f, 0.5f, 0f, 0.4f)   // cam = Immediate
            : new Color(0f, 0.8f, 1f, 0.4f);  // xanh = Proximity

        Gizmos.DrawWireSphere(transform.position, 0.4f);

        if (spawnType == SpawnType.Proximity)
        {
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.15f);
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
    }
}