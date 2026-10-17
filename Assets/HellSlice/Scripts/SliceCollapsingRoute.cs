using UnityEngine;

namespace HellSlice
{
    // Stage 5 upper route: entering the trigger on the stable stub drops every segment ahead.
    // Segments lose collision at once and the broken edge appears, so the route can never be used
    // and the stub stays a safe way back. Scene reload restores the saved bridge.
    public sealed class SliceCollapsingRoute : MonoBehaviour
    {
        [Tooltip("Trigger volume on the stable stub. The player entering it starts the collapse ahead of them.")]
        public BoxCollider Trigger;
        [Tooltip("Bridge pieces beyond the trigger, nearest first. Saved active; they fall into the lava and are disabled.")]
        public Transform[] Segments = new Transform[0];
        [Tooltip("Saved inactive. Solid broken edge that keeps the player on the stub after the collapse.")]
        public GameObject BrokenEdge;
        [Min(0f)] public float Stagger = .18f;
        [Min(.1f)] public float FallDuration = 1.6f;
        [Min(1f)] public float FallDistance = 12f;
        public bool Collapsed { get; private set; }
        public bool Finished { get; private set; }
        Vector3[] start = new Vector3[0];
        Quaternion[] startRotation = new Quaternion[0];
        float elapsed;

        void Update()
        {
            var s = SliceSession.Current;
            if (s == null || !s.Running || Finished) return;
            if (!Collapsed)
            {
                if (Trigger != null && Trigger.enabled && Trigger.bounds.Intersects(s.Player.Body.bounds)) Collapse();
                return;
            }
            elapsed += s.Clock.WorldDelta;
            bool falling = false;
            for (int i = 0; i < Segments.Length; i++)
            {
                var segment = Segments[i];
                if (segment == null || !segment.gameObject.activeSelf) continue;
                float t = (elapsed - i * Stagger) / FallDuration;
                if (t >= 1) { segment.gameObject.SetActive(false); continue; }
                falling = true;
                if (t <= 0) continue;
                segment.position = start[i] + Vector3.down * (FallDistance * t * t);
                segment.rotation = startRotation[i] * Quaternion.Euler(28 * t, 0, (i % 2 == 0 ? 12 : -12) * t);
            }
            if (!falling) Finished = true;
        }

        public void Collapse()
        {
            if (Collapsed) return;
            Collapsed = true;
            start = new Vector3[Segments.Length];
            startRotation = new Quaternion[Segments.Length];
            for (int i = 0; i < Segments.Length; i++)
            {
                if (Segments[i] == null) continue;
                start[i] = Segments[i].position;
                startRotation[i] = Segments[i].rotation;
                foreach (var solid in Segments[i].GetComponentsInChildren<Collider>(true)) solid.enabled = false;
            }
            if (BrokenEdge != null) BrokenEdge.SetActive(true);
        }

        void OnDrawGizmosSelected()
        {
            if (Trigger == null) return;
            Gizmos.color = Color.red;
            Gizmos.matrix = Trigger.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Trigger.center, Trigger.size);
        }
    }
}
