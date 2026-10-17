using UnityEngine;

namespace HellSlice
{
    // Stage 5 scripted figure (a copy of the protagonist): walks fixed points only (cell -> hold switch
    // -> exit area), then walks into the player and disappears. No colliders or pathfinding, so it never
    // blocks the player or gets stuck. Uses world time.
    public sealed class SliceHelperFigure : MonoBehaviour
    {
        public enum Phase { Trapped, ToSwitch, Holding, ToExit, Arrived, Merging, Merged }
        [Tooltip("Fixed points from the cell to the hold switch. The figure holds the switch at the last one.")]
        public Transform[] ToSwitch = new Transform[0];
        [Tooltip("Fixed points from the switch to the exit area. Keep the last one inside the exit area.")]
        public Transform[] ToExit = new Transform[0];
        [Min(.5f)] public float Speed = 4f;
        [Tooltip("Speed while walking into the player at the end (faster than the player, so it always arrives).")]
        [Min(.5f)] public float MergeSpeed = 7f;
        public Phase Current { get; private set; }
        public bool Holding => Current == Phase.Holding;
        public bool Arrived => Current == Phase.Arrived;
        public bool Merged => Current == Phase.Merged;
        int next;
        Transform mergeTarget;
        Vector3 savedScale;

        void Awake() { savedScale = transform.localScale; }

        public void Release() { if (Current == Phase.Trapped) { Current = Phase.ToSwitch; next = 0; } }
        public void LeaveSwitch() { if (Current == Phase.Holding) { Current = Phase.ToExit; next = 0; } }
        public bool MergeInto(Transform target)
        {
            if (Current != Phase.Arrived || target == null) return false;
            mergeTarget = target;
            Current = Phase.Merging;
            return true;
        }

        void Update()
        {
            var s = SliceSession.Current;
            if (s == null || !s.Running) return;
            if (Current == Phase.Merging) { Merge(s.Clock.WorldDelta); return; }
            var path = Current == Phase.ToSwitch ? ToSwitch : Current == Phase.ToExit ? ToExit : null;
            if (path == null) return;
            float step = Speed * s.Clock.WorldDelta;
            while (next < path.Length)
            {
                if (path[next] == null) { next++; continue; }
                Vector3 delta = path[next].position - transform.position;
                Face(delta, s.Clock.WorldDelta);
                if (delta.magnitude > step) { transform.position += delta.normalized * step; return; }
                transform.position = path[next].position;
                step -= delta.magnitude;
                next++;
            }
            Current = Current == Phase.ToSwitch ? Phase.Holding : Phase.Arrived;
        }

        void Merge(float dt)
        {
            Vector3 delta = mergeTarget.position - transform.position; delta.y = 0;
            Face(delta, dt);
            float distance = delta.magnitude;
            if (distance > .35f) transform.position += delta.normalized * Mathf.Min(distance, MergeSpeed * dt);
            // Shrink over the last metre so the figure visibly folds into the player.
            transform.localScale = savedScale * Mathf.Clamp(distance, .3f, 1f);
            if (distance <= .35f) { Current = Phase.Merged; gameObject.SetActive(false); }
        }

        void Face(Vector3 delta, float dt)
        {
            Vector3 flat = new Vector3(delta.x, 0, delta.z);
            if (flat.sqrMagnitude > .0001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(flat), 540 * dt);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 from = transform.position;
            foreach (var path in new[] { ToSwitch, ToExit })
                foreach (var point in path)
                {
                    if (point == null) continue;
                    Gizmos.DrawLine(from, point.position); Gizmos.DrawWireSphere(point.position, .3f);
                    from = point.position;
                }
        }
    }
}
