using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HellSlice
{
    // Visual response to the existing assistance state; never changes rescue or stage progression.
    public sealed class SliceMemoryFigure : MonoBehaviour
    {
        public GameObject Appearance;
        [Range(0, 1)] public float BoundOpacity = .65f;
        [Range(0, 1)] public float ReleasedOpacity;
        [Min(.1f)] public float FadeSeconds = 2f;
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

        void Awake()
        {
            if (Appearance == null) return;
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
            if (!Released && session.Assisted) Released = true;
            if (!Released || elapsed >= FadeSeconds) return;
            elapsed = Mathf.Min(elapsed + Time.unscaledDeltaTime, FadeSeconds);
            ApplyOpacity(Mathf.Lerp(BoundOpacity, ReleasedOpacity, Mathf.SmoothStep(0, 1, elapsed / FadeSeconds)));
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
