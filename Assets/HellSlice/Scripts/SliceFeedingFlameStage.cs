using System.Collections.Generic;
using UnityEngine;

namespace HellSlice
{
    public sealed class SliceFeedingFlameStage : MonoBehaviour
    {
        public SliceEnemy Entity;
        public BoxCollider[] Pillars = new BoxCollider[4];
        public GameObject CentralFire, ExitGate, ExitRoute;
        [Min(.1f), Tooltip("World-time seconds from projectile strike to pillar contact. Q also slows the fall.")]
        public float PillarFallDuration = 1.1f;
        readonly HashSet<BoxCollider> broken = new HashSet<BoxCollider>();
        readonly HashSet<BoxCollider> impacted = new HashSet<BoxCollider>();
        readonly List<FallingPillar> falling = new List<FallingPillar>();
        sealed class FallingPillar
        {
            public BoxCollider Pillar;
            public CharacterController Body;
            public Vector3 StartBase, EndBase;
            public Quaternion StartRotation, EndRotation;
            public float HalfHeight, Elapsed;
        }
        bool complete;
        public int BrokenPillars => broken.Count;
        public string Objective => SliceSession.Current.StageCompleted
            ? "Stage 3 complete / The next chamber is not available yet."
            : complete ? "The entity is defeated. Reach the exit."
            : "Bait shots into pillars (" + impacted.Count + " / 4 impacts). Q / Slow time. Shift / Dodge.";

        // Stage 3 no longer has E-operated cooling controls.
        public string HandleInteraction(SliceSession session) => "";

        public void HitPillar(Collider obstacle)
        {
            var session = SliceSession.Current;
            if (session == null || !session.Running || complete || Entity == null) return;
            foreach (var pillar in Pillars)
            {
                if (pillar == null || pillar != obstacle || !pillar.gameObject.activeInHierarchy || !broken.Add(pillar)) continue;
                Vector3 centre = pillar.transform.TransformPoint(pillar.center);
                Vector3 direction = Entity.transform.position - centre;
                direction.y = 0;
                direction = direction.sqrMagnitude > .001f ? direction.normalized : Vector3.forward;
                float halfHeight = pillar.size.y * Mathf.Abs(pillar.transform.lossyScale.y) * .5f;
                // Tip and slide toward the entity: corner pillars cannot reach it by tipping in place.
                // The final leading end overlaps its collider surface slightly, making contact deterministic.
                var body = Entity.GetComponent<CharacterController>();
                Vector3 target = body.ClosestPoint(new Vector3(centre.x, body.bounds.center.y, centre.z));
                Quaternion endRotation = Quaternion.FromToRotation(pillar.transform.up, direction) * pillar.transform.rotation;
                falling.Add(new FallingPillar {
                    Pillar = pillar, Body = body, HalfHeight = halfHeight,
                    StartBase = centre - pillar.transform.up * halfHeight,
                    EndBase = target + direction * .05f - direction * halfHeight * 2,
                    StartRotation = pillar.transform.rotation, EndRotation = endRotation
                });
                // ComputePenetration needs an enabled collider. A trigger stays queryable
                // without blocking movement or intercepting the trigger-ignoring fireballs.
                pillar.isTrigger = true;
                return;
            }
        }

        public void Tick(SliceSession session)
        {
            if (!session.Running) return;
            for (int i = falling.Count - 1; i >= 0; i--)
            {
                var fall = falling[i];
                fall.Elapsed += session.Clock.WorldDelta;
                float t = Mathf.Clamp01(fall.Elapsed / Mathf.Max(.1f, PillarFallDuration));
                float eased = t * t * (3f - 2f * t);
                var pose = fall.Pillar.transform;
                pose.rotation = Quaternion.Slerp(fall.StartRotation, fall.EndRotation, eased);
                Vector3 centre = Vector3.Lerp(fall.StartBase, fall.EndBase, eased) + pose.up * fall.HalfHeight;
                pose.position += centre - pose.TransformPoint(fall.Pillar.center);
                // Query the enabled trigger explicitly; it does not block movement during flight.
                // A timer alone cannot damage the entity: the pillar must reach its actual collider.
                if (!Physics.ComputePenetration(fall.Pillar, pose.position, pose.rotation,
                    fall.Body, fall.Body.transform.position, fall.Body.transform.rotation, out _, out _)) continue;
                if (impacted.Add(fall.Pillar)) Entity.PillarHit(this);
                fall.Pillar.gameObject.SetActive(false);
                falling.RemoveAt(i);
            }
            if (!complete && impacted.Count == 4 && Entity.Defeated)
            {
                complete = true;
                foreach (var projectile in FindObjectsByType<SliceFireball>(FindObjectsSortMode.None))
                    projectile.ClearForEncounter(this);
                CentralFire.SetActive(false);
                ExitGate.SetActive(false);
                if (ExitRoute != null) ExitRoute.SetActive(true);
                session.Player.SetReveal((session.EarlierCompletedStages + 1) / 7f, false);
            }
            if (complete && session.World.ExitContains(session.Player.transform.position)) session.CompleteStage();
        }
    }
}
