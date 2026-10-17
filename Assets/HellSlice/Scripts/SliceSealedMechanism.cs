using UnityEngine;

namespace HellSlice
{
    // Stage 7: a mechanism enclosed by a solid seal. Only the returning guardian's charge breaks the seal;
    // the player then activates the exposed mechanism once.
    public sealed class SliceSealedMechanism : MonoBehaviour
    {
        public string DisplayName = "mechanism";
        [Tooltip("Solid shell around the mechanism, saved active. The charge must touch this collider to break it.")]
        public BoxCollider Seal;
        [Tooltip("Saved inactive. Rubble shown once the seal breaks.")]
        public GameObject BrokenSeal;
        [Tooltip("Turns green when the mechanism is activated.")]
        public Renderer Core;
        [Min(.5f), Tooltip("Measured from this object's position (keep it at floor level, inside the seal).")]
        public float InteractionRange = 2f;
        public bool Exposed { get; private set; }
        public bool Activated { get; private set; }

        public bool Touches(Bounds body) =>
            !Exposed && Seal != null && Seal.enabled && Seal.gameObject.activeInHierarchy && body.Intersects(Seal.bounds);

        public bool Near(SlicePlayer player, float range)
        {
            Vector3 delta = player.transform.position - transform.position; delta.y = 0;
            return delta.magnitude <= range;
        }

        public bool InReach(SlicePlayer player) =>
            Near(player, InteractionRange) && !Physics.Linecast(player.transform.position + Vector3.up, transform.position + Vector3.up,
                SliceWorld.SolidMask, QueryTriggerInteraction.Ignore);

        public void Expose()
        {
            if (Exposed) return;
            Exposed = true;
            if (Seal != null) Seal.gameObject.SetActive(false);
            if (BrokenSeal != null) BrokenSeal.SetActive(true);
        }

        public bool Activate()
        {
            if (!Exposed || Activated) return false;
            Activated = true;
            if (Core != null)
            {
                var block = new MaterialPropertyBlock(); Core.GetPropertyBlock(block);
                block.SetColor("_BaseColor", new Color(.25f, 1, .4f)); Core.SetPropertyBlock(block);
            }
            return true;
        }

        void OnDrawGizmosSelected() { Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, InteractionRange); }
    }
}
