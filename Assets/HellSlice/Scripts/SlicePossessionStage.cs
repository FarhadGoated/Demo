using UnityEngine;

namespace HellSlice
{
    // Only the two-return puzzle; movement, input, death and run progress remain shared.
    public sealed class SlicePossessionStage : MonoBehaviour
    {
        public SliceValuable[] Valuables = new SliceValuable[0];
        public GameObject ExitGate;
        public bool RouteComplete { get; private set; }
        SliceValuable carried;
        int returnedCount;
        bool HasLostValuable
        {
            get
            {
                foreach (var item in Valuables) if (item != null && item.Lost) return true;
                return false;
            }
        }
        public string Objective => SliceSession.Current != null && SliceSession.Current.StageCompleted
            ? "Stage 2 complete / The next chamber is not available yet."
            : HasLostValuable ? "A valuable was lost. Restart the chamber to try again."
            : RouteComplete ? "The way is restored. Reach the exit."
            : "Return the valuables to their matching places (" + returnedCount + " / " + Valuables.Length + ").";

        public string HandleInteraction(SliceSession session)
        {
            var player = session.Player;
            if (carried != null)
            {
                carried.transform.position = player.transform.position + Vector3.up * 1.1f + player.transform.forward * .65f;
                bool matching = carried.Receptacle != null && Near(player, carried.Receptacle.position, carried.InteractionRange);
                string prompt = matching ? "E / Return " + carried.DisplayName : "E / Drop " + carried.DisplayName;
                if (session.Input.Interact)
                {
                    if (matching && carried.ReturnToPlace()) returnedCount++;
                    else carried.Drop();
                    carried = null;
                    player.Carrying = false;
                    if (!RouteComplete && returnedCount == Valuables.Length && returnedCount > 0)
                    {
                        RouteComplete = true;
                        if (ExitGate != null) ExitGate.SetActive(false);
                        player.SetReveal((session.EarlierCompletedStages + 1) / 6f, false);
                    }
                }
                return prompt;
            }
            foreach (var item in Valuables)
            {
                if (item == null || !item.Available || !Near(player, item.transform.position, item.InteractionRange)) continue;
                if (session.Input.Interact && !player.Dodging)
                {
                    carried = item;
                    carried.PickUp();
                    player.Carrying = true;
                }
                return "E / Carry " + item.DisplayName;
            }
            return "";
        }

        static bool Near(SlicePlayer player, Vector3 target, float range)
        {
            return Vector3.Distance(player.transform.position, target) <= range &&
                !Physics.Linecast(player.transform.position + Vector3.up, target + Vector3.up,
                    SliceWorld.SolidMask, QueryTriggerInteraction.Ignore);
        }

        public void Tick(SliceSession session)
        {
            if (RouteComplete && session.World.ExitContains(session.Player.transform.position)) session.CompleteStage();
        }
    }
}
