using UnityEngine;

namespace HellSlice
{
    public sealed class SliceEnemy : MonoBehaviour
    {
        public bool Defeated => health <= 0;
        public int Health => health;
        public Renderer Visual;
        public Transform WarningRing;
        [Min(.5f)] public float DetectionRange = 4.5f;
        [Min(.5f)] public float GuardRadius = 25f;
        [Tooltip("Optional Stage 3 attack. Leave empty for the existing melee guard.")]
        public SliceFireball FireballPrefab;
        [Tooltip("Assigned only for the stationary four-pillar encounter. Ordinary attacks cannot damage it.")]
        public SliceFeedingFlameStage PillarEncounter;
        [Tooltip("Optional authored replacement visuals; gameplay collider stays on this entity.")]
        public GameObject Appearance;
        [Tooltip("Stage 1 only: invulnerable pursuit with lethal contact. Combat remains available in other stages.")]
        public bool CoreChase;
        [Min(.1f)] public float MoveSpeed = 2f, MeleeWindup = .85f, MeleeRecovery = 1.2f;
        [Min(0)] public float ChaseGraceSeconds = 1.5f;
        public Vector2 ChaseMin = new Vector2(-5.7f, -14f), ChaseMax = new Vector2(5.7f, 13.8f);
        public float ChaseExitZ = 15.2f;
        public bool ChaseStarted { get; private set; }
        public bool ChaseStopped { get; private set; }
        float chaseGrace;
        public Vector3 FireOrigin = new Vector3(0, .8f, 0);
        [Min(.1f)] public float FireWindup = 1.25f, FireRecovery = 2f;
        public string State { get; private set; } = "Hunting";
        int health = 4;
        float timer;
        Vector3 spawn;
        CharacterController controller;
        void Start()
        {
            spawn = transform.position; controller = GetComponent<CharacterController>();
            if (Appearance != null) Visual = Appearance.GetComponentInChildren<Renderer>();
            if (CoreChase) { State = "Dormant"; WarningRing.gameObject.SetActive(false); }
        }
        public void BeginChase()
        {
            if (!CoreChase || ChaseStarted) return;
            ChaseStarted = true; chaseGrace = ChaseGraceSeconds; State = "Hunting";
        }
        public void StopChase()
        {
            if (!CoreChase) return;
            ChaseStopped = true; State = "Stopped"; WarningRing.gameObject.SetActive(false);
        }
        void Update()
        {
            var s = SliceSession.Current;
            if (s == null || !s.Running || Defeated) return;
            float dt = s.Clock.WorldDelta;
            Vector3 target = s.Player.transform.position;
            if (CoreChase)
            {
                if (s.StageCompleted) StopChase();
                if (!ChaseStarted || ChaseStopped) return;
                if (chaseGrace > 0) { chaseGrace -= dt; return; }
                Pursue(s, dt);
                return;
            }
            Vector3 delta = target - transform.position; delta.y = 0;
            Vector3 fromSpawn = target - spawn; fromSpawn.y = 0;
            bool inArena = CoreChase || (FireballPrefab != null || (target.z < 14.2f && target.z > -2 && Mathf.Abs(target.x) < 6)) && fromSpawn.magnitude <= GuardRadius;
            if (State == "Windup")
            {
                timer -= dt; WarningRing.gameObject.SetActive(true);
                Visual.sharedMaterial.color = new Color(1f, .2f, .08f);
                if (timer <= 0)
                {
                    if (FireballPrefab != null)
                    {
                        Vector3 origin = transform.TransformPoint(FireOrigin);
                        Vector3 direction = (s.Player.transform.position + Vector3.up - origin).normalized;
                        var fireball = Instantiate(FireballPrefab, origin, Quaternion.identity);
                        fireball.Aim(direction, PillarEncounter);
                    }
                    else if (delta.magnitude < 2.1f && !Physics.Linecast(transform.position + Vector3.up, target + Vector3.up, SliceWorld.SolidMask, QueryTriggerInteraction.Ignore)) s.Player.TakeDamage(1);
                    State = "Recovering"; timer = FireballPrefab != null ? FireRecovery : MeleeRecovery;
                }
            }
            else if (State == "Recovering")
            {
                WarningRing.gameObject.SetActive(false); timer -= dt;
                Visual.sharedMaterial.color = new Color(.25f, .12f, .15f);
                if (timer <= 0) State = "Hunting";
            }
            else
            {
                WarningRing.gameObject.SetActive(false);
                Visual.sharedMaterial.color = new Color(.35f, .08f, .10f);
                if (inArena && delta.magnitude < (FireballPrefab != null ? DetectionRange : 1.65f))
                {
                    State = "Windup"; timer = FireballPrefab != null ? FireWindup : MeleeWindup;
                    if (FireballPrefab != null && delta.sqrMagnitude > .01f)
                    {
                        transform.forward = delta.normalized;
                        if (Appearance != null) Appearance.transform.rotation = transform.rotation;
                    }
                    s.Feedback.Cue(140, .1f);
                }
                else if (PillarEncounter == null)
                {
                    Vector3 goal = CoreChase || inArena && delta.magnitude < DetectionRange ? target : spawn;
                    Vector3 dir = goal - transform.position; dir.y = 0;
                    if (dir.magnitude > .15f)
                    {
                        Vector3 step = dir.normalized * (CoreChase ? Mathf.Min(MoveSpeed * dt, dir.magnitude) : MoveSpeed * dt);
                        if (CoreChase)
                        {
                            Vector3 next = transform.position + step;
                            next.x = Mathf.Clamp(next.x, ChaseMin.x, ChaseMax.x);
                            next.z = Mathf.Clamp(next.z, ChaseMin.y, ChaseMax.y);
                            step = next - transform.position;
                        }
                        controller.Move(step + Vector3.down * 2 * dt); transform.forward = dir.normalized;
                    }
                }
            }
        }
        void Pursue(SliceSession s, float dt)
        {
            State = "Pursuing";
            WarningRing.gameObject.SetActive(false);
            if (Caught(s)) { s.KillPlayer(); return; }
            Vector3 direction = s.Player.transform.position - transform.position; direction.y = 0;
            if (direction.sqrMagnitude > .001f)
            {
                Vector3 next = transform.position + direction.normalized * Mathf.Min(MoveSpeed * dt, direction.magnitude);
                next.x = Mathf.Clamp(next.x, ChaseMin.x, ChaseMax.x);
                next.z = Mathf.Clamp(next.z, ChaseMin.y, ChaseMax.y);
                controller.Move(next - transform.position + Vector3.down * 2 * dt);
                transform.forward = direction.normalized;
            }
            if (Caught(s)) s.KillPlayer();
        }
        bool Caught(SliceSession s)
        {
            var enemyBounds = controller.bounds;
            var playerBounds = s.Player.Body.bounds;
            if (playerBounds.min.y > enemyBounds.max.y || playerBounds.max.y < enemyBounds.min.y) return false;
            Vector3 delta = playerBounds.center - enemyBounds.center; delta.y = 0;
            float reach = Mathf.Max(enemyBounds.extents.x, enemyBounds.extents.z) +
                Mathf.Max(playerBounds.extents.x, playerBounds.extents.z) + .12f;
            return delta.sqrMagnitude <= reach * reach && !Physics.Linecast(
                enemyBounds.center, playerBounds.center, SliceWorld.SolidMask, QueryTriggerInteraction.Ignore);
        }
        public void Hit()
        {
            if (Defeated || CoreChase) return;
            if (PillarEncounter != null) return;
            LoseStrength();
        }
        public void PillarHit(SliceFeedingFlameStage encounter)
        {
            if (Defeated || CoreChase || encounter == null || encounter != PillarEncounter) return;
            LoseStrength();
        }
        void LoseStrength()
        {
            health--;
            SliceSession.Current.Feedback.Cue(200, .1f);
            if (health == 0)
            {
                WarningRing.gameObject.SetActive(false);
                GetComponent<CharacterController>().enabled = false;
                foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
                if (Appearance != null) Appearance.SetActive(false);
                SliceSession.Current.Notify(FireballPrefab != null ? "The fireball guard fades." : "The manifestation fades. The core is free; the exit is still closed.", 5);
            }
        }
    }
}

