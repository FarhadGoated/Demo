using UnityEngine;

namespace HellSlice
{
    // Stage 7: the Stage 1 guardian returns. Its charge breaks three seals; activating each exposed mechanism
    // stuns and weakens it once, and the last one dissolves it. A quiet walk applies the final transformation,
    // a fade wakes the protagonist in hospital, and one waking action alone completes the game.
    public sealed class SliceReturnStage : MonoBehaviour
    {
        public enum Phase { Encounter, Walk, FadeOut, Hold, FadeIn, Awake, Done }
        [Tooltip("The returning Stage 1 guardian. Its SliceEnemy.ChargeEncounter must reference this component.")]
        public SliceEnemy Monster;
        public SliceSealedMechanism[] Mechanisms = new SliceSealedMechanism[3];
        [Min(.5f), Tooltip("World seconds the monster is stunned after each activation.")]
        public float StunSeconds = 4f;
        [Tooltip("Arena hazards switched off once the monster has dissolved.")]
        public SliceHazard[] ArenaHazards = new SliceHazard[0];
        [Tooltip("Blocks the final passage until the monster has dissolved.")]
        public GameObject FinalGate;
        [Tooltip("Inside the quiet walk. Entering it applies the final transformation step (fully visible).")]
        public BoxCollider TransformationArea;
        [Tooltip("Player position and facing after the fade. SliceWorld.ExitVolume (the light) starts the fade.")]
        public Transform HospitalSpawn;
        [Tooltip("Floor-level point beside the bedside table where E takes the waking action.")]
        public Transform WakingAction;
        [Min(.5f)] public float WakingActionRange = 1.8f;
        public string WakingActionPrompt = "Call Mara back";
        [TextArea] public string WakeText = "You wake in a hospital bed.\nYour phone is on the table beside you.";
        [TextArea] public string WakingLine = "\"Mara, it was me. I took the wages and blamed you. I'll return them today and tell them the truth.\"";
        [TextArea] public string EndText = "A DAY THROUGH HELL\n\nHe cannot undo that night.\nHe can choose what he does next.";
        [Min(.1f)] public float FadeSeconds = 1.5f;
        [Min(0f)] public float WakeHoldSeconds = 2.5f;
        [Tooltip("The waking line stays readable this long before the closing card fades in.")]
        [Min(0f)] public float EndDelaySeconds = 6f;
        public Color FadeColour = new Color(.96f, .95f, .92f);
        public Phase Current { get; private set; }
        public bool Revealed { get; private set; }
        public int MechanismCount { get { int n = 0; foreach (var m in Mechanisms) if (m != null) n++; return n; } }
        public int ActivatedCount { get { int n = 0; foreach (var m in Mechanisms) if (m != null && m.Activated) n++; return n; } }
        float fade, endFade, timer;
        GUIStyle style;

        void Start()
        {
            if (Monster == null || Monster.ChargeEncounter != this)
                Debug.LogError("Stage 7: assign Monster, and set its SliceEnemy.ChargeEncounter to this component.", this);
        }

        public string Objective
        {
            get
            {
                string count = " (" + ActivatedCount + " / " + MechanismCount + ")";
                switch (Current)
                {
                    case Phase.Encounter:
                        string state = Monster != null ? Monster.State : "";
                        if (state == "Dormant") return "It came back. This time, turn and face it.";
                        if (state == "Dissolving" || state == "Dissolved") return "The nightmare loosens its hold.";
                        foreach (var m in Mechanisms)
                            if (m != null && m.Exposed && !m.Activated) return "A seal broke. Reach the exposed mechanism and press E." + count;
                        if (state == "Stunned") return "It reels. Lure it to the next seal." + count;
                        return "Bait its charge into a seal. Attacks cannot stop it." + count + "\nShift / Dodge   Q / Slow time";
                    case Phase.Walk: return "It is gone. Walk toward the light.";
                    case Phase.Awake: return "You are awake. Your phone is on the table.";
                    case Phase.Done: return WakingLine;
                    default: return "";
                }
            }
        }

        public string HandleInteraction(SliceSession session)
        {
            var player = session.Player;
            if (Current == Phase.Awake)
            {
                if (WakingAction == null || !InReach(player, WakingAction.position, WakingActionRange)) return "";
                if (!session.Input.Interact || player.Dodging) return "E / " + WakingActionPrompt;
                Current = Phase.Done; timer = EndDelaySeconds;
                session.Feedback.Cue(880, .3f);
                session.CompleteStage(); // The only completion: the game is never marked complete before this.
                return "";
            }
            if (Current != Phase.Encounter) return "";
            SliceSealedMechanism sealedNearby = null;
            foreach (var mechanism in Mechanisms)
            {
                if (mechanism == null || mechanism.Activated) continue;
                if (mechanism.Exposed && mechanism.InReach(player))
                {
                    if (!session.Input.Interact || player.Dodging) return "E / Activate the " + mechanism.DisplayName;
                    if (mechanism.Activate() && Monster != null && Monster.Weaken(this, StunSeconds)) session.Feedback.Cue(660);
                    return "";
                }
                if (!mechanism.Exposed && sealedNearby == null && mechanism.Near(player, mechanism.InteractionRange + 1.5f)) sealedNearby = mechanism;
            }
            return sealedNearby != null ? "Sealed / Bait its charge into the seal" : "";
        }

