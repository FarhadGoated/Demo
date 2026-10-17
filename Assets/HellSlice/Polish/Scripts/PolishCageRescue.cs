using System.Collections;
using UnityEngine;

namespace HellSlice.Polish
{
    /// <summary>
    /// Visual only: the hanging cage around the trapped figure. Reads the session and never changes gameplay.
    /// The cage sways on its chain; releasing the rescue lock (the second rescue switch) lifts the whole cage;
    /// helping the figure (Assisted) dims the cage light while SliceMemoryFigure fades her.
    /// The lever itself is a PolishSwitchLever. Scene reload resets everything with the stage.
    /// </summary>
    public sealed class PolishCageRescue : MonoBehaviour
    {
        public Transform Cage;
        public Light CageLight;
        [Tooltip("Whole cage lift in its parent's local units. The figure and support stay in place.")]
        [Min(0)] public float LiftHeight = 3.2f;
        [Min(0)] public float LiftDelay = .35f;
        [Min(.1f)] public float LiftSeconds = 1.4f;
        public float DimSeconds = 2f;
        public bool Lifted { get; private set; }

        Vector3 cageLowered;
        Quaternion cageRest;
        bool opened, dimmed;
        float lightStart;

        void Awake()
        {
            if (Cage != null) { cageLowered = Cage.localPosition; cageRest = Cage.localRotation; }
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

        /// <summary>Lifts the cage assembly once, including its body and door.</summary>
        public void PreviewRelease()
        {
            if (opened) return;
            opened = true; StartCoroutine(Open());
        }

        IEnumerator Open()
        {
            float elapsed = 0;
            while (elapsed < LiftDelay + LiftSeconds)
            {
                var session = SliceSession.Current;
                elapsed += session != null ? session.Clock.WorldDelta : Time.deltaTime;
                float progress = Mathf.Clamp01((elapsed - LiftDelay) / Mathf.Max(.1f, LiftSeconds));
                if (Cage != null) Cage.localPosition = cageLowered + Vector3.up * (LiftHeight * Mathf.SmoothStep(0, 1, progress));
                yield return null;
            }
            if (Cage != null) Cage.localPosition = cageLowered + Vector3.up * LiftHeight;
            Lifted = true;
        }

        IEnumerator Dim()
        {
            if (CageLight == null) yield break;
            for (float e = 0; e < DimSeconds; e += Time.deltaTime) { CageLight.intensity = Mathf.Lerp(lightStart, lightStart * .3f, e / DimSeconds); yield return null; }
        }
    }
}
