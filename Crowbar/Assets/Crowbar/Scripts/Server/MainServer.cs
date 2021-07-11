using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using Mirror;


namespace Crowbar.Server
{
    /// <summary>
    /// Master server logic to handle and route all client connections
    /// </summary>
    public class MainServer : MonoBehaviour
    {
        #region Variables
        public class ServerEvent : UnityEvent<PlayerInstance> { }
                
        public static MainServer Instance { get; private set; }

        public static ServerEvent ConnectHandler { get; set; }
        public static ServerEvent DisconnectHandler { get; set; }
        public static ServerEvent ReadyHandler { get; set; }
        public static ServerEvent DontReadyHandler { get; set; }

        public List<PlayerInstance> AllPlayers { get; private set; }
        public List<PlayerInstance> ReadyPlayers { get; private set; }

        public ushort NeedPlayersToStart { get; set; } = 2;
        public ushort NeedPlayersToStartForce { get; set; } = 100;
        public float TimeToStart { get; set; } = 20f;

        private string pathGameRoom;
        private string pathConfig;
        #endregion

        #region Functions
        /// <summary>
        /// Create all the necessary instances
        /// Subscribe to events
        /// Server start
        /// </summary>
        private void Start()
        {
            Instance = this;

            AllPlayers = new List<PlayerInstance>();
            ReadyPlayers = new List<PlayerInstance>();

            ConnectHandler = new ServerEvent();
            DisconnectHandler = new ServerEvent();
            ReadyHandler = new ServerEvent();
            DontReadyHandler = new ServerEvent();

            ConnectHandler.AddListener(OnConnectPlayer);
            DisconnectHandler.AddListener(OnDisconnectPlayer);
            ReadyHandler.AddListener(OnReadyPlayer);
            DontReadyHandler.AddListener(OnDontReadyPlayer);

            LoadConfig();

            NetworkManager.singleton.StartServer();
        }

        /// <summary>
        /// Test information about connecting players and finding games
        /// </summary>
        private void OnGUI()
        {
            string info = $"Подключённых клиентов: {AllPlayers.Count}\n" +
                $"Готовых клиентов: {ReadyPlayers.Count}"; 

            Rect r = new Rect(5, 5, 800, 500);
            GUI.Label(r, info);
        }

        /// <summary>
        /// Fires when the player starts searching for a game
        /// If the room is full, send a message to all clients about the start of the game
        /// Clearing the list of ready players
        /// Start the room server
        /// </summary>
        /// <param name="player"></param>
        private void OnReadyPlayer(PlayerInstance player)
        {
            if (!ReadyPlayers.Contains(player))
            {
                ReadyPlayers.Add(player);

                if (ReadyPlayers.Count == NeedPlayersToStartForce)
                {
                    StartGameRoom();
                }
                else if (ReadyPlayers.Count == NeedPlayersToStart)
                {
                    StartCoroutine(WaitToStart());
                }

                ReadyPlayers.ForEach(p => p.TargetSetTextPlayersFound(p.netIdentity.connectionToClient, $"Crows [{NeedPlayersToStartForce}/{ReadyPlayers.Count}]"));
            }
        }

        /// <summary>
        /// When a player cancels a game search
        /// The player is removed from the list of ready players
        /// </summary>
        /// <param name="player"></param>
        private void OnDontReadyPlayer(PlayerInstance player)
        {
            ReadyPlayers.Remove(player);
            ReadyPlayers.ForEach(p => p.TargetSetTextPlayersFound(p.netIdentity.connectionToClient, $"Crows [{NeedPlayersToStartForce}/{ReadyPlayers.Count}]"));

            if (ReadyPlayers.Count < NeedPlayersToStart)
                StopAllCoroutines();
        }

        /// <summary>
        /// When a new player connects to the main server
        /// Adds a player to the general list of connected players
        /// </summary>
        /// <param name="player"></param>
        private void OnConnectPlayer(PlayerInstance player)
        {
            if (!AllPlayers.Contains(player))
            {
                AllPlayers.Add(player);
            }
        }

        /// <summary>
        /// When a player disconnects, we remove him from the queue in the game (if any)
        /// </summary>
        /// <param name="player"></param>
        private void OnDisconnectPlayer(PlayerInstance player)
        {
            if (ReadyPlayers.Contains(player))
            {
                OnDontReadyPlayer(player);
            }

            AllPlayers.Remove(player);
        }

        /// <summary>
        /// Running a room on a free port
        /// </summary>
        private void StartGameRoom()
        {
            int freePort = GetFreePort();

            ReadyPlayers.ForEach(player => player.TargetStartGame(player.netIdentity.connectionToClient, ushort.Parse(freePort.ToString())));
            ReadyPlayers.Clear();

            Process.Start(pathGameRoom, $"{freePort} {SQLiteDB.DBPath}");

            StopAllCoroutines();
        }

        private void LoadConfig()
        {
            pathConfig = Directory.GetCurrentDirectory() + "\\Config.txt";
            string[] settings = File.ReadAllLines(pathConfig);

            SQLiteDB.DBPath = settings[0];
            pathGameRoom = settings[1];
            NetworkManager.singleton.networkAddress = settings[2];
            (Transport.activeTransport as TelepathyTransport).port = ushort.Parse(settings[3]);
            NeedPlayersToStartForce = ushort.Parse(settings[4]);
            NeedPlayersToStart = ushort.Parse(settings[5]);
            TimeToStart = ushort.Parse(settings[6]);
        }

        /// <summary>
        /// Get a free port to run the room
        /// </summary>
        /// <returns></returns>
        private int GetFreePort()
        {
            TcpListener serverForFreePort = new TcpListener(IPAddress.Loopback, 0);

            serverForFreePort.Start();

            int port = ((IPEndPoint)serverForFreePort.LocalEndpoint).Port;

            serverForFreePort.Stop();

            return port;
        }

        private IEnumerator WaitToStart()
        {
            yield return new WaitForSeconds(TimeToStart);

            StartGameRoom();
        }
        #endregion
    }
}
