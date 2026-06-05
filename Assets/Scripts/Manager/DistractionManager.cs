// Assets/Scripts/Managers/DistractionManager.cs

using System.Collections.Generic;
using UnityEngine;

public class DistractionManager : MonoBehaviour
{
    public static DistractionManager Instance { get; private set; }

    private List<Transform> distractions = new List<Transform>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(Transform t)   => distractions.Add(t);
    public void Unregister(Transform t) => distractions.Remove(t);
    public bool HasDistraction          => distractions.Count > 0;

    public Transform GetClosest(Vector2 from)
    {
        Transform closest = null;
        float minDist = float.MaxValue;

        foreach (Transform t in distractions)
        {
            if (t == null || !t.gameObject.activeInHierarchy)
                continue;

            float d = Vector2.Distance(from, t.position);

            if (d < minDist)
            {
                minDist = d;
                closest = t;
            }
        }

        return closest;
    }
}