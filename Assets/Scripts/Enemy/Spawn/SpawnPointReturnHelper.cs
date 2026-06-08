// Assets/Scripts/Enemy/SpawnPointReturnHelper.cs
// Gắn tự động lên enemy khi spawn.
// Lưu prefab gốc để trả về đúng pool khi chết.

using UnityEngine;

public class SpawnPointReturnHelper : MonoBehaviour
{
    private GameObject prefab;

    public void Init(GameObject sourcePrefab)
    {
        prefab = sourcePrefab;

        HealthComponent health = GetComponent<HealthComponent>();
        if (health != null)
            health.onDeath.AddListener(OnDeath);
    }

    void OnDeath(DamageSource source)
    {
        EnemySpawnManager.Instance?.Return(prefab, gameObject);//
    }
}