using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections;
using Crowbar.System;

namespace Crowbar.Server
{
    /// <summary>
    /// Client logic for connecting
    /// </summary>
    public class ClientGame : MonoBehaviour
    {
        #region Variables
        public static ushort port { get; set; }

        [Header("Client properties")]
        [Tooltip("Timeout for the first connection request"), SerializeField]
        private float timeOffsetForConnect = 5f;
        [Tooltip("Time for the check connection"), SerializeField]
        private float timeCheckForConnect = 10f;

        private bool isChecked;
        #endregion

        #region Functions
        public void Disconnect()
        {
            GameUI.SetLoadScreen(true);
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene("Menu");
        }

        public void Quit()
        {
            GameUI.SetLoadScreen(true);
            Application.Quit();
        }

        /// <summary>
        /// Connect to the room server
        /// We get the port from the main server before connecting to the room
        /// </summary>
        private void Connect()
        {
            (Transport.activeTransport as TelepathyTransport).port = port;

            NetworkManager.singleton.StartClient();

            FlashWindow.Flash();
        }

        /// <summary>
        /// Check connection for game server
        /// if no connect, load scene menu
        /// </summary>
        private void CheckConnection()
        {
            if (!NetworkClient.isConnected && !isChecked)
                StartCoroutine(WaitCheckConnection());
        }

        /// <summary>
        /// The delayed connection is made so that the server application has time to start
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForConnection()
        {
            yield return new WaitForSeconds(timeOffsetForConnect);

            Connect();
        }

        private IEnumerator WaitCheckConnection()
        {
            isChecked = true;

            yield return new WaitForSeconds(10f);

            if (!NetworkClient.isConnected)
            {
                SceneManager.LoadScene("Menu");
            }
            else
            {
                isChecked = false;
            }
        }

        /// <summary>
        /// At the start of the scene, we launch a delayed connection to the room server
        /// </summary>
        private void Start()
        {
            StartCoroutine(WaitForConnection());
            InvokeRepeating(nameof(CheckConnection), timeCheckForConnect, timeCheckForConnect);
        }
        #endregion
    }
}
