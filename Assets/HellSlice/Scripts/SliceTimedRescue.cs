using UnityEngine;

namespace HellSlice
{
    public sealed class SliceTimedRescue : MonoBehaviour
    {
        public string DisplayName = "trapped figure";
        public Transform Figure;
        public GameObject Restraints, EscapeWalkway;
        public Transform[] EscapePath = new Transform[0];
        [Min(.5f)] public float InteractionRange = 1.8f;
        [Min(.1f)] public float EscapeSpeed = 3f;
        public Transform FireFront;
        [Tooltip("Outer start, approach, warning, and contact positions. Visuals only; no fluid/shadow simulation.")]
        public Transform[] FirePositions = new Transform[0];
        public bool Released { get; private set; }
        public bool Escaped { get; private set; }
        int waypoint;

        public bool InReach(SlicePlayer player)
        {
            if (player == null) return false;
            return Vector3.Distance(player.transform.position, transform.position) <= InteractionRange &&
                !Physics.Linecast(player.transform.position + Vector3.up, transform.position + Vector3.up,
                    SliceWorld.SolidMask, QueryTriggerInteraction.Ignore);
        }

        public bool Release(SliceSession session)
        {
            if (Released || session == null || !session.Running || !InReach(session.Player)) return false;
            Released = true;
            if (Restraints != null) Restraints.SetActive(false);
            if (EscapeWalkway != null) EscapeWalkway.SetActive(true);
            if (FireFront != null) FireFront.gameObject.SetActive(false);
            return true;
        }

        public void AdvanceFire(float fraction)
        {
            if (Released || FireFront == null || FirePositions.Length < 2) return;
            float step = Mathf.Clamp01(fraction) * (FirePositions.Length - 1);
            int index = Mathf.Min(Mathf.FloorToInt(step), FirePositions.Length - 2);
            if (FirePositions[index] != null && FirePositions[index + 1] != null)
                FireFront.position = Vector3.Lerp(FirePositions[index].position, FirePositions[index + 1].position, step - index);
        }

        void Update()
        {
            var session = SliceSession.Current;
            if (!Released || Escaped || session == null || !session.Running || Figure == null) return;
            // Fixed paths lie on the newly opened walkway. No collision, pathfinding or escort state.
            if (waypoint >= EscapePath.Length) { Escaped = true; return; }
            var target = EscapePath[waypoint];
            if (target == null) { waypoint++; return; }
            Figure.position = Vector3.MoveTowards(Figure.position, target.position, EscapeSpeed * Time.unscaledDeltaTime);
            if (Vector3.SqrMagnitude(Figure.position - target.position) < .001f) waypoint++;
        }
    }
}
