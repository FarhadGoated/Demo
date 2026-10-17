using UnityEngine;

namespace HellSlice
{
    public sealed class SliceBorrowedTimeStage : MonoBehaviour
    {
        public SliceTimedRescue[] Captives = new SliceTimedRescue[3];
        [Min(10f)] public float DeadlineSeconds = 100f;
        [Min(0f)] public float GraceSeconds = 8f;
        public SliceHazard[] FlameJets = new SliceHazard[0];
        public SliceMovingObstacle Obstacle;
        public GameObject ExitGate;
        public float Elapsed { get; private set; }
        public bool AllRescued { get; private set; }
        public int RescuedCount
        {
            get { int count = 0; foreach (var captive in Captives) if (captive != null && captive.Released) count++; return count; }
        }
        public float Remaining => Mathf.Max(0, DeadlineSeconds - Elapsed);
        public string Objective => SliceSession.Current != null && SliceSession.Current.StageCompleted
            ? "Borrowed Time complete / You acted before it was too late."
            : AllRescued ? "All three are safe. Return to the central exit."
            : "Rescue the three figures (" + RescuedCount + " / " + Captives.Length + ") / " + Mathf.CeilToInt(Remaining) +
                "s of fire time. " + (Elapsed < GraceSeconds ? "Read the routes. Q slows the fire." : Remaining <= 25 ? "Fire is close! Q buys time." : "Q slows fire and hazards.");

        public string HandleInteraction(SliceSession session)
        {
            SliceTimedRescue nearest = null;
            float distance = float.PositiveInfinity;
            foreach (var captive in Captives)
            {
                if (captive == null || captive.Released || !captive.InReach(session.Player)) continue;
                float d = Vector3.SqrMagnitude(session.Player.transform.position - captive.transform.position);
                if (d < distance) { nearest = captive; distance = d; }
            }
            if (nearest == null) return "";
            if (session.Input.Interact && !session.Player.Dodging && nearest.Release(session))
                return "Released / The side walkway leads safely back.";
            return "E / Release " + nearest.DisplayName;
        }

        public void Tick(SliceSession session)
        {
            if (!session.Running) return;
            if (!AllRescued && Captives.Length > 0 && RescuedCount == Captives.Length)
            {
                AllRescued = true;
                if (ExitGate != null) ExitGate.SetActive(false);
                foreach (var jet in FlameJets)
                {
                    if (jet == null) continue;
                    jet.enabled = false;
                    if (jet.Flames != null) jet.Flames.gameObject.SetActive(false);
                }
                if (Obstacle != null) Obstacle.Stop();
                session.Player.SetReveal((session.EarlierCompletedStages + 1) / 7f, false);
            }
            if (AllRescued)
            {
                if (session.World.ExitContains(session.Player.transform.position)) session.CompleteStage();
                return;
            }
            Elapsed += session.Clock.WorldDelta;
            float grace = Mathf.Clamp(GraceSeconds, 0, Mathf.Max(0, DeadlineSeconds - 1));
            float progress = Mathf.Clamp01((Elapsed - grace) / Mathf.Max(1, DeadlineSeconds - grace));
            foreach (var captive in Captives) if (captive != null) captive.AdvanceFire(progress);
            if (Elapsed >= DeadlineSeconds) session.Restart();
        }
    }
}
