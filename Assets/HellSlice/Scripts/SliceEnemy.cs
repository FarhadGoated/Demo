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
        [Tooltip("Stage 7 only: the Stage 1 guardian returns. It pursues inside ChaseMin/ChaseMax, keeps the melee strike, telegraphs a straight charge and is weakened only by this encounter's mechanisms. Ordinary attacks cannot damage it.")]
        public SliceReturnStage ChargeEncounter;
        [Min(.5f)] public float ChargeRange = 10f, ChargeMinRange = 3f;
        [Min(.1f), Tooltip("World-time charge telegraph. It keeps turning toward the player until ChargeLock seconds remain.")]
        public float ChargeWindup = 1.2f;
        [Min(0)] public float ChargeLock = .45f;
        [Min(.5f), Tooltip("Keep the ChargeLane strip as long as ChargeDistance.")]
        public float ChargeSpeed = 12f, ChargeDistance = 14f;
        [Min(.1f)] public float ChargeRecovery = 1.2f, ChargeCooldown = 1.5f, SealStagger = 2f, DissolveSeconds = 2f;
        [Min(0)] public int ChargeDamage = 2;
        [Tooltip("Saved inactive. Flat strip along the charge line, shown during the charge telegraph.")]
        public GameObject ChargeLane;
        public bool Dissolved => State == "Dissolved";
        public string State { get; private set; } = "Hunting";
        int health = 4;
        float timer;
        Vector3 spawn;
        CharacterController controller;
        int strength;
        bool passedPlayer;
        float chargeCooldown, charged;
        Vector3 chargeDirection, appearanceScale, appearanceHome, dissolveScale;
        Color baseColour = Color.white;
        static readonly Color telegraphColour = new Color(1f, .2f, .08f), stunColour = new Color(.45f, .5f, .62f);
        void Start()
        {
            spawn = transform.position; controller = GetComponent<CharacterController>();
            if (Appearance != null) Visual = Appearance.GetComponentInChildren<Renderer>();
            if (CoreChase) { State = "Dormant"; WarningRing.gameObject.SetActive(false); }
            if (ChargeEncounter != null)
            {
                // One strength per mechanism, so each mechanism weakens it exactly once.
                State = "Dormant"; WarningRing.gameObject.SetActive(false);
                health = strength = Mathf.Max(1, ChargeEncounter.MechanismCount);
                if (Visual != null) baseColour = Visual.sharedMaterial.color;
                if (Appearance != null) { appearanceScale = Appearance.transform.localScale; appearanceHome = Appearance.transform.localPosition; }
            }
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
            if (s == null || !s.Running) return;
            if (ChargeEncounter != null) { ChargeUpdate(s, s.Clock.WorldDelta); return; }
            if (Defeated) return;
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
            if (PillarEncounter != null || ChargeEncounter != null) return;
            LoseStrength();
        }
        // Stage 7: pursue -> (close) existing melee strike, or (mid range) charge telegraph -> straight
        // charge -> recover. Mechanisms stun/weaken it; the last one starts the dissolve. World time.
        void ChargeUpdate(SliceSession s, float dt)
        {
            if (State == "Dissolving") { Dissolve(dt); return; }
            if (Defeated) return;
            var player = s.Player;
            Vector3 delta = player.transform.position - transform.position; delta.y = 0;
            float distance = delta.magnitude;
            switch (State)
            {
                case "Dormant":
                    // Waits until the player steps into its arena, then the existing grace applies.
                    Vector3 p = player.transform.position;
                    if (p.x < ChaseMin.x || p.x > ChaseMax.x || p.z < ChaseMin.y || p.z > ChaseMax.y) return;
                    State = "Hunting"; chaseGrace = ChaseGraceSeconds;
                    return;
                case "Stunned":
                case "Recovering":
                    timer -= dt;
                    if (timer <= 0) { State = "Hunting"; Tint(baseColour); }
                    return;
                case "Windup":
                    timer -= dt;
                    if (timer > 0) return;
                    if (distance < 2.1f && Clear(player)) player.TakeDamage(1);
                    Recover(MeleeRecovery);
                    return;
                case "ChargeWindup":
                    timer -= dt;
                    if (timer > ChargeLock) Face(delta);
                    if (timer > 0) return;
                    State = "Charging"; charged = 0; chargeDirection = transform.forward;
                    if (ChargeLane != null) ChargeLane.SetActive(false);
                    s.Feedback.Cue(90, .2f);
                    return;
                case "Charging":
                    Charge(s, dt);
                    return;
            }
            if (chaseGrace > 0) { chaseGrace -= dt; return; }
            chargeCooldown = Mathf.Max(0, chargeCooldown - dt);
            if (distance < 1.65f) { Telegraph("Windup", MeleeWindup, false); s.Feedback.Cue(140, .1f); return; }
            if (chargeCooldown <= 0 && distance >= ChargeMinRange && distance <= ChargeRange && Clear(player))
            {
                Face(delta); Telegraph("ChargeWindup", ChargeWindup, true); s.Feedback.Cue(110, .25f);
                return;
            }
            if (distance > .15f) { Step(delta / distance * Mathf.Min(MoveSpeed * dt, distance), dt); Face(delta); }
        }
        void Charge(SliceSession s, float dt)
        {
            float step = Mathf.Min(ChargeSpeed * dt, ChargeDistance - charged);
            Vector3 before = transform.position;
            if (step > 0) Step(chargeDirection * step, dt);
            charged += step;
            Vector3 moved = transform.position - before; moved.y = 0;
            var seal = ChargeEncounter.SealHitBy(controller.bounds);
            bool hitPlayer = Touching(s.Player);
            if (hitPlayer && (passedPlayer || s.Player.Dodging))
            {
                // A dodge's immunity lets the charge carry on through the player toward the seal.
                IgnorePlayer(s, true);
                hitPlayer = false;
            }
            if (hitPlayer) s.Player.TakeDamage(ChargeDamage);
            if (seal != null) ChargeEncounter.BreakSeal(seal);
            // Stops on a seal, the player, a wall, the arena edge or after its full distance.
            if (seal != null || hitPlayer || charged >= ChargeDistance || moved.magnitude < step * .3f)
            {
                chargeCooldown = ChargeCooldown;
                Recover(seal != null ? SealStagger : ChargeRecovery);
            }
        }
        public bool Weaken(SliceReturnStage encounter, float stunSeconds)
        {
            if (Defeated || encounter == null || encounter != ChargeEncounter) return false;
            health--;
            IgnorePlayer(SliceSession.Current, false);
            SliceSession.Current.Feedback.Cue(200, .1f);
            WarningRing.gameObject.SetActive(false);
            if (ChargeLane != null) ChargeLane.SetActive(false);
            // Visibly smaller after each mechanism; gameplay collider unchanged.
            if (Appearance != null) Appearance.transform.localScale = appearanceScale * (1f - .12f * (strength - health));
            Tint(stunColour);
            if (health > 0) { State = "Stunned"; timer = stunSeconds; return true; }
            State = "Dissolving"; timer = DissolveSeconds;
            if (Appearance != null) dissolveScale = Appearance.transform.localScale;
            controller.enabled = false;
            return true;
        }
        void Dissolve(float dt)
        {
            timer -= dt;
            float t = 1 - Mathf.Clamp01(timer / Mathf.Max(.1f, DissolveSeconds));
            if (Appearance != null)
            {
                Appearance.transform.localScale = dissolveScale * Mathf.Lerp(1f, .05f, t * t);
                Appearance.transform.localPosition = appearanceHome + Vector3.down * (1.5f * t);
            }
            Tint(Color.Lerp(stunColour, Color.black, t));
            if (timer > 0) return;
            State = "Dissolved";
            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
            if (Appearance != null) Appearance.SetActive(false);
        }
        void Telegraph(string state, float seconds, bool lane)
        {
            State = state; timer = seconds;
            WarningRing.gameObject.SetActive(true);
            if (ChargeLane != null) ChargeLane.SetActive(lane);
            Tint(telegraphColour);
        }
        void Recover(float seconds)
        {
            State = "Recovering"; timer = seconds;
            IgnorePlayer(SliceSession.Current, false);
            WarningRing.gameObject.SetActive(false);
            if (ChargeLane != null) ChargeLane.SetActive(false);
            Tint(Color.Lerp(baseColour, Color.black, .35f));
        }
        // Stays inside ChaseMin/ChaseMax and never steps off solid floor, so it can always reach every seal.
        void Step(Vector3 move, float dt)
        {
            Vector3 next = transform.position + move;
            next.x = Mathf.Clamp(next.x, ChaseMin.x, ChaseMax.x);
            next.z = Mathf.Clamp(next.z, ChaseMin.y, ChaseMax.y);
            Vector3 step = next - transform.position; step.y = 0;
            if (step.sqrMagnitude < 1e-8f || !Physics.Raycast(next + Vector3.up * .5f, Vector3.down, 1.5f,
                SliceWorld.SolidMask, QueryTriggerInteraction.Ignore)) return;
            controller.Move(step + Vector3.down * 2 * dt);
        }
        bool Touching(SlicePlayer player)
        {
            var enemyBounds = controller.bounds;
            var playerBounds = player.Body.bounds;
            if (playerBounds.min.y > enemyBounds.max.y || playerBounds.max.y < enemyBounds.min.y) return false;
            Vector3 delta = player.transform.position - transform.position; delta.y = 0;
            // Both skins (0.08 each) can keep blocked capsules ~0.16 m apart; the margin must exceed that.
            float reach = controller.radius * transform.lossyScale.x + player.Body.radius * player.transform.lossyScale.x + .3f;
            return delta.sqrMagnitude <= reach * reach;
        }
        void IgnorePlayer(SliceSession s, bool ignore)
        {
            if (passedPlayer == ignore || s == null || s.Player == null) return;
            passedPlayer = ignore;
            if (controller.enabled && s.Player.Body.enabled) Physics.IgnoreCollision(controller, s.Player.Body, ignore);
        }
        bool Clear(SlicePlayer player) => !Physics.Linecast(transform.position + Vector3.up, player.transform.position + Vector3.up,
            SliceWorld.SolidMask, QueryTriggerInteraction.Ignore);
        void Face(Vector3 delta) { if (delta.sqrMagnitude > .001f) transform.forward = delta.normalized; }
        void Tint(Color colour) { if (Visual != null) Visual.sharedMaterial.color = colour; }
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

