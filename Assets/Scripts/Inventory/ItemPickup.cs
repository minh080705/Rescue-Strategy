// Assets/Scripts/Inventory/ItemPickup.cs
// Object item trên map — player chạm vào thì nhặt.
// Tự biến mất sau lifetime giây, nhấp nháy khi sắp hết thời gian.

using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────
    [Header("Item")]
    public ItemDefinition item;
    public int quantity = 1;

    [Header("Pickup")]
    public string playerTag = "Player";

    // ── Internal ──────────────────────────────────────────
    private float timer;
    private float flickerTimer;
    private bool isFlickering = false;
    private SpriteRenderer sr;

    // ── Unity lifecycle ───────────────────────────────────
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (item == null) return;
        timer = item.lifetime;
        flickerTimer = item.flickerTime;
    }

    void Update()
    {
        if (item == null) return;

        timer -= Time.deltaTime;

        // Bắt đầu nhấp nháy khi sắp hết thời gian
        if (!isFlickering && timer <= flickerTimer)
        {
            isFlickering = true;
            InvokeRepeating(nameof(Flicker), 0f, 0.15f);
        }

        // Hết thời gian → biến mất
        if (timer <= 0f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (InventorySystem.Instance == null) return;

        bool success = InventorySystem.Instance.AddItem(item, quantity);
        if (success) { 
            Destroy(gameObject);
            AudioManager.Instance.Play("CoinPickup");
        }
    }

    // ── Flicker ───────────────────────────────────────────
    void Flicker()
    {
        if (sr != null)
            sr.enabled = !sr.enabled;
    }

    void OnDestroy()
    {
        CancelInvoke(nameof(Flicker));
    }

    // ── Gizmos ────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (item == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}