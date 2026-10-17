using System.Collections.Generic;
using UnityEngine;

namespace HellSlice.Polish
{
    /// <summary>
    /// Visual-only layer for the player or an enemy. Hides the placeholder primitives, spawns an animated
    /// model in their place and drives its Animator and shadow-reveal material from existing gameplay state.
    /// Gameplay scripts stay authoritative; deleting this component restores the original look.
    /// </summary>
    [DefaultExecutionOrder(200)]
    public sealed class PolishSkin : MonoBehaviour
    {
        public enum Kind { Player, Enemy }
        public Kind Role;
        public GameObject Model;
        public RuntimeAnimatorController Controller;
        public Material RevealTemplate;
        [Tooltip("Optional colour atlas applied to every model material (used when the import did not link textures).")]
        public Texture OverrideBaseMap;
        [Min(.2f)] public float TargetHeight = 1.8f;
        public float YawOffset;
        [Tooltip("Enemy only: constant reveal value. Below 0 keeps the shadow body; 1 shows full colours.")]
        public float EnemyReveal = -1;
        public Color EnemyRim = new Color(1f, .22f, .06f);

        SlicePlayer player;
        SliceEnemy enemy;
        GameObject instance;
        Animator animator;
        readonly List<Material> materials = new List<Material>();
        readonly List<Renderer> modelRenderers = new List<Renderer>();
        Vector3 lastPosition;
        int lastHealth;
        float lastAttackFlash, hitFlash, deathTimer = -1, airTime;
        bool placeholder;
        int carryLayer = -1;
        string lastState;
        static readonly int SpeedId = Animator.StringToHash("Speed"), AttackId = Animator.StringToHash("Attack"),
            DodgeId = Animator.StringToHash("Dodge"), HitId = Animator.StringToHash("Hit"), DeadId = Animator.StringToHash("Dead"),
            CarryId = Animator.StringToHash("Carry"), GroundedId = Animator.StringToHash("Grounded");
        const string DefaultPlayerController = "PolishPlayerAnimator"; // Polish/Resources, used when Controller is empty
        static readonly int RevealId = Shader.PropertyToID("_Reveal"), HitPropId = Shader.PropertyToID("_Hit");

        void Awake()
        {
            player = GetComponent<SlicePlayer>();
            enemy = GetComponent<SliceEnemy>();
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                if (!(r is LineRenderer)) r.enabled = false; // keep warning rings, hide placeholder body
            if (Model == null) { placeholder = true; RestylePlaceholders(); return; }

            instance = Instantiate(Model, transform);
            instance.name = "Visual model";
            instance.transform.localRotation = Quaternion.Euler(0, YawOffset, 0);
            PolishBoneFollow.Attach(instance); // before any animation: offsets come from the default pose
            FitHeight();
            foreach (var r in instance.GetComponentsInChildren<Renderer>(true))
            {
                modelRenderers.Add(r);
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                if (r is SkinnedMeshRenderer smr) smr.updateWhenOffscreen = true;
                if (RevealTemplate == null) continue;
                var slots = r.sharedMaterials;
                Bounds b = r is SkinnedMeshRenderer s && s.sharedMesh != null ? s.sharedMesh.bounds : r is MeshRenderer ? r.GetComponent<MeshFilter>().sharedMesh.bounds : new Bounds(Vector3.up, Vector3.one * 2);
                for (int i = 0; i < slots.Length; i++)
                {
                    var m = new Material(RevealTemplate) { name = "Reveal " + (slots[i] != null ? slots[i].name : i.ToString()) };
                    if (slots[i] != null)
                    {
                        if (slots[i].HasProperty("_BaseColor")) m.SetColor("_BaseColor", slots[i].GetColor("_BaseColor"));
                        else if (slots[i].HasProperty("_Color")) m.SetColor("_BaseColor", slots[i].GetColor("_Color"));
                        Texture tex = slots[i].HasProperty("_BaseMap") ? slots[i].GetTexture("_BaseMap") : slots[i].HasProperty("_MainTex") ? slots[i].GetTexture("_MainTex") : null;
                        if (tex != null) m.SetTexture("_BaseMap", tex);
                    }
                    if (OverrideBaseMap != null) { m.SetTexture("_BaseMap", OverrideBaseMap); m.SetColor("_BaseColor", Color.white); }
                    m.SetFloat("_Height", b.size.y); m.SetFloat("_BaseY", b.min.y);
                    if (Role == Kind.Enemy) m.SetColor("_RimColor", EnemyRim * 1.4f);
                    slots[i] = m; materials.Add(m);
                }
                r.sharedMaterials = slots;
            }
            animator = instance.GetComponentInChildren<Animator>();
            if (animator == null) animator = instance.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            if (Controller == null && Role == Kind.Player) Controller = Resources.Load<RuntimeAnimatorController>(DefaultPlayerController);
            if (Controller != null) animator.runtimeAnimatorController = Controller;
            carryLayer = animator.runtimeAnimatorController != null ? animator.GetLayerIndex("Carry") : -1;
            lastPosition = transform.position;
            lastHealth = player != null ? player.Health : enemy != null ? enemy.Health : 0;
            ApplyReveal(true);
        }

