using UnityEngine;

namespace HellSlice
{
    public sealed class SliceTime : MonoBehaviour
    {
        public const float Duration = 5f;
        public const float RechargeDuration = 8f;
        public float Remaining { get; private set; }
        public float Recharge { get; private set; }
        public float Factor => Remaining > 0f ? .25f : 1f;
        public float WorldDelta => SliceSession.Current != null && SliceSession.Current.Running ? Time.unscaledDeltaTime * Factor : 0f;
        public bool Activate()
        {
            var s = SliceSession.Current;
            if (s == null || !s.SlowTimeAvailable || Remaining > 0f || Recharge > 0f) return false;
            Remaining = Duration;
            return true;
        }
        public void Tick(float dt)
        {
            // Charge starts only after the active window; preserve overflow for deterministic tests.
            if (Remaining > 0f)
            {
                float spent = Mathf.Min(dt, Remaining);
                Remaining -= spent;
                dt -= spent;
                if (Remaining <= 0f) Recharge = RechargeDuration;
            }
            Recharge = Mathf.Max(0f, Recharge - dt);
        }
        public void ResetState() { Remaining = 0f; Recharge = 0f; }
    }
}

