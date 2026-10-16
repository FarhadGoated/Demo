using UnityEngine;

namespace HellSlice
{
    public sealed class SlicePuzzleSwitch : MonoBehaviour
    {
        [Min(.1f)] public float InteractionRange = 1.2f;
        public string InteractionLabel = "Operate mechanism";
        public Renderer Visual;
        public SlicePuzzleSwitch Prerequisite;
        public GameObject OpensGate;
        public bool Completed { get; private set; }

        public bool CanInteract(SlicePlayer player)
        {
            return isActiveAndEnabled && !Completed && player != null && !player.Carrying &&
                (Prerequisite == null || Prerequisite.Completed) &&
                Vector3.Distance(player.transform.position, transform.position) <= InteractionRange &&
                !Physics.Linecast(player.transform.position + Vector3.up * .5f, transform.position + Vector3.up * .5f,
                    SliceWorld.SolidMask, QueryTriggerInteraction.Ignore);
        }
        public bool Interact(SlicePlayer player)
        {
            if (SliceSession.Current == null || !SliceSession.Current.Running || !CanInteract(player)) return false;
            Completed = true;
            if (OpensGate != null) OpensGate.SetActive(false);
            if (Visual != null)
            {
                var block = new MaterialPropertyBlock(); Visual.GetPropertyBlock(block);
                block.SetColor("_BaseColor", new Color(.25f, 1, .4f)); Visual.SetPropertyBlock(block);
            }
            return true;
        }
        void OnDrawGizmosSelected() { Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, InteractionRange); }
    }
}
