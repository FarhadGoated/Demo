using System.Collections;
using UnityEngine;

namespace HellSlice.Polish
{
    /// <summary>
    /// Visual only: a lever standing in for an existing SlicePuzzleSwitch. It glows while the switch can be used
    /// (its prerequisite is done) and swings once the switch completes, whoever completed it. Never changes gameplay;
    /// scene reload resets it with the stage.
    /// </summary>
    public sealed class PolishSwitchLever : MonoBehaviour
    {
        public SlicePuzzleSwitch Switch;
        public Transform Handle;
        public Renderer Glow;
        [Tooltip("Handle X rotation before and after the pull (degrees).")]
        public float From = -40, To = 40;
        public float Seconds = .35f;

        MaterialPropertyBlock block;
        bool pulled;
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        void Start() { block = new MaterialPropertyBlock(); Set(0); }

        void Update()
        {
            if (Switch == null) return;
            if (Glow != null)
            {
                bool ready = !pulled && Switch.isActiveAndEnabled && (Switch.Prerequisite == null || Switch.Prerequisite.Completed);
                float pulse = ready ? .5f + .5f * Mathf.Sin(Time.time * 4) : 0;
                Glow.GetPropertyBlock(block); block.SetColor(EmissionId, new Color(1f, .55f, .15f) * (pulse * 2.5f)); Glow.SetPropertyBlock(block);
            }
            if (Switch.Completed) Pull();
        }

        /// <summary>Plays the pull once (also used by the review capture).</summary>
        public void Pull()
        {
            if (pulled) return;
            pulled = true; StartCoroutine(Swing());
        }

        IEnumerator Swing()
        {
            for (float e = 0; e < Seconds; e += Time.deltaTime) { Set(e / Seconds); yield return null; }
            Set(1);
        }

        void Set(float k)
        {
            if (Handle != null) Handle.localRotation = Quaternion.Euler(Mathf.Lerp(From, To, Mathf.SmoothStep(0, 1, k)), 0, 0);
        }
    }
}
