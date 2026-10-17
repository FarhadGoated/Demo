using UnityEngine;

namespace HellSlice
{
    // Stage 5: the upper route collapses; the lower route needs the freed figure to hold a switch,
    // the player to open the figure's passage beyond the flames, and both to reach the exit area.
    public sealed class SlicePrideStage : MonoBehaviour
    {
        [Tooltip("Direct upper route. Collapses when the player walks onto it; never required.")]
        public SliceCollapsingRoute UpperRoute;
        public SliceHelperFigure Figure;
        [Tooltip("Player-side lever. Its OpensGate is the cell gate.")]
        public SlicePuzzleSwitch CellLever;
        [Tooltip("Plate the figure stands on. Lit while held.")]
        public Renderer SwitchPlate;
        [Tooltip("Saved inactive. Extends across the gap once the figure holds the switch, then stays.")]
        public Transform FinalRoute;
        [Tooltip("Saved inactive. Lit link from the switch to the final route.")]
        public GameObject SwitchLink;
        [Min(.05f)] public float RouteExtendSeconds = 1f;
        [Tooltip("Lever beyond the flames. Its OpensGate is the figure's passage door. Works only while the figure holds the switch.")]
        public SlicePuzzleSwitch PassageLever;
        [Tooltip("Both the player and the figure must be inside before the forward gate opens.")]
        public BoxCollider ExitArea;
        public GameObject ForwardGate;
        [Tooltip("Shown on the HUD when the upper route collapses. Leave empty to show nothing.")]
        [TextArea] public string Memory = "I thought asking for help made me small.";
        [Min(1f)] public float MemorySeconds = 8f;
        public bool RouteOpen { get; private set; }
        public bool GateOpen { get; private set; }
        Vector3 routeScale;
        float routeProgress, memoryTime;
        bool collapseAnnounced;

        void Start() { if (FinalRoute != null) routeScale = FinalRoute.localScale; }

        public string Objective
        {
            get
            {
                var session = SliceSession.Current;
                if (session != null && session.StageCompleted) return "Stage 5 complete / The next chamber is not available yet.";
                var phase = Figure != null ? Figure.Current : SliceHelperFigure.Phase.Trapped;
                string goal = GateOpen ? "Go on together."
                    : phase == SliceHelperFigure.Phase.Arrived ? "Join them at the tower gate."
                    : phase == SliceHelperFigure.Phase.ToExit ? "Wait for them at the tower gate."
                    : phase == SliceHelperFigure.Phase.Holding ? "They hold the bridge. Cross the flames, then open their way."
                    : phase == SliceHelperFigure.Phase.ToSwitch ? "They head for the switch."
                    : UpperRoute != null && UpperRoute.Collapsed ? "The high way is gone. Take the lower path."
                    : "Cross to the tower.";
                return memoryTime > 0 ? "\"" + Memory + "\"\n" + goal : goal;
            }
        }

        public string HandleInteraction(SliceSession session)
        {
            var player = session.Player;
            if (CellLever != null && CellLever.CanInteract(player))
            {
                if (session.Input.Interact && CellLever.Interact(player)) session.Feedback.Cue(440);
                return "E / " + CellLever.InteractionLabel;
            }
            if (PassageLever != null && PassageLever.CanInteract(player))
            {
                if (Figure == null || !Figure.Holding) return "They must hold the switch first";
                if (session.Input.Interact && PassageLever.Interact(player)) session.Feedback.Cue(520);
                return "E / " + PassageLever.InteractionLabel;
            }
            return "";
        }

        public void Tick(SliceSession session)
        {
            if (memoryTime > 0) memoryTime -= Time.unscaledDeltaTime;
            if (!collapseAnnounced && UpperRoute != null && UpperRoute.Collapsed)
            {
                collapseAnnounced = true;
                session.Feedback.Cue(70, .5f);
                if (!string.IsNullOrWhiteSpace(Memory)) memoryTime = MemorySeconds;
            }
            if (Figure == null) return;
            if (CellLever != null && CellLever.Completed) Figure.Release();
            if (!RouteOpen && Figure.Holding)
            {
                RouteOpen = true;
                session.Feedback.Cue(660);
                if (SwitchLink != null) SwitchLink.SetActive(true);
                if (SwitchPlate != null)
                {
                    var block = new MaterialPropertyBlock(); SwitchPlate.GetPropertyBlock(block);
                    block.SetColor("_BaseColor", new Color(.25f, 1, .4f)); SwitchPlate.SetPropertyBlock(block);
                }
                if (FinalRoute != null) { FinalRoute.gameObject.SetActive(true); ExtendRoute(0); }
            }
            if (RouteOpen && routeProgress < 1) ExtendRoute(session.Clock.WorldDelta / RouteExtendSeconds);
            if (PassageLever != null && PassageLever.Completed) Figure.LeaveSwitch();
            if (!GateOpen && Figure.Arrived && Inside(ExitArea, session.Player.transform.position))
            {
                GateOpen = true;
                if (ForwardGate != null) ForwardGate.SetActive(false);
                session.Feedback.Cue(880);
                session.Player.SetReveal((session.EarlierCompletedStages + 1) / 7f, false);
            }
            if (GateOpen && session.World.ExitContains(session.Player.transform.position)) session.CompleteStage();
        }

        void ExtendRoute(float amount)
        {
            // The route root's pivot sits on the near bank, so scaling Z slides the deck out over the gap.
            routeProgress = Mathf.Min(1, routeProgress + amount);
            if (FinalRoute != null) FinalRoute.localScale = new Vector3(routeScale.x, routeScale.y, routeScale.z * Mathf.Max(.02f, routeProgress));
        }

        static bool Inside(BoxCollider box, Vector3 feet)
        {
            if (box == null || !box.gameObject.activeInHierarchy) return false;
            return new Bounds(box.center, box.size).Contains(box.transform.InverseTransformPoint(feet + Vector3.up * .9f));
        }
    }
}
