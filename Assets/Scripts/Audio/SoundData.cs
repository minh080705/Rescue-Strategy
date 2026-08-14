//Assets/Scripts/Audio/SoundData.cs

using UnityEngine;

[System.Serializable]
public class SoundData
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 2f)] public float pitch = 1f;
    public float pitchVariance = 0.05f; // random nhẹ mỗi lần play
    public bool loop = false;
}