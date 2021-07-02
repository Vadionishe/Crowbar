namespace Crowbar
{
    using UnityEngine;
    using Mirror;
    using System.Collections;

    /// <summary>
    /// Custom component for sync position
    /// </summary>
    public class SyncPosition : NetworkBehaviour
    {
        #region Variables
        [Header("Sync properties")]
        [Tooltip("Defines who will be responsible for object sync")]
        public bool isServerObject;

        [Tooltip("Sync this object position")]
        public bool syncPosition = true;
        [Tooltip("Sync this object rotation")]
        public bool syncRotation = true;
        [Tooltip("Always position Z = 0 (global and local)")]
        public bool alwaysZeroZ = false;

        [Tooltip("Rate send package for sync ([1 / sendRate] in sec)"), SerializeField]
        private int sendRate = 10;
        [Tooltip("Smooth speed for position"), SerializeField]
        private float smoothMove = 0.1f;
        [Tooltip("Smooth speed for rotation"), SerializeField]
        private float smoothRotate = 0.1f;
        [Tooltip("Simulate ping"), SerializeField]
        private float simulatePing = 0f;
        [Tooltip("Simulate ping"), SerializeField]
        private float percentLostPacket = 0f;

        private Vector3 m_targetPositionGlobal, m_targetPositionLocal, m_targetRotation;
        private Vector3 m_velocityMove, m_velocityRotate;
        #endregion

        #region Fuctions
        private void Start()
        {
            float rate = 1f / sendRate;

            m_targetPositionGlobal = transform.position;
            m_targetPositionLocal = transform.position;

            InvokeRepeating(nameof(UpdatePosition), rate, rate);
        }

        private void Update()
        {
            if (isServer)
            {
                if (!isServerObject)
                {
                    PositionLerp();
                    RotationLerp();
                }
            }
            else if (!isLocalPlayer)
            {
                PositionLerp();
                RotationLerp();
            }
        }

        private void UpdatePosition()
        {
            if (!syncRotation && !syncPosition)
                return;

            if (gameObject.activeSelf)
                StartCoroutine(SimulatePing());
        }

        [Command]
        private void CmdUpdatePosition(Vector3 globalPosition, Vector3 localPosition, NetworkIdentity newParent, Vector3 rotation)
        {
            UpdatePositionForOther(globalPosition, localPosition, newParent, rotation);
            RpcUpdatePosition(globalPosition, localPosition, newParent, rotation);
        }

        [ClientRpc]
        private void RpcUpdatePosition(Vector3 globalPosition, Vector3 localPosition, NetworkIdentity newParent, Vector3 rotation)
        {
            if (isLocalPlayer)
                return;

            UpdatePositionForOther(globalPosition, localPosition, newParent, rotation);
        }

        private void UpdatePositionForOther(Vector3 globalPosition, Vector3 localPosition, NetworkIdentity newParent, Vector3 rotation)
        {
            if (syncPosition)
            {
                Transform parent = (newParent != null) ? newParent.transform : null;

                if (transform.parent != transform)
                {
                    transform.SetParent(parent);

                    if (parent != null)
                        m_velocityMove = Vector3.zero;
                }

                m_targetPositionGlobal = globalPosition;
                m_targetPositionLocal = localPosition;
                m_targetRotation = rotation;
            }
            else
            {
                m_targetPositionGlobal = transform.position;
                m_targetPositionLocal = transform.position;
            }
        }

        private void PositionLerp()
        {
            if (!syncPosition)
                return;

            if (alwaysZeroZ)
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);

            if (transform.parent != null)
            {
                Vector3 newPos = Vector3.SmoothDamp(transform.localPosition, m_targetPositionLocal, ref m_velocityMove, smoothMove);

                transform.localPosition = new Vector3(newPos.x, newPos.y, transform.localPosition.z);
            }
            else
            {
                Vector3 newPos = Vector3.SmoothDamp(transform.position, m_targetPositionLocal, ref m_velocityMove, smoothMove);

                transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
            }              
        }

        private void RotationLerp()
        {
            if (!syncRotation)
                return;

            float angleX = Mathf.SmoothDampAngle(transform.eulerAngles.x, m_targetRotation.x, ref m_velocityRotate.x, smoothRotate);
            float angleY = Mathf.SmoothDampAngle(transform.eulerAngles.y, m_targetRotation.y, ref m_velocityRotate.y, smoothRotate);
            float angleZ = Mathf.SmoothDampAngle(transform.eulerAngles.z, m_targetRotation.z, ref m_velocityRotate.z, smoothRotate);

            transform.eulerAngles = new Vector3(angleX, angleY, angleZ);
        }

        private NetworkIdentity GetParent()
        {
            if (transform.parent != null)
                return transform.parent.GetComponent<NetworkIdentity>();

            return null;
        }

        private IEnumerator SimulatePing()
        {
            yield return new WaitForSeconds(simulatePing);

            if (gameObject != null && gameObject.activeSelf)
            {
                if (percentLostPacket < Random.Range(0, 1f))
                {
                    if (isServerObject && isServer)
                        RpcUpdatePosition(transform.position, transform.localPosition, GetParent(), transform.eulerAngles);
                    else if (isLocalPlayer && NetworkClient.isConnected)
                        CmdUpdatePosition(transform.position, transform.localPosition, GetParent(), transform.eulerAngles);
                }
            }
        }
        #endregion
    }
}
