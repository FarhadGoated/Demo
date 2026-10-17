using UnityEngine;

namespace HellSlice.Polish
{
    /// <summary>
    /// Visual-only: drives an enemy appearance's humanoid Animator from its parent SliceEnemy state.
    /// Lives on the *-Visual prefab root (assigned as SliceEnemy.Appearance); gameplay is unchanged.
    /// </summary>
    [DefaultExecutionOrder(200)]
    public sealed class PolishEnemyAnimator : MonoBehaviour
    {
        public Animator Animator;
        [Tooltip("Keep the model's own clavicle pose during locomotion; preserve animated arm direction.")]
        public bool UseRestShoulders;

        Transform leftShoulder, rightShoulder, leftArm, rightArm;
        Quaternion leftRest, rightRest;

        void Awake()
        {
            if (!UseRestShoulders) return;
            if (Animator == null) Animator = GetComponentInChildren<Animator>();
            if (Animator == null || !Animator.isHuman) return;
            leftShoulder = Animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            rightShoulder = Animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            leftArm = Animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightArm = Animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            if (leftShoulder != null) leftRest = leftShoulder.localRotation;
            if (rightShoulder != null) rightRest = rightShoulder.localRotation;
        }

        SliceEnemy enemy;
        Vector3 lastPosition;
        int lastHealth;
        string lastState;
        static readonly int SpeedId = Animator.StringToHash("Speed"), AttackId = Animator.StringToHash("Attack"),
            HitId = Animator.StringToHash("Hit"), DeadId = Animator.StringToHash("Dead"),
            StunnedId = Animator.StringToHash("Stunned"), DormantId = Animator.StringToHash("Dormant");

        void Start()
        {
            enemy = GetComponentInParent<SliceEnemy>();
            if (Animator == null) Animator = GetComponentInChildren<Animator>();
            if (Animator != null) { Animator.applyRootMotion = false; Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; }
            if (enemy != null) { lastPosition = enemy.transform.position; lastHealth = enemy.Health; }
        }

        void Update()
        {
            if (enemy == null || Animator == null || Animator.runtimeAnimatorController == null) return;
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0) return;
            var s = SliceSession.Current;
            float worldDt = s != null && s.Clock != null ? s.Clock.WorldDelta : dt;
            Animator.speed = Mathf.Clamp01(worldDt / dt); // slow time slows the enemy too

            Vector3 delta = enemy.transform.position - lastPosition; delta.y = 0;
            lastPosition = enemy.transform.position;
            Animator.SetFloat(SpeedId, worldDt > 0 ? delta.magnitude / worldDt : 0, .1f, dt);

            string state = enemy.State;
            if ((state == "Windup" || state == "ChargeWindup") && state != lastState) Animator.SetTrigger(AttackId);
            lastState = state;
            Animator.SetBool(StunnedId, state == "Stunned");
            Animator.SetBool(DormantId, state == "Dormant");
            Animator.SetBool(DeadId, enemy.Defeated || state == "Dissolving" || state == "Dissolved");
            if (enemy.Health < lastHealth && !enemy.Defeated) Animator.SetTrigger(HitId);
            lastHealth = enemy.Health;
        }

        void LateUpdate()
        {
            if (!UseRestShoulders || enemy == null || Animator == null || enemy.Defeated ||
                !Animator.isHuman || Animator.runtimeAnimatorController == null) return;
            var current = Animator.GetCurrentAnimatorStateInfo(0);
            float weight = RestPoseState(current) ? 1 : 0;
            if (Animator.IsInTransition(0))
            {
                var next = Animator.GetNextAnimatorStateInfo(0);
                float progress = Mathf.Clamp01(Animator.GetAnimatorTransitionInfo(0).normalizedTime);
                weight = Mathf.Lerp(weight, RestPoseState(next) ? 1 : 0, progress);
            }
            if (weight <= 0) return;
            RestoreShoulder(leftShoulder, leftArm, leftRest, weight);
            RestoreShoulder(rightShoulder, rightArm, rightRest, weight);
        }

        static bool RestPoseState(AnimatorStateInfo state) => state.IsName("Locomotion") || state.IsName("Dormant");

        static void RestoreShoulder(Transform shoulder, Transform arm, Quaternion rest, float weight)
        {
            if (shoulder == null || arm == null) return;
            Quaternion armDirection = arm.rotation;
            shoulder.localRotation = Quaternion.Slerp(shoulder.localRotation, rest, weight);
            arm.rotation = armDirection;
        }
    }
}
