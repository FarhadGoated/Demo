using UnityEngine;

namespace HellSlice
{
    public sealed class SliceHazard : MonoBehaviour
    {
        public float phaseOffset;
        public Transform Flames;
        public Renderer Warning;
        public BoxCollider DamageVolume;
        [Min(1)] public int Damage = 1;
        public float Elapsed { get; private set; }
        public bool Burning => Phase >= 1 && Phase < 3;
        public float Phase => Mathf.Repeat(Elapsed + phaseOffset, 7);
        public static bool IsBurningAt(float time) => Mathf.Repeat(time, 7) >= 1 && Mathf.Repeat(time, 7) < 3;
        void Update()
        {
            var s = SliceSession.Current;
            if (s == null || !s.Running) return;
            Elapsed += s.Clock.WorldDelta;
            Flames.gameObject.SetActive(Burning);
            Warning.sharedMaterial.color = Burning ? new Color(1, .22f, .02f) : Phase < 1 ? new Color(1, .65f, .08f) : new Color(.12f, .35f, .38f);
            if (Burning)
            {
                if (DamageVolume != null && new Bounds(DamageVolume.center, DamageVolume.size).Contains(
                    DamageVolume.transform.InverseTransformPoint(s.Player.transform.position + Vector3.up * .9f))) s.Player.TakeDamage(Damage);
                Flames.localScale = new Vector3(1, .9f + Mathf.Sin(Elapsed * 15) * .1f, 1);
            }
        }
    }
}