        // Called by the monster while charging. Each seal breaks once; its mechanism stays exposed until reload.
        public SliceSealedMechanism SealHitBy(Bounds body)
        {
            body.Expand(.4f);
            foreach (var mechanism in Mechanisms) if (mechanism != null && mechanism.Touches(body)) return mechanism;
            return null;
        }

        public void BreakSeal(SliceSealedMechanism mechanism)
        {
            if (mechanism == null || mechanism.Exposed) return;
            mechanism.Expose();
            var session = SliceSession.Current;
            if (session != null) session.Feedback.Cue(70, .4f);
        }

        public void Tick(SliceSession session)
        {
            float dt = Time.unscaledDeltaTime;
            var player = session.Player;
            switch (Current)
            {
                case Phase.Encounter:
                    if (Monster == null || !Monster.Dissolved) return;
                    Current = Phase.Walk;
                    if (FinalGate != null) FinalGate.SetActive(false);
                    foreach (var hazard in ArenaHazards)
                    {
                        if (hazard == null) continue;
                        hazard.enabled = false;
                        if (hazard.Flames != null) hazard.Flames.gameObject.SetActive(false);
                    }
                    session.Feedback.Cue(660, .4f);
                    return;
                case Phase.Walk:
                    if (!Revealed && Inside(TransformationArea, player.transform.position)) Reveal(session);
                    // Start only on the ground and outside a dodge, so no jump or dodge is frozen mid-air.
                    if (!session.World.ExitContains(player.transform.position) || player.Dodging || !player.Body.isGrounded) return;
                    Reveal(session); // The hospital is never reached without the final step.
                    Current = Phase.FadeOut;
                    player.enabled = false; // Stand still in the light while it fades.
                    return;
                case Phase.FadeOut:
                    fade = Mathf.MoveTowards(fade, 1, dt / FadeSeconds);
                    if (fade < 1) return;
                    WakeUp(session);
                    Current = Phase.Hold; timer = WakeHoldSeconds;
                    return;
                case Phase.Hold:
                    timer -= dt;
                    if (timer > 0) return;
                    Current = Phase.FadeIn;
                    player.enabled = true;
                    return;
                case Phase.FadeIn:
                    fade = Mathf.MoveTowards(fade, 0, dt / FadeSeconds);
                    if (fade <= 0) Current = Phase.Awake;
                    return;
                case Phase.Done:
                    timer -= dt;
                    if (timer > 0) return;
                    if (player.enabled) player.enabled = false;
                    endFade = Mathf.MoveTowards(endFade, 1, dt / FadeSeconds);
                    return;
            }
        }

        void Reveal(SliceSession session)
        {
            if (Revealed) return;
            Revealed = true;
            // Final existing transformation step: fully visible; body renderers keep casting shadows.
            session.Player.SetReveal(1f, false);
            session.Feedback.Cue(880, .3f);
        }

        void WakeUp(SliceSession session)
        {
            var player = session.Player;
            player.SetReveal(1f, true);
            if (HospitalSpawn != null)
            {
                // CharacterController positions only move while it is disabled.
                player.Body.enabled = false;
                player.transform.SetPositionAndRotation(HospitalSpawn.position, HospitalSpawn.rotation);
                player.Body.enabled = true;
                Physics.SyncTransforms();
            }
            session.CameraRig.ResetView();
            // Awake: the dream's slow time no longer applies (restored by any reload).
            session.SlowTimeUnlockStage = 0;
            session.Clock.ResetState();
        }

        static bool InReach(SlicePlayer player, Vector3 target, float range)
        {
            return Vector3.Distance(player.transform.position, target) <= range &&
                !Physics.Linecast(player.transform.position + Vector3.up, target + Vector3.up,
                    SliceWorld.SolidMask, QueryTriggerInteraction.Ignore);
        }

        static bool Inside(BoxCollider box, Vector3 feet)
        {
            if (box == null || !box.gameObject.activeInHierarchy) return false;
            return new Bounds(box.center, box.size).Contains(box.transform.InverseTransformPoint(feet + Vector3.up * .9f));
        }

        void OnGUI()
        {
            float alpha = Mathf.Max(fade, endFade);
            var session = SliceSession.Current;
            if (alpha <= 0 || Polish.PolishSettings.IsOpen || session == null || session.Paused) return;
            var old = GUI.color;
            GUI.color = new Color(FadeColour.r, FadeColour.g, FadeColour.b, alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            string text = endFade > 0 ? EndText : Current == Phase.Hold || Current == Phase.FadeIn ? WakeText : "";
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (style == null) style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true, font = Polish.PolishCine.Body };
                style.fontSize = Mathf.Max(14, Mathf.RoundToInt(Screen.height * .04f));
                style.normal.textColor = new Color(.08f, .09f, .11f);
                GUI.color = new Color(1, 1, 1, alpha);
                GUI.Label(new Rect(Screen.width * .15f, 0, Screen.width * .7f, Screen.height), text, style);
            }
            GUI.color = old;
        }
    }
}
