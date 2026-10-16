using UnityEngine;

namespace HellSlice
{
    public sealed class SliceWorldLabel : MonoBehaviour
    {
        Renderer labelRenderer;
        void Awake() { labelRenderer = GetComponent<Renderer>(); }
        void LateUpdate()
        {
            var s = SliceSession.Current;
            if (s == null || s.CameraRig == null) return;
            Vector3 from = s.CameraRig.transform.position;
            bool nearby = Vector3.Distance(s.Player.ActivePosition, transform.position) < 9f;
            labelRenderer.enabled = nearby && !Physics.Linecast(from, transform.position, SliceWorld.SolidMask, QueryTriggerInteraction.Ignore);
        }
    }
}
