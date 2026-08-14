using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound Library")]
    public SoundData[] sounds;

    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 8;

    private List<AudioSource> _pool;
    private Dictionary<string, SoundData> _lookup;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildLookup();
        BuildPool();
    }

    void BuildLookup()
    {
        _lookup = new Dictionary<string, SoundData>();
        foreach (var s in sounds)
            _lookup[s.name] = s;
    }

    void BuildPool()
    {
        _pool = new List<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject("AudioSource_" + i);
            go.transform.SetParent(transform);
            _pool.Add(go.AddComponent<AudioSource>());
        }
    }

    // ───── PUBLIC API ─────

    public void Play(string soundName)
    {
        if (!_lookup.TryGetValue(soundName, out var data))
        {
            Debug.LogWarning($"[AudioManager] Sound '{soundName}' not found.");
            return;
        }

        var src = GetFreeSource();
        src.clip = data.clip;
        src.volume = data.volume;
        src.pitch = data.pitch + Random.Range(-data.pitchVariance, data.pitchVariance);
        src.loop = data.loop;
        src.Play();
    }

    public void Stop(string soundName)
    {
        if (!_lookup.TryGetValue(soundName, out var data)) return;
        foreach (var src in _pool)
            if (src.clip == data.clip && src.isPlaying) src.Stop();
    }

    AudioSource GetFreeSource()
    {
        foreach (var src in _pool)
            if (!src.isPlaying) return src;

        // Pool đầy → lấy cái đã chạy lâu nhất (override)
        return _pool[0];
    }
    
 
}