// Assets/Scripts/Enemy/EnemySpawnManager.cs
// Quản lý pool chung và tất cả SpawnPoint trên map.
// SpawnPoint tự đăng ký vào đây khi Awake.

using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────
    public static EnemySpawnManager Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────
    [Header("Pool")]
    [Tooltip("Pool size cho từng prefab — tự mở rộng nếu cần")]
    public int defaultPoolSizePerPrefab = 10;

    // ── Internal ──────────────────────────────────────────
    // Pool riêng cho từng prefab
    private Dictionary<GameObject, Queue<GameObject>> pools
        = new Dictionary<GameObject, Queue<GameObject>>();

    private List<SpawnPoint> allSpawnPoints = new List<SpawnPoint>();

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ── SpawnPoint đăng ký ────────────────────────────────
    public void Register(SpawnPoint sp)
    {
        if (!allSpawnPoints.Contains(sp))
            allSpawnPoints.Add(sp);
    }

    // ── Pool API ──────────────────────────────────────────
    public GameObject Get(GameObject prefab, Vector2 position)
    {
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        Queue<GameObject> pool = pools[prefab];

        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab);
        obj.transform.position = position;
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject prefab, GameObject obj)
    {
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<GameObject>();

        obj.SetActive(false);
        pools[prefab].Enqueue(obj);
    }
}