using UnityEngine;

namespace HellSlice
{
    // Stage 1 reuses the existing core, carry movement, receptacle and gate.
    public sealed class SliceChaseStage : MonoBehaviour
    {
        public GameObject Gate;
        public BoxCollider EscapeWall;
        public Transform WallVisual;
        [Min(.1f)] public float WallDropSeconds = .6f;
        public bool WallTriggered { get; private set; }
        public bool WallClosed { get; private set; }
        Vector3 raisedWall;
        float wallTime;
        Vector3 pickupPosition;
        void Start() { raisedWall = WallVisual.localPosition; }
        public string Objective => SliceSession.Current.PiecePlaced
            ? "The gate is open. Escape through the light."
            : SliceSession.Current.Player.Carrying
                ? "Carry the core to the socket. You cannot defeat the guardian yet."
                : "Run forward. Take the core and carry it to the socket!";

        bool Near(SliceSession s, Vector3 target) =>
            Vector3.Distance(s.Player.transform.position, target) < 2.2f &&
            !Physics.Linecast(s.Player.transform.position + Vector3.up, target + Vector3.up,
                SliceWorld.SolidMask, QueryTriggerInteraction.Ignore);

        public string HandleInteraction(SliceSession s)
        {
            if (s.PiecePlaced) return "";
            var player = s.Player;
            var world = s.World;
            if (player.Carrying)
            {
                world.Piece.transform.position = player.transform.position + Vector3.up * 1.1f + player.transform.forward * .65f;
                bool socket = Near(s, world.SocketPosition);
                if (s.Input.Interact)
                {
                    player.Carrying = false;
                    if (socket)
                    {
                        world.Piece.transform.position = world.SocketPosition + Vector3.up * .65f;
                        s.PlacePiece(); Gate.SetActive(false);
                        s.Notify("The gate is open. Escape!");
                    }
                    else
                    {
                        // Safe put-back preserves the existing carry interaction; the chase does not reset.
                        world.Piece.transform.position = pickupPosition;
                    }
                }
                return socket ? "E / Place core and open gate" : "E / Return core to its starting place";
            }
            if (!Near(s, world.Piece.transform.position - Vector3.up * .65f)) return "";
            if (s.Input.Interact && !player.Dodging)
            {
                pickupPosition = world.Piece.transform.position;
                player.Carrying = true;
                s.Notify("Carry the core to the socket. Keep moving!");
            }
            return "E / Take core";
        }

        public void Tick(SliceSession s)
        {
            s.Enemy.BeginChase(); // First running frame, independent of core pickup.
            if (!WallTriggered && s.PiecePlaced && s.World.ExitContains(s.Player.transform.position))
            {
                // The entire player capsule must be beyond the authored wall's far face.
                Bounds closed = new Bounds(EscapeWall.transform.TransformPoint(EscapeWall.center),
                    Vector3.Scale(EscapeWall.size, EscapeWall.transform.lossyScale));
                if (s.Player.Body.bounds.min.z > closed.max.z + .15f)
                {
                    WallTriggered = true;
                    EscapeWall.enabled = true; // Seal safely before animating: no retreat/crush race.
                    s.Enemy.StopChase();
                }
            }
            if (!WallTriggered) return;
            wallTime += s.Clock.WorldDelta;
            float fraction = Mathf.Clamp01(wallTime / WallDropSeconds);
            WallVisual.localPosition = Vector3.Lerp(raisedWall, Vector3.zero, fraction * fraction);
            WallClosed = fraction >= 1;
            if (!WallClosed || s.StageCompleted) return;
            s.Player.SetReveal((s.EarlierCompletedStages + 1) / 7f, false);
            s.CompleteStage();
        }
    }
}
