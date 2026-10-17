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
        }
        void Update()
        {
            var s = SliceSession.Current;
            if (s == null || !s.Running || Defeated) return;
            float dt = s.Clock.WorldDelta;
            Vector3 target = s.Player.transform.position;
            Vector3 delta = target - transform.position; delta.y = 0;
            Vector3 fromSpawn = target - spawn; fromSpawn.y = 0;
            bool inArena = (FireballPrefab != null || (target.z < 14.2f && target.z > -2 && Mathf.Abs(target.x) < 6)) && fromSpawn.magnitude <= GuardRadius;
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
                    State = "Recovering"; timer = FireballPrefab != null ? FireRecovery : 1.2f;
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
                    State = "Windup"; timer = FireballPrefab != null ? FireWindup : .85f;
                    if (FireballPrefab != null && delta.sqrMagnitude > .01f)
                    {
                        transform.forward = delta.normalized;
                        if (Appearance != null) Appearance.transform.rotation = transform.rotation;
                    }
                    s.Feedback.Cue(140, .1f);
                }
                else if (PillarEncounter == null)
                {
                    Vector3 goal = inArena && delta.magnitude < DetectionRange ? target : spawn;
                    Vector3 dir = goal - transform.position; dir.y = 0;
                    if (dir.magnitude > .15f) { controller.Move((dir.normalized * 2 + Vector3.down * 2) * dt); transform.forward = dir.normalized; }
                }
            }
        }
        public void Hit()
        {
            if (Defeated) return;
            if (PillarEncounter != null) return;
            LoseStrength();
        }
        public void PillarHit(SliceFeedingFlameStage encounter)
        {
            if (Defeated || encounter == null || encounter != PillarEncounter) return;
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

