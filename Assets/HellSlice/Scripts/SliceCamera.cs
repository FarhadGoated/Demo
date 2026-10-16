using UnityEngine;

namespace HellSlice
{
    [DefaultExecutionOrder(100)]
    public sealed class SliceCamera : MonoBehaviour
    {
        public float Yaw { get; private set; }
        public SlicePlayer FollowPlayer;
        [Min(.5f)] public float FollowDistance = 4.6f;
        public float TargetHeight = 1.4f;
        public Vector2 Sensitivity = new Vector2(.12f, .10f);
        float pitch = 22f;
        public void ResetView() { Yaw = 0; pitch = 22; }
        void LateUpdate()
        {
            var s = SliceSession.Current;
            if (s == null || s.Player == null) return;
            if (s.Running)
            {
                var delta = s.Input.Look;
                Yaw += delta.x * Sensitivity.x;
                pitch = Mathf.Clamp(pitch - delta.y * Sensitivity.y, -15, 65);
            }
            Vector3 target = (FollowPlayer != null ? FollowPlayer : s.Player).ActivePosition + Vector3.up * TargetHeight;
            Quaternion rotation = Quaternion.Euler(pitch, Yaw, 0);
            Vector3 direction = rotation * Vector3.back;
            float distance = FollowDistance;
            if (Physics.SphereCast(target, .22f, direction, out var hit, distance, SliceWorld.SolidMask, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(.3f, hit.distance - .12f);
            transform.SetPositionAndRotation(target + direction * distance, rotation);
        }
    }
}

