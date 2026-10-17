using UnityEngine;

namespace HellSlice
{
    public sealed class SlicePlayer : MonoBehaviour
    {
        [field: SerializeField] public CharacterController Body { get; private set; }
        [Min(.1f)] public float WalkSpeed = 4.3f, RunSpeed = 6f, CarrySpeed = 2.5f;
        public bool Carrying { get; set; }
        public int Health { get; private set; } = 5;
        public bool Dodging => dodgeLeft > 0;
        public float DodgeCooldown => dodgeCooldown;
        public float AttackFlash => attackFlash;
        public Vector3 ActivePosition => transform.position;
        public Renderer[] BodyRenderers;
        float dodgeLeft, dodgeCooldown, attackCooldown, immunity, attackFlash, gravity;
        Vector3 dodgeDirection;
        float reveal, targetReveal;
        /// <summary>Current 0..1 human appearance, read by visual layers.</summary>
        public float Reveal => reveal;
        bool revealing;
        readonly Color dark = new Color(.035f, .05f, .08f);
        readonly Color revealed = new Color(.29f, .38f, .47f);

        public void Configure(CharacterController body) { Body = body; }
        void Update()
        {
            var s = SliceSession.Current;
            if (s == null || !s.Running) return;
            float dt = Time.unscaledDeltaTime;
            dodgeCooldown = Mathf.Max(0, dodgeCooldown - dt);
            attackCooldown = Mathf.Max(0, attackCooldown - dt);
            immunity = Mathf.Max(0, immunity - dt);
            attackFlash = Mathf.Max(0, attackFlash - dt);
            if (revealing) { reveal = Mathf.MoveTowards(reveal, targetReveal, dt * .6f); ApplyReveal(); }
            Vector2 input = s.Input.Move;
            Vector3 direction = Quaternion.Euler(0, s.CameraRig.Yaw, 0) * new Vector3(input.x, 0, input.y);
            direction = Vector3.ClampMagnitude(direction, 1);
            if (s.Input.Dodge && dodgeCooldown <= 0 && !Carrying)
            {
                dodgeLeft = .28f; dodgeCooldown = .95f;
                dodgeDirection = direction.sqrMagnitude > .01f ? direction.normalized : transform.forward;
                s.Feedback.Cue(180, .07f);
            }
            float speed = Carrying ? CarrySpeed : s.Input.Run ? RunSpeed : WalkSpeed;
            Vector3 velocity = direction * speed;
            if (dodgeLeft > 0) { dodgeLeft -= dt; velocity = dodgeDirection * 10; }
            if (direction.sqrMagnitude > .01f && !Dodging)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), dt * 16);
            gravity = Body.isGrounded ? -2 : Mathf.Max(-20, gravity - 24 * dt);
            velocity.y = gravity;
            Body.Move(velocity * dt);
            if (s.Input.Attack && attackCooldown <= 0 && !Carrying && !Dodging && !s.landing)
            {
                attackCooldown = .48f; attackFlash = .18f; s.Feedback.Cue(120, .08f);
                var enemy = s.Enemy;
                if (enemy != null && !enemy.Defeated)
                {
                    Vector3 delta = enemy.transform.position - transform.position; delta.y = 0;
                    if (delta.magnitude <= 2.5f && Vector3.Dot(transform.forward, delta.normalized) > .15f &&
                        !Physics.Linecast(transform.position + Vector3.up, enemy.transform.position + Vector3.up, SliceWorld.SolidMask, QueryTriggerInteraction.Ignore)) enemy.Hit();
                }
            }
            if (transform.position.y < -3) s.KillPlayer();
        }
        public bool TakeDamage(int amount)
        {
            var s = SliceSession.Current;
            if (s == null || !s.Running || immunity > 0 || Dodging) return false;
            Health = Mathf.Max(0, Health - amount); immunity = 1.1f;
            s.Feedback.Cue(80, .14f);
            s.Notify("You were struck.");
            if (Health == 0) s.KillPlayer();
            return true;
        }
        public void SetReveal(float amount, bool immediate)
        {
            targetReveal = Mathf.Clamp01(amount); revealing = true;
            if (immediate) { reveal = targetReveal; ApplyReveal(); }
        }
        void ApplyReveal()
        {
            if (BodyRenderers == null) return;
            foreach (var r in BodyRenderers) if (r != null) r.sharedMaterial.color = Color.Lerp(dark, revealed, reveal);
        }
    }
}

