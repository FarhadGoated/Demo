using UnityEngine;

namespace HellSlice.Polish
{
    /// <summary>
    /// Business Man's skinned feet hang off the rig's Root (Blender IK), outside the humanoid leg chain, so the
    /// Animator never moves them. After animation, snap each foot to the leg-chain end that the avatar maps as the foot.
    /// </summary>
    public sealed class PolishBoneFollow : MonoBehaviour
    {
        public Transform[] Sources, Targets;
        Quaternion[] offsets;

        /// <summary>Adds the follower when the model has the Quaternius foot layout; call while it is still in its default pose.</summary>
        public static PolishBoneFollow Attach(GameObject model)
        {
            Transform Find(string n) { foreach (var t in model.GetComponentsInChildren<Transform>(true)) if (t.name == n) return t; return null; }
            Transform sl = Find("LowerLeg.L_end"), sr = Find("LowerLeg.R_end"), tl = Find("Foot.L"), tr = Find("Foot.R");
            if (sl == null || sr == null || tl == null || tr == null) return null;
            var f = model.AddComponent<PolishBoneFollow>();
            f.Sources = new[] { sl, sr }; f.Targets = new[] { tl, tr };
            f.offsets = new[] { Quaternion.Inverse(sl.rotation) * tl.rotation, Quaternion.Inverse(sr.rotation) * tr.rotation };
            return f;
        }

        public void Apply()
        {
            if (offsets == null) return;
            for (int i = 0; i < Targets.Length; i++)
                if (Sources[i] != null && Targets[i] != null)
                    Targets[i].SetPositionAndRotation(Sources[i].position, Sources[i].rotation * offsets[i]);
        }

        void LateUpdate() => Apply();
    }
}
