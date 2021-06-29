using UnityEngine;

namespace Crowbar
{
    public class CrushCharacterChecker : MonoBehaviour
    {
        [Header("Settings checker")]
        [Tooltip("Name layer for checker")]
        public string layerName = "GroundCollision";
        [Tooltip("Name tag for checker")]
        public string tagName = "Untagged";

        private CharacterStats stats;

        private void Start()
        {
            stats = GetComponentInParent<CharacterStats>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (stats.netIdentity.isLocalPlayer)
                if (LayerMask.LayerToName(collision.gameObject.layer) == layerName && collision.tag == tagName)
                    stats.CmdDied(true);
        }
    }
}
