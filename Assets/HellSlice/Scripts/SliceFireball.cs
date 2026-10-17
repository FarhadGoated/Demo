using UnityEngine;

namespace HellSlice
{
    public sealed class SliceFireball : MonoBehaviour
    {
        [Min(.1f)] public float Speed = 6f;
        [Min(.01f)] public float Radius = .22f;
        public int Damage = 1;
        SliceFeedingFlameStage encounter;
        float lifetime;

        public void ClearForEncounter(SliceFeedingFlameStage owner)
        {
            if (encounter != owner) return;
            gameObject.SetActive(false); // Stop damage immediately, before deferred destruction.
            Destroy(gameObject);
        }

        public void Aim(Vector3 direction, SliceFeedingFlameStage owner)
        {
            transform.forward = direction.normalized;
            encounter = owner;
        }

        void Impact(Collider obstacle, SliceSession session)
        {
            if (obstacle.GetComponentInParent<SlicePlayer>() != null) session.Player.TakeDamage(Damage);
            else if (encounter != null) encounter.HitPillar(obstacle);
            Destroy(gameObject);
        }

        void Update()
        {
            var s = SliceSession.Current;
            if (s == null || !s.Running) return;
            if (encounter != null && encounter.Entity.Defeated) { Destroy(gameObject); return; }
            float dt = s.Clock.WorldDelta;
            lifetime += dt;
            float distance = Speed * dt;
            var overlaps = Physics.OverlapSphere(transform.position, Radius,
                SliceWorld.SolidMask | (1 << SliceWorld.BodyLayer), QueryTriggerInteraction.Ignore);
            if (overlaps.Length > 0)
            {
                // No travelling through a collider if the projectile begins inside it.
                Collider nearest = overlaps[0];
                float closest = (nearest.ClosestPoint(transform.position) - transform.position).sqrMagnitude;
                foreach (var obstacle in overlaps)
                {
                    float squared = (obstacle.ClosestPoint(transform.position) - transform.position).sqrMagnitude;
                    if (squared < closest) { closest = squared; nearest = obstacle; }
                }
                Impact(nearest, s);
                return;
            }
            if (Physics.SphereCast(transform.position, Radius, transform.forward, out var hit,
                distance, SliceWorld.SolidMask | (1 << SliceWorld.BodyLayer), QueryTriggerInteraction.Ignore))
            {
                Impact(hit.collider, s);
                return;
            }
            transform.position += transform.forward * distance;
            if (lifetime > 15f) Destroy(gameObject);
        }
    }
}
