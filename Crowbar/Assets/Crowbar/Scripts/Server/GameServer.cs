namespace Crowbar.Server
{
    using System;
    using UnityEngine;
    using Mirror;

    /// <summary>
    /// Server room logic for client connections
    /// </summary>
    public class GameServer : MonoBehaviour
    {
        #region Variables
        public static GameServer instance;
        #endregion

        #region Functions
        /// <summary>
        /// Parsing arguments to compute the server port
        /// </summary>
        private void Start()
        {
            instance = this;

            string[] args = Environment.GetCommandLineArgs();

            try
            {
                if (args.Length > 1)
                {
                    (Transport.activeTransport as TelepathyTransport).port = Convert.ToUInt16(args[1]);
                }

                NetworkManager.singleton.StartServer();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// For a quick test
        /// When you click on '1', the server is forced to start
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                (Transport.activeTransport as TelepathyTransport).port = 8001;

                NetworkManager.singleton.StartServer();
            }
        }
        #endregion
    }
}
