// Assets/Scripts/Enemy/EnemySpawner.cs
// Spawn enemy từ 2 nguồn:
//   1. Điểm spawn cố định (gán trong Inspector)
//   2. Vị trí ngẫu nhiên quanh rìa map
// Dùng Object Pool để tránh tạo/xóa liên tục gây lag

using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────
    [Header("Prefab")]
    public GameObject enemyPrefab;

    [Header("Spawn Rate")]
    [Tooltip("Số giây giữa mỗi lần spawn")]
    public float spawnInterval = 2f;

    [Tooltip("Số enemy tối đa tồn tại cùng lúc")]
    public int maxEnemies = 20;

    [Header("Fixed Spawn Points")]
    [Tooltip("Kéo các Empty GameObject vào đây làm điểm spawn cố định")]
    public Transform[] fixedSpawnPoints;

    [Header("Random Edge Spawn")]
    [Tooltip("Bật/tắt spawn ngẫu nhiên rìa map")]
    public bool useEdgeSpawn = true;

    [Tooltip("Tâm map (thường để (0,0))")]
    public Vector2 mapCenter = Vector2.zero;

    [Tooltip("Nửa chiều rộng map (units)")]
    public float mapHalfWidth = 10f;

    [Tooltip("Nửa chiều cao map (units)")]
    public float mapHalfHeight = 10f;

    [Tooltip("Spawn lùi vào trong bao nhiêu đơn vị so với rìa")]
    public float edgeInset = 0.5f;

    // ── Object Pool ──────────────────────────────────────
    [Header("Object Pool")]
    [Tooltip("Số enemy khởi tạo sẵn trong pool")]
    public int poolSize = 30;

    private Queue<GameObject> pool = new Queue<GameObject>();

    // ── Private ──────────────────────────────────────────
    private float spawnTimer = 0f;
    private int   activeCount = 0;

    // ── Unity lifecycle ──────────────────────────────────
    void Start()
    {
        InitPool();
    }

    void Update()
    {
        if (activeCount >= maxEnemies) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }
    }

    // ── Object Pool ──────────────────────────────────────
    void InitPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(enemyPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    GameObject GetFromPool()
    {
        // Nếu pool cạn → tạo thêm
        if (pool.Count == 0)
        {
            GameObject extra = Instantiate(enemyPrefab);
            extra.SetActive(false);
            pool.Enqueue(extra);
        }

        return pool.Dequeue();
    }

    // Gọi từ EnemyController khi enemy chết / ra khỏi map
    public void ReturnToPool(GameObject enemy)
    {
        enemy.SetActive(false);
        pool.Enqueue(enemy);
        activeCount--;
    }

    // ── Spawn logic ──────────────────────────────────────
    void SpawnEnemy()
    {
        Vector2 spawnPos = ChooseSpawnPosition();

        GameObject obj = GetFromPool();
        obj.transform.position = spawnPos;
        obj.SetActive(true);

        // Gán reference spawner để enemy có thể trả về pool khi chết
        EnemyController ec = obj.GetComponent<EnemyController>();
        if (ec != null) ec.spawner = this;

        activeCount++;
    }

    Vector2 ChooseSpawnPosition()
    {
        // Nếu có điểm cố định VÀ edge spawn → random chọn nguồn
        bool hasFixed = fixedSpawnPoints != null && fixedSpawnPoints.Length > 0;

        if (hasFixed && useEdgeSpawn)
        {
            return Random.value < 0.5f ? FixedSpawnPos() : EdgeSpawnPos();
        }
        if (hasFixed)  return FixedSpawnPos();
        if (useEdgeSpawn) return EdgeSpawnPos();

        return mapCenter; // fallback
    }

    Vector2 FixedSpawnPos()
    {
        int idx = Random.Range(0, fixedSpawnPoints.Length);
        return fixedSpawnPoints[idx].position;
    }

    Vector2 EdgeSpawnPos()
    {
        // Chọn ngẫu nhiên 1 trong 4 cạnh rồi lấy điểm random trên cạnh đó
        int edge = Random.Range(0, 4);
        float x, y;
        float inset = edgeInset;

        switch (edge)
        {
            case 0: // cạnh trên
                x = Random.Range(mapCenter.x - mapHalfWidth,  mapCenter.x + mapHalfWidth);
                y = mapCenter.y + mapHalfHeight - inset;
                break;
            case 1: // cạnh dưới
                x = Random.Range(mapCenter.x - mapHalfWidth,  mapCenter.x + mapHalfWidth);
                y = mapCenter.y - mapHalfHeight + inset;
                break;
            case 2: // cạnh trái
                x = mapCenter.x - mapHalfWidth + inset;
                y = Random.Range(mapCenter.y - mapHalfHeight, mapCenter.y + mapHalfHeight);
                break;
            default: // cạnh phải
                x = mapCenter.x + mapHalfWidth - inset;
                y = Random.Range(mapCenter.y - mapHalfHeight, mapCenter.y + mapHalfHeight);
                break;
        }

        return new Vector2(x, y);
    }

    // ── Gizmos: visualize map bounds + fixed points ──────
    void OnDrawGizmos()
    {
        // Viền map
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireCube(mapCenter, new Vector3(mapHalfWidth * 2f, mapHalfHeight * 2f, 0f));

        // Điểm spawn cố định
        if (fixedSpawnPoints == null) return;
        Gizmos.color = Color.red;
        foreach (Transform t in fixedSpawnPoints)
        {
            if (t != null)
                Gizmos.DrawWireSphere(t.position, 0.3f);
        }
    }
}