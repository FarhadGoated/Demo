using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HellSlice
{
    // The cage release drives the memory's faint/fade; completion uses the existing assistance action.
    public sealed class SliceMemoryFigure : MonoBehaviour
    {
        public GameObject Appearance;
        public Polish.PolishCageRescue CageRelease;
        [Range(0, 1)] public float BoundOpacity = 1f;
        [Range(0, 1)] public float ReleasedOpacity;
        [Min(.1f)] public float FadeSeconds = 2f;
        [Min(.1f)] public float FaintSeconds = .85f;
        public Vector3 FaintRotation = new Vector3(0, 0, 75);
        public bool Released { get; private set; }

        sealed class Surface
        {
            public Renderer Renderer;
            public int Slot;
            public Color Tint;
            public readonly MaterialPropertyBlock Block = new MaterialPropertyBlock();
        }

        readonly List<Surface> surfaces = new List<Surface>();
        float elapsed;
        Vector3 standingPosition;
        Quaternion standingRotation;

        void Awake()
        {
            if (Appearance == null) return;
            standingPosition = Appearance.transform.localPosition;
            standingRotation = Appearance.transform.localRotation;
            foreach (var renderer in Appearance.GetComponentsInChildren<Renderer>(true))
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                var materials = renderer.sharedMaterials;
                for (int slot = 0; slot < materials.Length; slot++)
                {
                    if (materials[slot] == null) continue;
                    surfaces.Add(new Surface
                    {
                        Renderer = renderer,
                        Slot = slot,
                        Tint = materials[slot].GetColor("_BaseColor")
                    });
                }
            }
            ApplyOpacity(BoundOpacity);
        }

        void Update()
        {
            var session = SliceSession.Current;
            if (session == null || !session.Running) return;
            if (!Released && (CageRelease != null ? CageRelease.Lifted : session.Assisted)) Released = true;
            if (!Released) return;
            float duration = Mathf.Max(.1f, Mathf.Max(FadeSeconds, FaintSeconds));
            elapsed = Mathf.Min(elapsed + session.Clock.WorldDelta, duration);
            if (Appearance != null && CageRelease != null)
            {
                float faint = Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / Mathf.Max(.1f, FaintSeconds)));
                Appearance.transform.localRotation = standingRotation * Quaternion.Euler(FaintRotation * faint);
                Appearance.transform.localPosition = standingPosition + Vector3.up * (.12f * faint);
            }
            float fade = Mathf.Clamp01(elapsed / Mathf.Max(.1f, FadeSeconds));
            ApplyOpacity(Mathf.Lerp(BoundOpacity, ReleasedOpacity, Mathf.SmoothStep(0, 1, fade)));
            if (CageRelease != null && elapsed >= duration && !session.Assisted) session.Assist();
        }

        void ApplyOpacity(float opacity)
        {
            foreach (var surface in surfaces)
            {
                Color tint = surface.Tint;
                tint.a = opacity;
                surface.Renderer.GetPropertyBlock(surface.Block, surface.Slot);
                surface.Block.SetColor("_BaseColor", tint);
                surface.Block.SetColor("_Color", tint);
                surface.Renderer.SetPropertyBlock(surface.Block, surface.Slot);
            }
        }
    }
}
