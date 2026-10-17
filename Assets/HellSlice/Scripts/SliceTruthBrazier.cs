using UnityEngine;

namespace HellSlice
{
    // One Stage 4 section: E at the fixed handle moves the brazier to the next authored anchor.
    // The correct anchor latches the section, showing its route markings and memory. No light/shadow detection.
    public sealed class SliceTruthBrazier : MonoBehaviour
    {
        [Tooltip("Fixed point the player stands near to press E.")]
        public Transform Handle;
        [Tooltip("Movable brazier root (stand, fire, light). It starts at the anchor it is nearest to.")]
        public Transform Brazier;
        [Tooltip("Predefined brazier positions, visited in order.")]
        public Transform[] Positions = new Transform[3];
        [Tooltip("Index into Positions that reveals the true route (the anchor with the paired mark).")]
        [Min(0)] public int CorrectPosition;
        [Min(.5f)] public float InteractionRange = 1.8f;
        [Min(.05f)] public float MoveDuration = .4f;
        [Tooltip("Optional: stays locked until this section is solved.")]
        public SliceTruthBrazier Prerequisite;
        [Tooltip("Saved inactive. Paired-bar markings on the real stones.")]
        public GameObject Markings;
        [Tooltip("Saved inactive. World text that keeps the memory readable after solving.")]
        public GameObject MemoryLabel;
        [TextArea] public string Memory;
        public bool Solved { get; private set; }
        public bool Moving { get; private set; }
        public bool Unlocked => Prerequisite == null || Prerequisite.Solved;
        public int CurrentPosition => index;
        int index;
        float progress;
        Vector3 from;

        void Start()
        {
            // Start from whichever anchor the brazier was saved closest to.
            float best = float.MaxValue;
            for (int i = 0; i < Positions.Length; i++)
            {
                if (Positions[i] == null || Brazier == null) continue;
                Vector3 delta = Positions[i].position - Brazier.position; delta.y = 0;
                if (delta.sqrMagnitude < best) { best = delta.sqrMagnitude; index = i; }
            }
        }

        public bool InReach(SlicePlayer player)
        {
            if (Handle == null || player == null) return false;
            Vector3 p = player.transform.position;
            return Vector3.Distance(p, Handle.position) <= InteractionRange &&
                !Physics.Linecast(p + Vector3.up, Handle.position + Vector3.up, SliceWorld.SolidMask, QueryTriggerInteraction.Ignore);
        }

        public bool Advance()
        {
            if (Solved || Moving || !Unlocked || Brazier == null || Positions.Length == 0) return false;
            from = Brazier.position;
            for (int step = 0; step < Positions.Length; step++)
            {
                index = (index + 1) % Positions.Length;
                if (Positions[index] != null) break;
            }
            progress = 0;
            Moving = true;
            return true;
        }

        void Update()
        {
            var s = SliceSession.Current;
            if (!Moving || s == null || !s.Running) return;
            progress = Mathf.Min(1, progress + Time.unscaledDeltaTime / MoveDuration);
            float eased = progress * progress * (3f - 2f * progress);
            Brazier.position = Vector3.Lerp(from, Positions[index].position, eased);
            if (progress < 1) return;
            Moving = false;
            if (index != CorrectPosition) return;
            Solved = true;
            if (Markings != null) Markings.SetActive(true);
            if (MemoryLabel != null) MemoryLabel.SetActive(true);
        }

        void OnDrawGizmosSelected()
        {
            if (Handle != null) { Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(Handle.position, InteractionRange); }
            for (int i = 0; i < Positions.Length; i++)
            {
                if (Positions[i] == null) continue;
                Gizmos.color = i == CorrectPosition ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(Positions[i].position, .45f);
            }
        }
    }
}
