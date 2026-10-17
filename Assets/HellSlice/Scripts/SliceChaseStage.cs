using UnityEngine;

namespace HellSlice
{
    // Stage 1's opening chase. Crossing the door hands control back to the saved rescue puzzle.
    public sealed class SliceChaseStage : MonoBehaviour
    {
        public GameObject Door;
        public BoxCollider DoorCollider;
        public Transform Button;
        public Renderer ButtonVisual;
        public SlicePuzzleSwitch PlayerButton;
        [Min(.5f)] public float ButtonStrikeRange = 2.8f;
        [Min(.1f)] public float ButtonAttackWindupSeconds = .75f;
        public bool ButtonHit { get; private set; }
        public bool DoorClosed { get; private set; }
        public bool ChaseComplete { get; private set; }

        Bounds doorBounds;
        float attackTime;
        bool attackStarted;
        bool buttonApproach;

        void Start()
        {
            doorBounds = DoorCollider.bounds;
            SetButtonColor(new Color(1f, .24f, .035f));
        }

        public string Objective => !ButtonHit
            ? "Run forward. Stay ahead of the guardian."
            : !DoorClosed
                ? "The door is open. Run through the opening!"
                : "Cross the flames and reach the waiting figure.";

        public string HandleInteraction(SliceSession session)
        {
            if (ButtonHit || !PlayerButton.CanInteract(session.Player)) return "";
            if (session.Input.Interact && PlayerButton.Interact(session.Player)) OpenDoor(session);
            return ButtonHit ? "" : "E / Open door";
        }

        void OpenDoor(SliceSession session)
        {
            if (ButtonHit) return;
            ButtonHit = true;
            Door.SetActive(false);
            session.Feedback.Cue(180, .15f);
            session.Notify("The door is open. Run through the opening!");
            SetButtonColor(new Color(.15f, 1f, .38f));
            session.Enemy.TargetChaseButton(null);
            session.Enemy.ResumeChase();
        }

        public void Tick(SliceSession session)
        {
            if (ChaseComplete) return;

            session.Enemy.BeginChase();
            if (!ButtonHit)
            {
                if (!buttonApproach && session.Player.Body.bounds.max.z >= doorBounds.min.z - 2f)
                {
                    buttonApproach = true;
                    session.Enemy.TargetChaseButton(Button);
                }
                if (!buttonApproach) return;
                Vector3 toButton = Button.position - session.Enemy.transform.position;
                toButton.y = 0;
                float distance = toButton.magnitude;
                if (distance > ButtonStrikeRange)
                {
                    if (attackStarted) session.Enemy.ResumeChase();
                    attackStarted = false;
                    attackTime = 0;
                    return;
                }

                if (!attackStarted)
                {
                    session.Enemy.HoldForButtonAttack(Button.position);
                    attackStarted = true;
                    SetButtonColor(new Color(1f, .58f, .06f));
                }
                attackTime += session.Clock.WorldDelta;
                if (attackTime < ButtonAttackWindupSeconds) return;

                OpenDoor(session);
                return;
            }

            if (session.Player.Body.bounds.min.z <= doorBounds.max.z + .15f) return;

            Door.SetActive(true);
            DoorClosed = true;
            session.Enemy.StopChase();
            ChaseComplete = true;
            session.UnlockRescueProgression();
            session.Notify("The doorway seals behind you. Someone is still waiting.");
        }

        void SetButtonColor(Color color)
        {
            if (ButtonVisual == null) return;
            var block = new MaterialPropertyBlock();
            ButtonVisual.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
            ButtonVisual.SetPropertyBlock(block);
        }
    }
}
