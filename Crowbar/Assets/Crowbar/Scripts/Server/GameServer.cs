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
        string[] args;
        public GameObject loadScreen;
        #endregion

        #region Functions
        /// <summary>
        /// Parsing arguments to compute the server port
        /// </summary>
        private void Start()
        {
            instance = this;

            args = Environment.GetCommandLineArgs();

            try
            {
                if (args.Length > 1)
                {
                    (Transport.activeTransport as TelepathyTransport).port = Convert.ToUInt16(args[1]);
                    SQLiteDB.DBPath = args[2];

                    NetworkManager.singleton.StartServer();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            InvokeRepeating(nameof(CheckToQuit), 20f, 20f);
        }

        private void CheckToQuit()
        {
            Character[] characters = FindObjectsOfType<Character>();

            if (characters.Length > 0)
            {
                foreach (Character character in characters)
                {
                    if (!character.GetComponent<CharacterStats>().isDied)
                        return;
                }
            }

            Application.Quit();
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

            loadScreen.SetActive(!NetworkServer.active);
        }
        #endregion
    }
}
