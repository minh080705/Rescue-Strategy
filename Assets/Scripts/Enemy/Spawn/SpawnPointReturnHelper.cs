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

        
    }

    public void ReturnToPoolNow()
    {
        EnemySpawnManager.Instance?.Return(prefab, gameObject);
    }
}