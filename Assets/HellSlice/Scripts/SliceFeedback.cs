using System.Collections.Generic;
using UnityEngine;

namespace HellSlice
{
    public sealed class SliceFeedback : MonoBehaviour
    {
        public bool Muted;
        AudioSource source;
        readonly Dictionary<int, AudioClip> clips = new Dictionary<int, AudioClip>();
        void Awake() { source = GetComponent<AudioSource>(); if (source == null) { source = gameObject.AddComponent<AudioSource>(); source.spatialBlend = 0; source.volume = .12f; } }
        public void Cue(int frequency, float length = .12f)
        {
            if (Muted) return;
            int key = frequency * 1000 + Mathf.RoundToInt(length * 1000);
            if (!clips.TryGetValue(key, out var clip))
            {
                int count = Mathf.Max(1, Mathf.RoundToInt(22050 * length));
                float[] data = new float[count];
                for (int i = 0; i < count; i++) { float t = i / 22050f; data[i] = Mathf.Sin(t * frequency * Mathf.PI * 2) * Mathf.Sin(Mathf.PI * i / count) * .6f; }
                clip = AudioClip.Create("Original cue " + frequency, count, 1, 22050, false); clip.SetData(data, 0); clips.Add(key, clip);
            }
            source.PlayOneShot(clip);
        }
        void OnDestroy() { foreach (var clip in clips.Values) Destroy(clip); }
    }
}

