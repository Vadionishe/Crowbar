using System;
using UnityEngine;
using Mirror;

namespace Crowbar.Server
{
    /// <summary>
    /// Server room logic for client connections
    /// </summary>
    public class GameServer : MonoBehaviour
    {
        #region Variables
        public static GameServer instance;

        public float physicTime = 0.04f;
        public bool destroyObject;
        public GameObject[] destroedGameObjectInServer;
        public Component[] destroedComponentInServer;       

        private string[] args;
        #endregion

        #region Functions
        /// <summary>
        /// Parsing arguments to compute the server port
        /// </summary>
        private void Start()
        {
            instance = this;
            args = Environment.GetCommandLineArgs();
            Time.fixedDeltaTime = physicTime;

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

            if (destroyObject)
            {
                foreach (GameObject destroyObject in destroedGameObjectInServer)
                    if (destroyObject != null)
                        Destroy(destroyObject);

                foreach (Component destroyComponent in destroedComponentInServer)
                    if (destroyComponent != null)
                        Destroy(destroyComponent);
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
        }
        #endregion
    }
}
