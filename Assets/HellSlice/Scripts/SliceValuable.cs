using UnityEngine;

namespace HellSlice
{
    public sealed class SliceValuable : MonoBehaviour
    {
        public string DisplayName = "valuable";
        public Transform Receptacle, Placement;
        public GameObject RestoredRoute;
        public SliceValuable Prerequisite;
        [Min(.5f)] public float InteractionRange = 1.65f;
        public bool Returned { get; private set; }
        public bool Lost { get; private set; }
        public bool Available => !Returned && !Lost && (Prerequisite == null || Prerequisite.Returned);
        bool falling;
        float fallSpeed;
        Renderer visual;
        SliceLava[] lava;

        void Awake()
        {
            visual = GetComponentInChildren<Renderer>();
            lava = FindObjectsByType<SliceLava>(FindObjectsSortMode.None);
        }
        public void PickUp() { falling = false; fallSpeed = 0; }
        public void Drop() { falling = true; fallSpeed = 0; }

        void Update()
        {
            var session = SliceSession.Current;
            if (!falling || Lost || Returned || session == null || !session.Running) return;
            float dt = Time.deltaTime;
            fallSpeed += 24f * dt;
            float distance = fallSpeed * dt;
            var bounds = visual != null ? visual.bounds : new Bounds(transform.position, Vector3.one * .4f);
            // Keep the release position's X/Z; settle vertically on solid ground.
            if (Physics.Raycast(bounds.center, Vector3.down, out var hit, distance + bounds.extents.y,
                SliceWorld.SolidMask, QueryTriggerInteraction.Ignore))
            {
                distance = Mathf.Max(0, hit.distance - bounds.extents.y);
                falling = false;
            }
            var swept = bounds;
            swept.Encapsulate(new Bounds(bounds.center + Vector3.down * distance, bounds.size));
            transform.position += Vector3.down * distance;
            foreach (var hazard in lava)
            {
                if (hazard == null || !hazard.isActiveAndEnabled || hazard.Volume == null || !hazard.Volume.enabled) continue;
                if (!swept.Intersects(hazard.Volume.bounds)) continue;
                Lose();
                return;
            }
            if (transform.position.y < -30f) Lose();
        }

        void Lose()
        {
            Lost = true;
            falling = false;
            gameObject.SetActive(false);
        }
        public bool ReturnToPlace()
        {
            if (!Available || Receptacle == null || Placement == null) return false;
            Returned = true;
            falling = false;
            transform.SetPositionAndRotation(Placement.position, Placement.rotation);
            if (RestoredRoute != null) RestoredRoute.SetActive(true);
            return true;
        }
    }
}
