using UnityEngine;

namespace Crowbar
{
    public class CrushCharacterChecker : MonoBehaviour, ICollisionChecker
    {
        [Header("Settings checker")]
        [Tooltip("Name layer for checker")]
        public string layerName = "GroundCollision";
        [Tooltip("Name tag for checker")]
        public string tagName = "Untagged";

        private CharacterStats stats;
        private bool isGround;

        /// <summary>
        /// Check collision ground
        /// </summary>
        /// <returns>Result collision</returns>
        public bool CheckCollision()
        {
            return isGround;
        }

        private void Start()
        {
            stats = GetComponentInParent<CharacterStats>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (LayerMask.LayerToName(collision.gameObject.layer) == layerName && collision.tag == tagName)
            {
                isGround = true;

                stats.ChangeDied(true);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (LayerMask.LayerToName(collision.gameObject.layer) == layerName && collision.tag == tagName)
            {
                isGround = false;
            }
        }
    }
}
