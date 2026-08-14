// Assets/Scripts/Player2/Projectile/BulletPool.cs
// Object pool đơn giản cho Bullet.
// Gán BulletPrefab trong Inspector, pool tự mở rộng nếu cần.

using UnityEngine;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Pool")]
    public GameObject bulletPrefab;
    public int initialSize = 110;

    // ── Internal ──────────────────────────────────────────
    private Queue<GameObject> pool = new Queue<GameObject>();

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
            pool.Enqueue(CreateBullet());
    }

    // ── Public API ────────────────────────────────────────
    public Bullet Get(Vector2 position, Vector2 direction, DamageSource source = DamageSource.Player)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : CreateBullet();

        obj.transform.position = position;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);

        Bullet bullet = obj.GetComponent<Bullet>();
        bullet.Launch(direction, this, source);
        return bullet;
    }

    public void Return(GameObject bullet)
    {
        bullet.SetActive(false);
        pool.Enqueue(bullet);
    }

    // ── Helper ────────────────────────────────────────────
    GameObject CreateBullet()
    {
        GameObject obj = Instantiate(bulletPrefab, transform);
        obj.SetActive(false);
        return obj;
    }
}