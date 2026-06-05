// Assets/Scripts/Items/BoneItem.cs

using UnityEngine;

public class BoneItem : MonoBehaviour
{
    public float lifetime = 5f;

    [HideInInspector] public BoneThrower thrower;

    private float timer;

    void OnEnable()
    {
        timer = lifetime;
        DistractionManager.Instance?.Register(transform);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) Expire();
    }

    void Expire()
    {
        DistractionManager.Instance?.Unregister(transform);
        thrower?.ReturnToPool(gameObject);
    }
}