using UnityEngine;

namespace HellSlice.Polish
{
    /// <summary>Roaring-fire light flicker; flares when the light switches on (eruption start).</summary>
    [RequireComponent(typeof(Light))]
    public sealed class PolishLightFlicker : MonoBehaviour
    {
        Light l; float baseIntensity, seed, age;
        void Awake() { l = GetComponent<Light>(); baseIntensity = l.intensity; seed = Random.value * 100; }
        void OnEnable() => age = 0;
        void Update()
        {
            age += Time.deltaTime;
            float t = Time.time * 9 + seed;
            float flicker = .7f + .45f * Mathf.PerlinNoise(t, seed) + .15f * Mathf.Sin(t * 2.3f);
            float flare = 1 + 1.2f * Mathf.Exp(-age * 5);
            l.intensity = baseIntensity * flicker * flare;
        }
    }
}
