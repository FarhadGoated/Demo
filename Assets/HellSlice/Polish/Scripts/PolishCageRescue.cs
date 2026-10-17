using System.Collections;
using UnityEngine;

namespace HellSlice.Polish
{
    /// <summary>
    /// Visual only: the hanging cage around the trapped figure and the lever in the rescue cell. Reads the session and
    /// never changes gameplay. The lever (standing on the existing rescue-lock pad) glows until it is used; releasing
    /// the rescue lock (SwitchLatched) pulls the lever and lifts the cage door. Helping the figure (Assisted) dims the
    /// cage light while SliceMemoryFigure fades her. Scene reload resets everything with the stage.
    /// </summary>
    public sealed class PolishCageRescue : MonoBehaviour
    {
        public Transform Cage, Door, LeverHandle;
        public Renderer LeverGlow;
        public Light CageLight;
        [Tooltip("Lever handle X rotation before and after the pull (degrees).")]
        public float LeverFrom = -40, LeverTo = 40;
        [Tooltip("Door lift in the cage's local units.")]
        public float DoorLift = 1.8f;
        public float LeverSeconds = .35f, DoorSeconds = 1.4f, DimSeconds = 2f;

        MaterialPropertyBlock block;
        Vector3 doorClosed;
        Quaternion cageRest;
        bool opened, dimmed;
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
                // The lever pulses until it is pulled, so the player can find it in the cell.
                float pulse = !opened ? .5f + .5f * Mathf.Sin(t * 4) : 0;
                LeverGlow.GetPropertyBlock(block); block.SetColor(EmissionId, new Color(1f, .55f, .15f) * (pulse * 2.5f)); LeverGlow.SetPropertyBlock(block);
            }
            if (s.SwitchLatched) PreviewRelease();
            if (s.Assisted && !dimmed) { dimmed = true; StartCoroutine(Dim()); }
        }

        /// <summary>Plays the lever pull and door lift once (also used by the review capture).</summary>
        public void PreviewRelease()
        {
            if (opened) return;
            opened = true; StartCoroutine(Open());
        }

        IEnumerator Open()
        {
            for (float e = 0; e < LeverSeconds; e += Time.deltaTime) { SetLever(e / LeverSeconds); yield return null; }
            SetLever(1);
            for (float e = 0; e < DoorSeconds; e += Time.deltaTime)
            {
                if (Door != null) Door.localPosition = doorClosed + Vector3.up * (DoorLift * Mathf.SmoothStep(0, 1, e / DoorSeconds));
                yield return null;
            }
            if (Door != null) Door.localPosition = doorClosed + Vector3.up * DoorLift;
        }

        IEnumerator Dim()
        {
            if (CageLight == null) yield break;
            for (float e = 0; e < DimSeconds; e += Time.deltaTime) { CageLight.intensity = Mathf.Lerp(lightStart, lightStart * .3f, e / DimSeconds); yield return null; }
        }

        void SetLever(float k)
        {
            if (LeverHandle != null) LeverHandle.localRotation = Quaternion.Euler(Mathf.Lerp(LeverFrom, LeverTo, Mathf.SmoothStep(0, 1, k)), 0, 0);
        }
    }
}
