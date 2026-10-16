using UnityEngine;

namespace HellSlice
{
    public sealed class SliceEnemy : MonoBehaviour
    {
        public bool Defeated => health <= 0;
        public int Health => health;
        public Renderer Visual;
        public Transform WarningRing;
        public string State { get; private set; } = "Hunting";
        int health = 4;
        float timer;
        Vector3 spawn;
        CharacterController controller;
        void Start() { spawn = transform.position; controller = GetComponent<CharacterController>(); }
        void Update()
        {
            var s = SliceSession.Current;
            if (s == null || !s.Running || Defeated) return;
            float dt = s.Clock.WorldDelta;
            Vector3 target = s.Player.transform.position;
            Vector3 delta = target - transform.position; delta.y = 0;
            bool inArena = target.z < 14.2f && target.z > -2 && Mathf.Abs(target.x) < 6;
            if (State == "Windup")
            {
                timer -= dt; WarningRing.gameObject.SetActive(true);
                Visual.sharedMaterial.color = new Color(1f, .2f, .08f);
                if (timer <= 0)
                {
                    if (delta.magnitude < 2.1f && !Physics.Linecast(transform.position + Vector3.up, target + Vector3.up, SliceWorld.SolidMask, QueryTriggerInteraction.Ignore)) s.Player.TakeDamage(1);
                    State = "Recovering"; timer = 1.2f;
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
                if (inArena && delta.magnitude < 1.65f) { State = "Windup"; timer = .85f; s.Feedback.Cue(140, .1f); }
                else
                {
                    Vector3 goal = inArena && delta.magnitude < 11 ? target : spawn;
                    Vector3 dir = goal - transform.position; dir.y = 0;
                    if (dir.magnitude > .15f) { controller.Move((dir.normalized * 2 + Vector3.down * 2) * dt); transform.forward = dir.normalized; }
                }
            }
        }
        public void Hit()
        {
            if (Defeated) return;
            health--;
            SliceSession.Current.Feedback.Cue(200, .1f);
            if (health == 0)
            {
                WarningRing.gameObject.SetActive(false);
                GetComponent<CharacterController>().enabled = false;
                foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
                SliceSession.Current.Notify("The manifestation fades. The core is free; the exit is still closed.", 5);
            }
        }
    }
}