        void FitHeight()
        {
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;
            instance.transform.localScale = Vector3.one;
            Bounds b = renderers[0].bounds; foreach (var r in renderers) b.Encapsulate(r.bounds);
            float scale = b.size.y > .001f ? TargetHeight / b.size.y : 1;
            instance.transform.localScale = Vector3.one * scale;
            b = renderers[0].bounds; foreach (var r in renderers) b.Encapsulate(r.bounds);
            instance.transform.position += Vector3.up * (transform.position.y - b.min.y);
        }

        void RestylePlaceholders()
        {
            // No model supplied: keep the capsule figure but give it the shadow-body look.
            foreach (var r in GetComponentsInChildren<MeshRenderer>(true))
            {
                if (RevealTemplate == null) { r.enabled = true; continue; }
                var m = new Material(RevealTemplate);
                m.SetFloat("_Reveal", 0); m.SetColor("_RimColor", (Role == Kind.Enemy ? EnemyRim : new Color(.35f, .45f, .95f)) * 1.6f);
                r.sharedMaterial = m; r.enabled = true; materials.Add(m); modelRenderers.Add(r);
            }
        }

        void Update()
        {
            var s = SliceSession.Current;
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0) return;
            float worldDt = s != null && s.Clock != null ? s.Clock.WorldDelta : dt;
            if (animator != null && Role == Kind.Enemy) animator.speed = dt > 0 ? Mathf.Clamp01(worldDt / dt) : 1; // slow time slows the enemy too

            Vector3 delta = transform.position - lastPosition; delta.y = 0;
            float speed = delta.magnitude / dt; lastPosition = transform.position;
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetFloat(SpeedId, speed, .08f, dt);
                if (player != null)
                {
                    if (player.AttackFlash > lastAttackFlash + .05f) animator.SetTrigger(AttackId);
                    animator.SetBool(DodgeId, player.Dodging);
                    animator.SetBool(CarryId, player.Carrying);
                    if (carryLayer > 0) animator.SetLayerWeight(carryLayer, Mathf.MoveTowards(animator.GetLayerWeight(carryLayer), player.Carrying ? 1 : 0, dt * 5));
                    // Short grace so CharacterController ground flicker on steps does not flash the air pose.
                    airTime = player.Body != null && !player.Body.isGrounded ? airTime + dt : 0;
                    animator.SetBool(GroundedId, airTime < .12f);
                }
                if (enemy != null)
                {
                    if (enemy.State == "Windup" && lastState != "Windup") animator.SetTrigger(AttackId);
                    lastState = enemy.State;
                }
            }
            if (player != null) lastAttackFlash = player.AttackFlash;

            int health = player != null ? player.Health : enemy != null ? enemy.Health : 0;
            if (health < lastHealth) { hitFlash = 1; if (animator != null && animator.runtimeAnimatorController != null && health > 0) animator.SetTrigger(HitId); }
            lastHealth = health;
            hitFlash = Mathf.Max(0, hitFlash - dt * 4);

            if (enemy != null && enemy.Defeated && deathTimer < 0)
            {
                deathTimer = 2.4f;
                foreach (var r in modelRenderers) if (r != null) r.enabled = true; // SliceEnemy hides renderers; play the fall first
                if (animator != null && animator.runtimeAnimatorController != null) animator.SetBool(DeadId, true);
            }
            if (deathTimer > 0)
            {
                deathTimer -= dt;
                if (instance != null && deathTimer < 1) instance.transform.position += Vector3.down * dt * .8f;
                if (deathTimer <= 0) foreach (var r in modelRenderers) if (r != null) r.enabled = false;
            }
            ApplyReveal(false);
        }

        void ApplyReveal(bool immediate)
        {
            float reveal = Role == Kind.Player && player != null ? player.Reveal : placeholder || EnemyReveal < 0 ? 0 : EnemyReveal;
            foreach (var m in materials)
            {
                if (m == null) continue;
                m.SetFloat(RevealId, reveal);
                m.SetFloat(HitPropId, hitFlash);
            }
        }

        void OnDestroy() { foreach (var m in materials) if (m != null) Destroy(m); }
    }
}
