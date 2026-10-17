using System.Collections;
using UnityEngine;

namespace HellSlice.Polish
{
    /// <summary>
    /// Visual only: the hanging cage around the trapped figure. Reads the session and never changes gameplay.
    /// The cage sways on its chain; releasing the rescue lock (SwitchLatched, the lever in the cell) lifts the door;
    /// helping the figure (Assisted) dims the cage light while SliceMemoryFigure fades her.
    /// The lever itself is a PolishSwitchLever. Scene reload resets everything with the stage.
    /// </summary>
    public sealed class PolishCageRescue : MonoBehaviour
    {
        public Transform Cage, Door;
        public Light CageLight;
        [Tooltip("Door lift in the cage's local units.")]
        public float DoorLift = 1.8f;
        public float DoorDelay = .35f, DoorSeconds = 1.4f, DimSeconds = 2f;

        Vector3 doorClosed;
        Quaternion cageRest;
        bool opened, dimmed;
        float lightStart;

        void Start()
        {
            if (Door != null) doorClosed = Door.localPosition;
            if (Cage != null) cageRest = Cage.localRotation;
            if (CageLight != null) lightStart = CageLight.intensity;
        }

        void Update()
        {
            float t = Time.time;
            if (Cage != null) Cage.localRotation = cageRest * Quaternion.Euler(Mathf.Sin(t * .7f) * 1.2f, 0, Mathf.Sin(t * .53f) * 1.5f);
            var s = SliceSession.Current;
            if (s == null) return;
            if (s.SwitchLatched) PreviewRelease();
            if (s.Assisted && !dimmed) { dimmed = true; StartCoroutine(Dim()); }
        }

        /// <summary>Lifts the door once (also used by the review capture).</summary>
        public void PreviewRelease()
        {
            if (opened) return;
            opened = true; StartCoroutine(Open());
        }

        IEnumerator Open()
        {
            yield return new WaitForSeconds(DoorDelay);   // let the lever finish its swing first
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
    }
}
