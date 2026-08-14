// Assets/Scripts/Enemy/SpawnPoint.cs
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

        [Header("Layer Settings")]
        [Tooltip("Physics Layer (tên phải khớp Project Settings > Tags and Layers)")]
        public string physicsLayer = "Default";

        [Tooltip("Sorting Layer (tên phải khớp Project Settings > Tags and Layers)")]
        public string sortingLayerName = "Default";

        [Tooltip("Order in Layer (số nhỏ = vẽ phía sau)")]
        public int orderInLayer = 0;
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
    private HostageAI hostage;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        EnemySpawnManager.Instance?.Register(this);
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;

        enemyGroup = gameObject.AddComponent<EnemyGroup>();
        hostage = FindObjectOfType<HostageAI>();

        if (spawnType == SpawnType.Immediate && !hasSpawnedNormal)
        {
            StartCoroutine(SpawnEnemies(SpawnPhase.Normal));
            hasSpawnedNormal = true;
        }
    }
    
    void Update()
    {
        if (!isReturning && hostage != null && hostage.HostageState == HostageState.Following)
        {
            isReturning = true;
            Debug.Log($"[SpawnPoint] {gameObject.name} chuyển sang Return phase (Hostage Following)");
        }

        if (spawnType != SpawnType.Proximity) return;
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (!hasSpawnedNormal && !isReturning && dist <= triggerRadius)
        {
            hasSpawnedNormal = true;
            StartCoroutine(SpawnEnemies(SpawnPhase.Normal));
        }

        if (!hasSpawnedReturn && isReturning && dist <= triggerRadius)
        {
            hasSpawnedReturn = true;
            StartCoroutine(SpawnEnemies(SpawnPhase.Return));
        }
    }

    // ── Spawn ────────────────────────────────────────────
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
                GameObject obj = EnemySpawnManager.Instance.Get(entry.prefab, positions[i]);

                ApplyLayerSettings(obj, entry);

                BaseEnemy enemy = obj.GetComponent<BaseEnemy>();
                if (enemy != null)
                    enemy.group = enemyGroup;

                SpawnPointReturnHelper helper = obj.GetComponent<SpawnPointReturnHelper>();
                if (helper == null)
                    helper = obj.AddComponent<SpawnPointReturnHelper>();
                helper.Init(entry.prefab);

                yield return new WaitForSeconds(spawnDelay);
            }
        }

        Debug.Log($"[SpawnPoint] {gameObject.name} spawned phase={currentPhase}");
    }

    // ── Áp dụng toàn bộ layer settings sau khi spawn ─────
    void ApplyLayerSettings(GameObject obj, EnemyEntry entry)
    {
        // 1) Physics Layer — áp dụng đệ quy cho toàn bộ cây
        int physicsLayerIndex = LayerMask.NameToLayer(entry.physicsLayer);
        if (physicsLayerIndex == -1)
            Debug.LogWarning($"[SpawnPoint] Physics layer '{entry.physicsLayer}' không tồn tại!");
        else
            SetPhysicsLayerRecursive(obj, physicsLayerIndex);

        // 2) Sorting Layer + Order in Layer — áp dụng cho tất cả SpriteRenderer
        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"[SpawnPoint] {obj.name} không có SpriteRenderer nào!");
            return;
        }

        foreach (SpriteRenderer sr in renderers)
        {
            sr.sortingLayerName = entry.sortingLayerName;
            sr.sortingOrder = entry.orderInLayer;
        }

        Debug.Log($"[SpawnPoint] {obj.name} → " +
                  $"PhysicsLayer='{entry.physicsLayer}' | " +
                  $"SortingLayer='{entry.sortingLayerName}' | " +
                  $"Order={entry.orderInLayer} " +
                  $"({renderers.Length} renderer)");
    }

    void SetPhysicsLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetPhysicsLayerRecursive(child.gameObject, layer);
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
                    float radius = Mathf.Max(spacing * count / (2f * Mathf.PI), spacing);
                    positions.Add(center + new Vector2(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius));
                }
                break;

            default:
                for (int i = 0; i < count; i++)
                    positions.Add(center + Random.insideUnitCircle * spacing);
                break;
        }

        return positions;
    }

    // ── Gizmos ────────────────────────────────────────────
    void OnDrawGizmos()
    {
        Gizmos.color = spawnType == SpawnType.Immediate
            ? new Color(1f, 0.5f, 0f, 0.4f)
            : new Color(0f, 0.8f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, 0.4f);

        if (spawnType == SpawnType.Proximity)
        {
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.15f);
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
    }
}