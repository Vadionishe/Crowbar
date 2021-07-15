using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using System;

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
        public List<Room> Rooms { get; private set; }

        public ushort NeedPlayersToStart { get; set; } = 2;
        public ushort NeedPlayersToStartForce { get; set; } = 100;
        public float TimeToStart { get; set; } = 20f;

        private string pathGameRoom;
        private string pathConfig;
        #endregion

        #region Functions
        public static void SendMessage(PlayerInstance player, string message)
        {
            Room room = null;

            foreach (Room _room in Instance.Rooms)
            {
                if (_room.players.Contains(player))
                {
                    room = _room;

                    break;
                }
            }

            if (room != null)
            {
                string _message = $"[{player.nameCrow}]: {message}";

                room.players.ForEach(p => p.TargetSendMessage(p.connectionToClient, _message));
            }
        }

        public static void PlayerReady(PlayerInstance player, int idRoom, bool isReady)
        {
            Room room = Instance.Rooms.Find(r => r.Id == idRoom);

            if (room != null)
            {
                player.isReady = isReady;

                Instance.UpdateRoomInfoPlayers(room);

                if (room.players.Count >= room.MinPlayer)
                {
                    foreach (PlayerInstance _player in room.players)
                        if (!_player.isReady)
                            return;

                    StartRoom(idRoom);
                }
            }   
        }

        public static void StartRoom(int id)
        {
            Room room = Instance.Rooms.Find(r => r.Id == id);
            int freePort = Instance.GetFreePort();

            room.players.ForEach(player => player.TargetStartGame(player.netIdentity.connectionToClient, ushort.Parse(freePort.ToString())));

            try
            {
                Process gameServer = new Process();

                gameServer.StartInfo = new ProcessStartInfo(Instance.pathGameRoom, $"{freePort} {SQLiteDB.DBPath} -batchmode -nographics -logFile");
                gameServer.StartInfo.UseShellExecute = false;

                gameServer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            DeleteRoom(room.Id);
        }

        public static void CreateRoom(PlayerInstance creator, string name, string password)
        {
            Room room = new Room(name, password);
 
            Instance.Rooms.Add(room);
            EnterRoom(creator, room.Id);

            string info = $"Crows: {room.players.Count}/{room.MaxPlayer}";

            if (!string.IsNullOrEmpty(room.Password))
                info += "\nNeed password";

            Instance.AllPlayers.ForEach(p => p.TargetSpawnRoom(p.connectionToClient, room.Id, info));
        }

        public static void DeleteRoom(int id)
        {
            Room room = Instance.Rooms.Find(r => r.Id == id);

            Instance.Rooms.Remove(room);
            Instance.AllPlayers.ForEach(p => p.TargetRemoveRoom(p.connectionToClient, room.Id));
            room.players.ForEach(p => p.TargetLeaveRoom(p.connectionToClient));
        }

        public static void EnterRoom(PlayerInstance enterPlayer, int id)
        {
            Room room = Instance.Rooms.Find(r => r.Id == id);

            if (room != null)
            {
                if (room.players.Count < room.MaxPlayer)
                {
                    room.players.Add(enterPlayer);

                    string info = $"Crows: {room.players.Count}/{room.MaxPlayer}";

                    if (!string.IsNullOrEmpty(room.Password))
                        info += "\nNeed password";

                    Instance.AllPlayers.ForEach(p => p.TargetUpdateRoom(p.connectionToClient, room.Id, info));
                    enterPlayer.TargetGoRoom(enterPlayer.connectionToClient, id, string.Empty);

                    Instance.UpdateRoomInfoPlayers(room);
                }
                else
                {
                    enterPlayer.TargetGoRoom(enterPlayer.connectionToClient, id, "Room is full!");
                }
            }
        }

        public static void RemoveFromRoom(PlayerInstance removePlayer, int id)
        {
            Room room = Instance.Rooms.Find(r => r.Id == id);

            if (room != null)
            {
                room.players.Remove(removePlayer);

                string info = $"Crows: {room.players.Count}/{room.MaxPlayer}";

                if (!string.IsNullOrEmpty(room.Password))
                    info += "\nNeed password";

                if (room.players.Count == 0)
                {
                    DeleteRoom(room.Id);
                }
                else
                {
                    Instance.AllPlayers.ForEach(p => p.TargetUpdateRoom(p.connectionToClient, room.Id, info));
                    Instance.UpdateRoomInfoPlayers(room);
                }

                removePlayer.isReady = false;
                removePlayer.TargetLeaveRoom(removePlayer.connectionToClient);
            }
        }

        private void UpdateRoomInfoPlayers(Room room)
        {
            string infoPlayers = "Crows:\n\n";

            foreach (PlayerInstance player in room.players)
            {
                infoPlayers += $"{player.nameCrow}";

                if (player.isReady)
                    infoPlayers += " [Ready]";

                infoPlayers += "\n";
            }

            room.players.ForEach(p => p.TargetSetTextPlayers(p.connectionToClient, infoPlayers));
        }

        /// <summary>
        /// Create all the necessary instances
        /// Subscribe to events
        /// Server start
        /// </summary>
        private void Start()
        {
            Instance = this;

            Rooms = new List<Room>();
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
                AllPlayers.Add(player);

            foreach (Room room in Rooms)
            {
                string info = $"Crows: {room.players.Count}/{room.MaxPlayer}";

                if (!string.IsNullOrEmpty(room.Password))
                    info += "\nNeed password";

                player.TargetSpawnRoom(player.connectionToClient, room.Id, info);
            }
        }

        /// <summary>
        /// When a player disconnects, we remove him from the queue in the game (if any)
        /// </summary>
        /// <param name="player"></param>
        private void OnDisconnectPlayer(PlayerInstance player)
        {
            if (ReadyPlayers.Contains(player))
                OnDontReadyPlayer(player);

            foreach (Room room in Rooms) 
            {
                if (room.players.Contains(player))
                {
                    RemoveFromRoom(player, room.Id);

                    break;
                }
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

            try
            {
                Process gameServer = new Process();

                gameServer.StartInfo = new ProcessStartInfo(pathGameRoom, $"{freePort} {SQLiteDB.DBPath} -batchmode -nographics -logFile");
                gameServer.StartInfo.UseShellExecute = false;

                gameServer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace.ToString());
            }

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
