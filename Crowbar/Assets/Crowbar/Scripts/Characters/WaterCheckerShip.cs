namespace Crowbar
{
    using UnityEngine;

    public class WaterCheckerShip : MonoBehaviour, ICollisionChecker
    {
        #region Variables
        [Header("Settings checker")]
        [Tooltip("Name layer for checker")]
        public string layerName = "Water";

        private bool isCollision;
        #endregion

        #region Fuctions
        public bool CheckCollision()
        {
            return isCollision;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (LayerMask.LayerToName(collision.gameObject.layer) == layerName)
            {
                isCollision = true;
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (LayerMask.LayerToName(collision.gameObject.layer) == layerName)
            {
                isCollision = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (LayerMask.LayerToName(collision.gameObject.layer) == layerName)
            {
                isCollision = false;
            }
        }
        #endregion
    }
}