namespace Crowbar
{
    using UnityEngine.Events;
    using UnityEngine;
    using Mirror;

    /// <summary>
    /// Sync inputs player for server
    /// </summary>
    public class PlayerInputForServer : NetworkBehaviour, ICharacterComponent
    {
        #region Variables
        [Header("Sync properties")]
        [Tooltip("Enable on server")]
        public bool enableOnServer = false;
        [Tooltip("Rate send package for sync ([1 / sendRate] in sec)"), SerializeField]
        private int sendRate = 10;

        public float horizontal { get; private set; }
        public float vertical { get; private set; }

        public class PushEvent : UnityEvent { }
        public PushEvent onClickLeft;
        public PushEvent onPushQ;
        public PushEvent onPushE;
        #endregion

        #region Fuctions
        /// <summary>
        /// Set state component
        /// </summary>
        /// <param name="isActive">State component</param>
        public void SetComponentActive(bool isActive)
        {
            enabled = isActive;
        }

        /// <summary>
        /// Send new value input
        /// </summary>
        /// <param name="nameInput">Name input update</param>
        /// <param name="value">New value</param>
        [Command]
        public void CmdSendInput(string nameInput, float value)
        {
            switch (nameInput)
            {
                case "horizontal":
                    horizontal = value;
                    break;
                case "vertical":
                    vertical = value;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Send event clicked left key mouse
        /// </summary>
        [Command]
        public void CmdClickLeft()
        {
            onClickLeft.Invoke();
        }

        /// <summary>
        /// Send event push key Q
        /// </summary>
        [Command]
        public void CmdPushQ()
        {
            onPushQ.Invoke();
        }

        /// <summary>
        /// Send event push key E
        /// </summary>
        [Command]
        public void CmdPushE()
        {
            onPushE.Invoke();
        }

        /// <summary>
        /// Send inputs player
        /// </summary>
        public void SendInputs()
        {
            if (NetworkClient.isConnected)
            {
                CmdSendInput("horizontal", Input.GetAxis("Horizontal"));
                CmdSendInput("vertical", Input.GetAxis("Vertical"));
            }
        }

        /// <summary>
        /// Check enable component for server
        /// </summary>
        public bool IsEnableComponentOnServer()
        {
            return enableOnServer;
        }

        private void Start()
        {
            onClickLeft = new PushEvent();
            onPushQ = new PushEvent();
            onPushE = new PushEvent();

            if (isLocalPlayer)
            {
                float rate = 1f / sendRate;

                InvokeRepeating(nameof(SendInputs), rate, rate);
            }
        }

        private void Update()
        {
            if (isLocalPlayer)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                    CmdClickLeft();

                if (Input.GetKeyDown(KeyCode.Q))
                    CmdPushQ();

                if (Input.GetKeyDown(KeyCode.E))
                    CmdPushE();
            }                
        }
        #endregion
    }
}
