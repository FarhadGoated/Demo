using UnityEngine;

namespace HellSlice
{
    // Stage 4: three brazier sections reveal the true stones and memories; returning the
    // evidence acknowledges responsibility and opens the exit. Movement, death and progress stay shared.
    public sealed class SliceShadowTruthStage : MonoBehaviour
    {
        [Tooltip("Sections in route order. Solving the last one exposes the evidence.")]
        public SliceTruthBrazier[] Sections = new SliceTruthBrazier[3];
        [Tooltip("Signed receipt. Saved inactive; uses the existing carry/drop/return rules.")]
        public SliceValuable Evidence;
        public GameObject ExitGate;
        [Min(1f)] public float MemorySeconds = 8f;
        public bool EvidenceReturned { get; private set; }
        public int SolvedSections
        {
            get
            {
                int count = 0;
                foreach (var section in Sections) if (section != null && section.Solved) count++;
                return count;
            }
        }
        bool[] announced = new bool[0];
        bool evidenceExposed;
        SliceValuable carried;
        string memory;
        float memoryTime;

        public string Objective
        {
            get
            {
                var session = SliceSession.Current;
                if (session != null && session.StageCompleted) return "Stage 4 complete / The next chamber is not available yet.";
                string goal = Evidence != null && Evidence.Lost ? "The receipt was lost in the fire. Press Esc and choose Respawn to restart."
                    : EvidenceReturned ? "The truth is on record. Reach the exit."
                    : carried != null ? "Carry the receipt across the marked stones to Mara's case file."
                    : evidenceExposed ? "Take the signed receipt to Mara's case file."
                    : "Set each brazier on its paired mark to reveal the true stones (" + SolvedSections + " / " + Sections.Length + ").";
                return memoryTime > 0 ? "\"" + memory + "\"\n" + goal : goal;
            }
        }

        public string HandleInteraction(SliceSession session)
        {
            var player = session.Player;
            if (carried != null)
            {
                carried.transform.position = player.transform.position + Vector3.up * 1.1f + player.transform.forward * .65f;
                bool matching = carried.Receptacle != null && Near(player, carried.Receptacle.position, carried.InteractionRange);
                string prompt = matching ? "E / Place the receipt in Mara's case file" : "E / Drop " + carried.DisplayName;
                if (session.Input.Interact)
                {
                    if (matching && carried.ReturnToPlace()) Acknowledge(session);
                    else carried.Drop();
                    carried = null;
                    player.Carrying = false;
                }
                return prompt;
            }
            if (Evidence != null && evidenceExposed && Evidence.isActiveAndEnabled && Evidence.Available &&
                Near(player, Evidence.transform.position, Evidence.InteractionRange))
            {
                if (session.Input.Interact && !player.Dodging)
                {
                    carried = Evidence;
                    carried.PickUp();
                    player.Carrying = true;
                    session.Feedback.Cue(400);
                }
                return "E / Carry " + Evidence.DisplayName;
            }
            foreach (var section in Sections)
            {
                if (section == null || section.Solved || !section.InReach(player)) continue;
                if (!section.Unlocked) return "Reveal the earlier memory first";
                if (section.Moving) return "";
                if (session.Input.Interact && !player.Carrying && section.Advance()) session.Feedback.Cue(300, .08f);
                return "E / Move brazier (position " + (section.CurrentPosition + 1) + " of " + section.Positions.Length + ")";
            }
            return "";
        }

        void Acknowledge(SliceSession session)
        {
            if (EvidenceReturned) return;
            EvidenceReturned = true;
            if (ExitGate != null) ExitGate.SetActive(false);
            session.Feedback.Cue(880);
            session.Player.SetReveal((session.EarlierCompletedStages + 1) / 7f, false);
        }

        static bool Near(SlicePlayer player, Vector3 target, float range)
        {
            return Vector3.Distance(player.transform.position, target) <= range &&
                !Physics.Linecast(player.transform.position + Vector3.up, target + Vector3.up,
                    SliceWorld.SolidMask, QueryTriggerInteraction.Ignore);
        }

        public void Tick(SliceSession session)
        {
            if (announced.Length != Sections.Length) announced = new bool[Sections.Length];
            if (memoryTime > 0) memoryTime -= Time.unscaledDeltaTime;
            for (int i = 0; i < Sections.Length; i++)
            {
                var section = Sections[i];
                if (section == null || !section.Solved || announced[i]) continue;
                announced[i] = true;
                memory = section.Memory;
                memoryTime = MemorySeconds;
                session.Feedback.Cue(660);
            }
            var last = Sections.Length > 0 ? Sections[Sections.Length - 1] : null;
            if (!evidenceExposed && last != null && last.Solved && Evidence != null)
            {
                evidenceExposed = true;
                Evidence.gameObject.SetActive(true);
            }
            if (EvidenceReturned && session.World.ExitContains(session.Player.transform.position)) session.CompleteStage();
        }
    }
}
