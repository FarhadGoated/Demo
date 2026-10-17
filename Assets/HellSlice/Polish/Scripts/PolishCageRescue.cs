using System.Collections;
using UnityEngine;

namespace HellSlice.Polish
{
    /// <summary>
    /// Visual only: the hanging cage around the trapped figure. Reads the session and never changes gameplay.
    /// The cage sways on its chain; once the rescue lock is released the lever glows; when the session records the
    /// assistance (the existing "help" interaction at the cage) the lever is pulled and the door lifts.
    /// The figure's own fade is handled by SliceMemoryFigure. Scene reload resets everything with the stage.
    /// </summary>
    public sealed class PolishCageRescue : MonoBehaviour
    {
        public Transform Cage, Door, LeverHandle;
        public Renderer LeverGlow;
        public Light CageLight;
        [Tooltip("Lever handle X rotation before and after the pull (degrees).")]
        public float LeverFrom = 40, LeverTo = -40;
        [Tooltip("Door lift in the cage's local units.")]
        public float DoorLift = 2.2f;
        public float LeverSeconds = .35f, DoorSeconds = 1.2f;

        MaterialPropertyBlock block;
        Vector3 doorClosed;
        Quaternion cageRest;
        bool released;
        float lightStart;
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        void Start()
        {
            block = new MaterialPropertyBlock();
            if (Door != null) doorClosed = Door.localPosition;
            if (Cage != null) cageRest = Cage.localRotation;
            if (CageLight != null) lightStart = CageLight.intensity;
            SetLever(0);
        }

        void Update()
        {
            float t = Time.time;
            if (Cage != null) Cage.localRotation = cageRest * Quaternion.Euler(Mathf.Sin(t * .7f) * 1.2f, 0, Mathf.Sin(t * .53f) * 1.5f);

            var s = SliceSession.Current;
            if (s == null) return;
            if (LeverGlow != null)
            {
                // Pulse the lever once the rescue lock is released, so the player knows where to pull.
                float pulse = s.SwitchLatched && !s.Assisted ? .5f + .5f * Mathf.Sin(t * 4) : 0;
                LeverGlow.GetPropertyBlock(block); block.SetColor(EmissionId, new Color(1f, .55f, .15f) * (pulse * 2.5f)); LeverGlow.SetPropertyBlock(block);
            }
            if (s.Assisted) PreviewRelease();
        }

        /// <summary>Plays the lever pull and door lift once (also used by the review capture).</summary>
        public void PreviewRelease()
        {
            if (released) return;
            released = true; StartCoroutine(Release());
        }

        IEnumerator Release()
        {
            for (float e = 0; e < LeverSeconds; e += Time.deltaTime) { SetLever(e / LeverSeconds); yield return null; }
            SetLever(1);
            for (float e = 0; e < DoorSeconds; e += Time.deltaTime)
            {
                float d = Mathf.SmoothStep(0, 1, e / DoorSeconds);
                if (Door != null) Door.localPosition = doorClosed + Vector3.up * (DoorLift * d);
                if (CageLight != null) CageLight.intensity = Mathf.Lerp(lightStart, lightStart * .45f, d);
                yield return null;
            }
            if (Door != null) Door.localPosition = doorClosed + Vector3.up * DoorLift;
        }

        void SetLever(float k)
        {
            if (LeverHandle != null) LeverHandle.localRotation = Quaternion.Euler(Mathf.Lerp(LeverFrom, LeverTo, Mathf.SmoothStep(0, 1, k)), 0, 0);
        }
    }
}
