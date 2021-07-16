using System.Collections;
using UnityEngine;
using Mirror;
using Crowbar.System;

namespace Crowbar
{
    /// <summary>
    /// Component for move camera for character
    /// </summary>
    [AddComponentMenu("Components character/Components player/Camera component")]
    public class CameraComponent : NetworkBehaviour, ICharacterComponent
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
        #endregion

        #region Functions
        [Client]
        public void Shake(float duration, float magnitude)
        {
            StopAllCoroutines();
            StartCoroutine(Shaking(duration, magnitude));

            FlashWindow.Flash(3);
        }

        /// <summary>
        /// Set camera default look size
        /// </summary>
        [Client]
        public void SetDefaultLookSize(float smooth = 0.1f)
        {
            smoothMove = smooth;
            SetLookSize(lookSize);
        }

        /// <summary>
        /// Set camera look size
        /// </summary>
        /// <param name="size">Look size</param>
        [Client]
        public void SetLookSize(float size, float smooth = 0.1f)
        {
            smoothMove = smooth;
            m_targetLookSize = size;
        }

        /// <summary>
        /// Set state component
        /// </summary>
        /// <param name="isActive">State component</param>
        [Client]
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
        [Client]
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
        [Client]
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

        [Client]
        private IEnumerator Shaking(float duration, float magnitude)
        {
            Vector3 originalPos = cam.transform.localPosition;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float xOffset = Random.Range(-0.5f, 0.5f) * magnitude;
                float yOffset = Random.Range(-0.5f, 0.5f) * magnitude;

                cam.transform.localPosition += new Vector3(xOffset, yOffset, 0f);

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            cam.transform.localPosition = originalPos;
        }

        private void Awake()
        {
            if (!isServer)
                Initialize();
        }

        private void Update()
        {
            if (!isServer)
                MoveToTarget();
        }   
        #endregion
    }
}
