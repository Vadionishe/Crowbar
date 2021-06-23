namespace Crowbar.Server
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Mirror;
    using System.Collections;

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
        #endregion

        #region Functions
        public void Disconnect()
        {
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene("Menu");
        }

        public void Quit()
        {
            Application.Quit();
        }

        /// <summary>
        /// At the start of the scene, we launch a delayed connection to the room server
        /// </summary>
        private void Start()
        {
            StartCoroutine(WaitForConnection());
            InvokeRepeating(nameof(CheckConnection), timeCheckForConnect, timeCheckForConnect);
        }

        /// <summary>
        /// For test
        /// Shows the status of connection to the room server
        /// </summary>
        private void OnGUI()
        {
            Rect r = new Rect(5, 5, 800, 500);
            GUI.Label(r, (NetworkClient.isConnected) ? "Connect game" : "Disconnect game");
        }

        /// <summary>
        /// Connect to the room server
        /// We get the port from the main server before connecting to the room
        /// </summary>
        private void Connect()
        {
            (Transport.activeTransport as TelepathyTransport).port = port;

            NetworkManager.singleton.StartClient();
        }

        /// <summary>
        /// Check connection for game server
        /// if no connect, load scene menu
        /// </summary>
        private void CheckConnection()
        {
            if (!NetworkClient.isConnected)
                SceneManager.LoadScene("Menu");
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
        #endregion
    }
}
