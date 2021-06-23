namespace Crowbar.Server
{
    using System.Collections.Generic;
    using System.Diagnostics;    
    using System.Net.NetworkInformation;
    using System.Net;
    using System.Linq;

    using UnityEngine;
    using UnityEngine.Events;
    using Mirror;

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

        private const string pathGameRoom = @"C:\Users\Validay\Desktop\GameServer\Crowbar.exe";
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

                if (ReadyPlayers.Count == NeedPlayersToStart)
                {
                    StartGameRoom();
                }
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

            Process.Start(pathGameRoom, $"{freePort}");
        }

        /// <summary>
        /// Get a free port to run the room
        /// </summary>
        /// <returns></returns>
        private int GetFreePort()
        {
            int startPort = 1000;
            int endPort = 10000;

            bool IsFree(int port)
            {
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] listeners = properties.GetActiveTcpListeners();
                int[] openPorts = listeners.Select(item => item.Port).ToArray();

                return openPorts.All(openPort => openPort != port);
            }

            for (int p = startPort; p < endPort; p++)
            {
                if (IsFree(startPort))
                {
                    return p;
                }
            }

            return -1;
        }
        #endregion
    }
}
