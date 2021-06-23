namespace Crowbar
{
    using UnityEngine;

    /// <summary>
    /// Component for move camera for character
    /// </summary>
    [AddComponentMenu("Components character/Components player/Camera component")]
    public class CameraComponent : MonoBehaviour, ICharacterComponent
    {
        #region Variables
        [Header("Camera settings")]
        [Tooltip("Enable on server")]
        public bool enableOnServer = false;
        [Tooltip("Z pozition camera")]
        public float zLevel = -100f;
        [Tooltip("Look size camera")]
        public float lookSize = 5f;
        [Tooltip("Smooth move camera")]
        public float smoothMove = 0.1f;
        [Tooltip("Current render camera")]
        public Camera cam;
        [Tooltip("Current camera targets")]
        public Transform target;

        private float m_targetLookSize;
        private float m_velocityLookSize;
        #endregion

        #region Functions
        /// <summary>
        /// Set camera default look size
        /// </summary>
        public void SetDefaultLookSize()
        {
            SetLookSize(lookSize);
        }

        /// <summary>
        /// Set camera look size
        /// </summary>
        /// <param name="size">Look size</param>
        public void SetLookSize(float size)
        {
            m_targetLookSize = size;
        }

        /// <summary>
        /// Set state component
        /// </summary>
        /// <param name="isActive">State component</param>
        public void SetComponentActive(bool isActive)
        {
            enabled = isActive;
        }

        /// <summary>
        /// Check enable component for server
        /// </summary>
        public bool IsEnableComponentOnServer()
        {
            return enableOnServer;
        }

        /// <summary>
        /// Initialization this component
        /// </summary>
        public virtual void Initialize()
        {
            if (target == null)
                target = transform;

            if (cam == null && Camera.main != null)
                cam = Camera.main;

            m_targetLookSize = lookSize;
        }

        /// <summary>
        /// Moving camera
        /// </summary>
        public virtual void MoveToTarget()
        {
            if (cam != null && target != null)
            {
                Vector3 pos = Vector3.Lerp(cam.transform.position, target.transform.position, smoothMove);
                pos.z = zLevel;

                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, m_targetLookSize, smoothMove);
                cam.transform.position = pos;
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            MoveToTarget();
        }   
        #endregion
    }
}
