namespace Crowbar
{
    using UnityEngine;

    /// <summary>
    /// Component for check collision ground
    /// </summary>
    public class GroundChecker : MonoBehaviour, ICollisionChecker
    {
        #region Variables
        [Header("Settings checker")]
        [Tooltip("Name layer for checker")]
        public string layerName = "GroundCollision";
        [Tooltip("Height checker for getting normal")]
        public float heightCheck;
        [Tooltip("Height checker for check grounded")]
        public float heightCheckGrounded;
        [Tooltip("Height checker for check grounded")]
        public float heighStarttCheckGrounded;
        [Tooltip("Vector checker for getting normal")]
        public Vector2 vectorCheck;

        private bool isCollision;
        private string tagCollision;
        #endregion

        #region Fuctions
        /// <summary>
        /// Check collision ground
        /// </summary>
        /// <returns>Result collision</returns>
        public bool CheckCollision()
        {
            return isCollision;
        }

        /// <summary>
        /// Check collision ground
        /// </summary>
        /// <param name="tagCollision">tag collisions</param>
        /// <returns>Result collision</returns>
        public bool CheckCollision(out string tagCollision)
        {
            tagCollision = this.tagCollision;

            return isCollision;
        }

        /// <summary>
        /// Check normal ground
        /// </summary>
        /// <returns>Result normal for collision ground</returns>
        public float GetNormalGround()
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 raycatsPosition = transform.position - new Vector3(0.3f, heighStarttCheckGrounded, 0) + (new Vector3(0.15f, 0, 0) * i);
                RaycastHit2D hit = Physics2D.Raycast(raycatsPosition, vectorCheck, heightCheckGrounded, LayerMask.GetMask(layerName));

                if (hit.collider != null)
                    return hit.normal.x;
            }

            return 1f;
        }

        /// <summary>
        /// Check normal ground
        /// </summary>
        /// <returns>Result normal for collision ground</returns>
        public bool IsGroundRaycasting()
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 raycatsPosition = transform.position - new Vector3(0.3f, heighStarttCheckGrounded, 0) + (new Vector3(0.15f, 0, 0) * i);
                RaycastHit2D hit = Physics2D.Raycast(raycatsPosition, vectorCheck, heightCheckGrounded, LayerMask.GetMask(layerName));

                if (hit.collider != null)
                    return true;
            }

            return false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            SetCollisionInfo(collision, true);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            SetCollisionInfo(collision, true);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            SetCollisionInfo(collision, false);
        }

        private void SetCollisionInfo(Collider2D collision, bool isEnter)
        {
            if (LayerMask.LayerToName(collision.gameObject.layer) == layerName)
            {
                tagCollision = (isEnter) ? collision.tag : string.Empty;
                isCollision = isEnter;
            }
        }
        #endregion
    }
}
