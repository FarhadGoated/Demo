using UnityEngine;

namespace HellSlice
{
    // Stage 5: only the upper route exists at first; its collapse raises the lower path. There the
    // trapped figure — the protagonist himself — holds a switch, the player opens his passage beyond
    // the flames, and when both reach the exit area the figure merges into the player.
    public sealed class SlicePrideStage : MonoBehaviour
    {
        [Tooltip("Direct upper route. The only way open at first; it collapses when the player walks onto it.")]
        public SliceCollapsingRoute UpperRoute;
        [Tooltip("Saved inactive. Lower-path stones; they rise out of the lava when the upper route collapses.")]
        public Transform LowerPath;
        [Min(.05f)] public float LowerPathRiseSeconds = 1.5f;
        [Min(0f)] public float LowerPathRiseDepth = 1.5f;
        [Tooltip("The trapped figure: a copy of the protagonist.")]
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
        [Tooltip("Both the player and the figure must be inside before the figure merges and the forward gate opens.")]
        public BoxCollider ExitArea;
        public GameObject ForwardGate;
        [Tooltip("Shown on the HUD when the upper route collapses. Leave empty to show nothing.")]
        [TextArea] public string Memory = "I thought asking for help made me small.";
        [Tooltip("Shown on the HUD when the figure merges into the player. Leave empty to show nothing.")]
        [TextArea] public string MergeLine = "The one I helped was me.";
        [Min(1f)] public float MemorySeconds = 8f;
        [Tooltip("The exit waits until the merge line has been on screen this long, so it is read before any scene change.")]
        [Min(0f)] public float MergeLineHoldSeconds = 2.5f;
        public bool RouteOpen { get; private set; }
        public bool GateOpen { get; private set; }
        public bool LowerPathRaised { get; private set; }
        Vector3 routeScale, lowerPathHome;
        float routeProgress, riseProgress, lineTime, sinceMerge;
        string line;
        bool collapseAnnounced;

        void Start()
        {
            if (FinalRoute != null) routeScale = FinalRoute.localScale;
            if (LowerPath != null) lowerPathHome = LowerPath.localPosition;
        }

        public string Objective
        {
            get
            {
                var session = SliceSession.Current;
                var phase = Figure != null ? Figure.Current : SliceHelperFigure.Phase.Trapped;
                string goal = session != null && session.StageCompleted ? "Stage 5 complete / The next chamber is not available yet."
                    : GateOpen ? "Enter the tower."
                    : phase >= SliceHelperFigure.Phase.Arrived ? "Join them at the tower gate."
                    : phase == SliceHelperFigure.Phase.ToExit ? "Wait for them at the tower gate."
                    : phase == SliceHelperFigure.Phase.Holding ? "They hold the bridge. Cross the flames, then open their way."
                    : phase == SliceHelperFigure.Phase.ToSwitch ? "They head for the switch."
                    : UpperRoute != null && UpperRoute.Collapsed ? "The high way is gone. Take the lower path."
                    : "Cross to the tower.";
                return lineTime > 0 ? "\"" + line + "\"\n" + goal : goal;
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
            if (lineTime > 0) lineTime -= Time.unscaledDeltaTime;
            if (!collapseAnnounced && UpperRoute != null && UpperRoute.Collapsed)
            {
                collapseAnnounced = true;
                session.Feedback.Cue(70, .5f);
                Show(Memory);
                if (LowerPath != null) { LowerPath.gameObject.SetActive(true); RaiseLowerPath(0); }
            }
            if (collapseAnnounced && riseProgress < 1) RaiseLowerPath(session.Clock.WorldDelta / LowerPathRiseSeconds);
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
            // Both at the gate: the figure walks into the player. Once merged, the gate opens.
            if (Figure.Arrived && Inside(ExitArea, session.Player.transform.position) && Figure.MergeInto(session.Player.transform))
                session.Feedback.Cue(660, .3f);
            if (!GateOpen && Figure.Merged)
            {
                GateOpen = true;
                if (ForwardGate != null) ForwardGate.SetActive(false);
                session.Feedback.Cue(880);
                session.Player.SetReveal((session.EarlierCompletedStages + 1) / 7f, false);
                Show(MergeLine);
            }
            if (GateOpen) sinceMerge += Time.unscaledDeltaTime;
            if (GateOpen && sinceMerge >= MergeLineHoldSeconds && session.World.ExitContains(session.Player.transform.position)) session.CompleteStage();
        }

        void Show(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            line = text;
            lineTime = MemorySeconds;
        }

        void RaiseLowerPath(float amount)
        {
            riseProgress = Mathf.Min(1, riseProgress + amount);
            LowerPathRaised = riseProgress >= 1;
            if (LowerPath == null) return;
            float eased = riseProgress * riseProgress * (3f - 2f * riseProgress);
            LowerPath.localPosition = lowerPathHome + Vector3.down * (LowerPathRiseDepth * (1 - eased));
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
