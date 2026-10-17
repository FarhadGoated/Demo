using UnityEngine;

namespace HellSlice
{
    public sealed class SliceInteraction : MonoBehaviour
    {
        public string Prompt { get; private set; }
        Vector3 pickupPosition;
        bool Near(Vector3 target, float range = 2.2f)
        {
            var p = SliceSession.Current.Player.transform.position;
            return Vector3.Distance(p, target) < range && !Physics.Linecast(p + Vector3.up, target + Vector3.up, SliceWorld.SolidMask, QueryTriggerInteraction.Ignore);
        }
        void Update()
        {
            var s = SliceSession.Current; Prompt = "";
            if (s == null || !s.Running) return;
            if (s.BorrowedTime != null) { Prompt = s.BorrowedTime.HandleInteraction(s); return; }
            if (s.Possession != null) { Prompt = s.Possession.HandleInteraction(s); return; }
            if (s.FeedingFlame != null) { Prompt = s.FeedingFlame.HandleInteraction(s); return; }
            if (s.ShadowTruth != null) { Prompt = s.ShadowTruth.HandleInteraction(s); return; }
            if (s.Pride != null) { Prompt = s.Pride.HandleInteraction(s); return; }
            if (s.Chase != null) { Prompt = s.Chase.HandleInteraction(s); return; }
            if (s.landing) return;
            var w = s.World; var p = s.Player;
            if (p.Carrying)
            {
                w.Piece.transform.position = p.transform.position + Vector3.up * 1.1f + p.transform.forward * .65f;
                bool socket = Near(w.SocketPosition);
                Prompt = socket ? "E / Place ember core" : "E / Put core back safely";
                if (s.Input.Interact)
                {
                    p.Carrying = false;
                    if (socket) { w.Piece.transform.position = w.SocketPosition + Vector3.up * .65f; s.PlacePiece(); }
                    else w.Piece.transform.position = pickupPosition;
                }
            }
            else if (!s.PiecePlaced && Near(w.Piece.transform.position - Vector3.up * .65f))
            {
                if (s.Enemy != null && !s.Enemy.Defeated) Prompt = "The manifestation holds the core";
                else
                {
                    Prompt = "E / Carry ember core";
                    if (s.Input.Interact) { pickupPosition = w.Piece.transform.position; p.Carrying = true; s.Feedback.Cue(400); }
                }
            }
            else if (!s.Assisted && Near(w.FigurePosition))
            {
                Prompt = s.SwitchLatched ? "E / Help the trapped figure" : "Operate the rescue mechanisms first";
                if (s.Input.Interact && s.SwitchLatched) s.Assist();
            }
            else if (!s.PiecePlaced && Near(w.SocketPosition)) Prompt = "Bring the released core here";
            else
            {
                foreach (var mechanism in w.PuzzleSwitches)
                {
                    if (mechanism == null || !mechanism.CanInteract(p)) continue;
                    Prompt = "E / " + mechanism.InteractionLabel;
                    if (s.Input.Interact) mechanism.Interact(p);
                    break;
                }
            }
        }
    }
}

