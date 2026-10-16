using UnityEngine;

namespace HellSlice
{
    public sealed class SliceLava : MonoBehaviour
    {
        public BoxCollider Volume;
        void Update()
        {
            var s = SliceSession.Current;
            if (s == null || !s.Running || Volume == null || !Volume.enabled) return;
            // Poll the authored volume: works with CharacterControllers without adding rigidbodies.
            if (Volume.bounds.Intersects(s.Player.Body.bounds)) s.KillPlayer();
        }
    }
}
