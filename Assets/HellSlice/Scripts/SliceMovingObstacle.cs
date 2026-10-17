using UnityEngine;

namespace HellSlice
{
    // A simple authored sliding pillar, using the same world-time movement convention as Stage 3.
    public sealed class SliceMovingObstacle : MonoBehaviour
    {
        public BoxCollider Blocker;
        public Transform From, To;
        [Min(.2f)] public float CrossingSeconds = 3f;
        [Min(0)] public float DwellSeconds = 1f;
        public float Elapsed { get; private set; }
        public bool Stopped { get; private set; }

        public void Stop() { Stopped = true; }
        void Update()
        {
            var s = SliceSession.Current;
            if (Stopped || s == null || !s.Running || Blocker == null || From == null || To == null) return;
            float proposed = Elapsed + s.Clock.WorldDelta;
            float half = Mathf.Max(.2f, CrossingSeconds) + Mathf.Max(0, DwellSeconds);
            float phase = Mathf.Repeat(proposed, half * 2);
            float t = Mathf.Clamp01((phase < half ? phase : phase - half) / Mathf.Max(.2f, CrossingSeconds));
            Vector3 position = phase < half ? Vector3.Lerp(From.position, To.position, t) : Vector3.Lerp(To.position, From.position, t);
            // Never push a CharacterController into an edge. The open side is always wide enough to leave.
            if (Physics.ComputePenetration(Blocker, position, Blocker.transform.rotation,
                s.Player.Body, s.Player.transform.position, s.Player.transform.rotation, out _, out _)) return;
            Blocker.transform.position = position;
            Elapsed = proposed;
        }
    }
}
