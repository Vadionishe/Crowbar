using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections.Generic;

namespace Crowbar.Server
{
    /// <summary>
    /// Main logic for connecting to main server
    /// </summary>
    public class PlayerInstance : NetworkBehaviour
    {
        public string nameCrow;

        public int currentRoom;
        public bool isReady;

        private UIController uiController;
        private ClientMenu clientMenu;
        private RoomManager roomManager;
        private ChatRoom chatRoom;

        #region Functions
        public override void OnStopClient()
        {
            //DONT WORK!

            base.OnStopClient();

            if (isLocalPlayer)
                uiController.SetMessageScreen("Connection lost! Please, restart game.");
        }

        public void SendMessageToRoom(string message)
        {
            CmdSendMessage(message);
        }

        public void GoToRoom(int id)
        {
            CmdGoToRoom(id);
        }

        public void LeaveRoom()
        {
            CmdLeaveRoom(currentRoom);
        }

        public void CreateRoom()
        {
            CmdCreateRoom();
        }

        [Command]
        public void CmdSendMessage(string message)
        {
            MainServer.SendMessage(this, message);
        }

        [Command]
        public void CmdCreateRoom()
        {
            MainServer.CreateRoom(this, nameCrow, string.Empty);
        }

        [Command]
        public void CmdGoToRoom(int id)
        {
            MainServer.EnterRoom(this, id);
        }

        [Command]
        public void CmdLeaveRoom(int id)
        {
            MainServer.RemoveFromRoom(this, id);
        }

        [TargetRpc]
        public void TargetSendMessage(NetworkConnection connection, string message)
        {
            chatRoom.ReceiveMessage(message);
        }

        [TargetRpc]
        public void TargetSetTextPlayers(NetworkConnection connection, string textPlayers)
        {
            roomManager.UpdatePlayerRoom(textPlayers);
        }

        [TargetRpc]
        public void TargetGoRoom(NetworkConnection connection, int id, string message)
        {
            uiController.SetWaitScreen(false);

            if (string.IsNullOrEmpty(message))
            {
                currentRoom = id;

                uiController.SetDeactivateWindow(uiController.rooms);
                uiController.SetActivateWindow(uiController.room);
            }
            else
            {
                uiController.SetMessageScreen(message);
            }
        }

        [TargetRpc]
        public void TargetLeaveRoom(NetworkConnection connection)
        {
            uiController.SetWaitScreen(false);
            uiController.SetDeactivateWindow(uiController.room);
            uiController.SetActivateWindow(uiController.rooms);
        }

        [TargetRpc]
        public void TargetSpawnRoom(NetworkConnection connection, int id, string info)
        {
            roomManager.SpawnRoom(id, info);
        }

        [TargetRpc]
        public void TargetUpdateRoom(NetworkConnection connection, int id, string info)
        {
            roomManager.UpdateRoom(id, info);
        }

        [TargetRpc]
        public void TargetRemoveRoom(NetworkConnection connection, int id)
        {
            roomManager.RemoveRoom(id);
        }

        public void GetRecords(int countRecords)
        {
            CmdGetRecords(countRecords);

            uiController.SetWaitScreen(true);
        }

        public void BuyHat(int id, string name, int price)
        {
            CmdBuyHat(id, name, price);

            uiController.SetWaitScreen(true);
        }

        [Client]
        public void SetHat(int id, string name)
        {
            CmdSetHat(id, name);
        }

        [Command]
        public void CmdSetHat(int id, string name)
        {
            SQLiteDB.ExecuteRequestWithoutAnswer($"UPDATE Accounts SET Skin = '{id}' WHERE CharacterName = '{name}';");
        }

        [Command]
        public void CmdBuyHat(int id, string name, int price)
        {
            string hats = SQLiteDB.ExecuteRequestWithAnswer($"SELECT SkinsId FROM Accounts WHERE CharacterName = '{name}';");
            string gold = SQLiteDB.ExecuteRequestWithAnswer($"SELECT Gold FROM Accounts WHERE CharacterName = '{name}';");

            string[] hatsIdString = hats.Split(',');
            List<int> hatsId = new List<int>();

            foreach (string hatId in hatsIdString)
                if (int.TryParse(hatId, out int _idTry))
                    hatsId.Add(_idTry);

            if (!hatsId.Contains(id))
            {
                if (int.Parse(gold) >= price)
                {
                    hats += $"{id},";

                    SQLiteDB.ExecuteRequestWithoutAnswer($"UPDATE Accounts SET SkinsId = '{hats}' WHERE CharacterName = '{name}';");
                    SQLiteDB.ExecuteRequestWithoutAnswer($"UPDATE Accounts SET Gold = Gold - {price} WHERE CharacterName = '{name}';");

                    TargetBuyHat(netIdentity.connectionToClient, true, id, "");
                }
                else
                {
                    TargetBuyHat(netIdentity.connectionToClient, false, id, "Not enough gold!");
                }               
            }
            else
            {
                TargetBuyHat(netIdentity.connectionToClient, false, id, "Not enough gold!");
            }           
        }

        [TargetRpc]
        public void TargetBuyHat(NetworkConnection connection, bool isAccess, int id, string message)
        {
            uiController.SetWaitScreen(false);

            if (!string.IsNullOrEmpty(message))
                uiController.SetMessageScreen(message);

            if (isAccess)
            {
                SkinHat skinHat = FindObjectOfType<SkinShop>().hats.Find(h => h.id == id);

                if (skinHat != null) 
                {
                    Account.Gold -= skinHat.price;

                    skinHat.SetBuy();
                    clientMenu.SetInfoAccount();
                }
            }              
        }

        [Command]
        public void CmdGetRecords(int countRecords)
        {
            List<string> records = SQLiteDB.GetRecords($"SELECT CharacterName, MaxDeep FROM Accounts ORDER BY MaxDeep DESC LIMIT {countRecords};");

            TargetSendRecords(netIdentity.connectionToClient, records);
        }

        [TargetRpc]
        public void TargetSendRecords(NetworkConnection connection, List<string> records)
        {
            FindObjectOfType<RecordManager>().SetRecords(records);

            uiController.SetWaitScreen(false);
        }

        [TargetRpc]
        public void TargerCallbackAuthentication(NetworkConnection connection, string data, string message)
        {            
            string[] parceData = data.Split(':');

            if (parceData[0] == "true")
            {
                Account.MapAccount(data);

                uiController.SetActivateWindow(uiController.mainMenu);
                uiController.SetDeactivateWindow(uiController.authenticationWindow);

                clientMenu.SetInfoAccount();
                FindObjectOfType<ManagerAchivments>().SetActiveAchivments(Account.idAchivments);
                FindObjectOfType<SkinShop>().LoadHats(Account.idHats, Account.idCurrentHat);
            }
            else
            {
                Account.Reset();

                uiController.SetMessageScreen(message);
            }

            uiController.SetWaitScreen(false);
        }

        [TargetRpc]
        public void TargerCallbackRegistration(NetworkConnection connection, string data, string message)
        {
            string[] parceData = data.Split(':');

            if (parceData[0] == "true")
            {
                Account.MapAccount(data);

                uiController.SetActivateWindow(uiController.mainMenu);
                uiController.SetDeactivateWindow(uiController.registrationWindow);

                clientMenu.SetInfoAccount();
            }
            else
            {
                Account.Reset();

                uiController.SetMessageScreen(message);
            }

            uiController.SetWaitScreen(false);
        }

        public void Authentication(string login, string password)
        {
            uiController.SetWaitScreen(true);
            CmdAuthentication(login, password, Application.version);
        }

        public void Registration(string login, string password, string name)
        {
            uiController.SetWaitScreen(true);
            CmdRegistration(login, password, name);
        }

        [Command]
        public void CmdAuthentication(string login, string password, string version)
        {
            if (version == Application.version)
            {
                string _password = SQLiteDB.ExecuteRequestWithAnswer($"SELECT Password FROM Accounts WHERE Login = '{login}';");

                if (password == _password)
                {
                    nameCrow = SQLiteDB.ExecuteRequestWithAnswer($"SELECT CharacterName FROM Accounts WHERE Login = '{login}';");
                    string gold = SQLiteDB.ExecuteRequestWithAnswer($"SELECT Gold FROM Accounts WHERE Login = '{login}';");
                    string currentHat = SQLiteDB.ExecuteRequestWithAnswer($"SELECT Skin FROM Accounts WHERE Login = '{login}';");
                    string achivments = SQLiteDB.ExecuteRequestWithAnswer($"SELECT AchivmentsId FROM Accounts WHERE Login = '{login}';");
                    string hats = SQLiteDB.ExecuteRequestWithAnswer($"SELECT SkinsId FROM Accounts WHERE Login = '{login}';");

                    TargerCallbackAuthentication(netIdentity.connectionToClient, $"true:{login}:{password}:{nameCrow}:{gold}:{achivments}:{currentHat}:{hats}", "Access!");
                    Debug.Log($"[{nameCrow}] login!");
                }
                else
                {
                    TargerCallbackAuthentication(netIdentity.connectionToClient, $"false:{login}:{password}", "Failure authentication!");
                    Debug.Log($"[{password}] incorrect!");
                }
            }
            else
            {
                TargerCallbackAuthentication(netIdentity.connectionToClient, $"false:{login}:{password}", $"Version {version} incorrect! Actual version {Application.version}");
                Debug.Log($"[Version {version}] incorrect!");
            }
        }

        [Command]
        public void CmdRegistration(string login, string password, string name)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(name))
            {
                Debug.Log($"Incorrect input value!");

                return;
            }

            bool existLogin = SQLiteDB.ExecuteRequestWithAnswer($"SELECT count(*) FROM Accounts WHERE Login='{login}';") != "0" ? true : false;
            bool existNickname = SQLiteDB.ExecuteRequestWithAnswer($"SELECT count(*) FROM Accounts WHERE CharacterName='{name}';") != "0" ? true : false;

            if (existLogin)
            {
                TargerCallbackRegistration(netIdentity.connectionToClient, $"false:{login}", "This login exist!");
                Debug.Log($"[{login}] exist!");
            }
            else
            {
                if (existNickname)
                {
                    TargerCallbackRegistration(netIdentity.connectionToClient, $"false:{login}", "This nickname exist!");
                    Debug.Log($"[{name}] exist!");
                }
                else
                {
                    SQLiteDB.ExecuteRequestWithoutAnswer($"INSERT INTO Accounts (Login, Password, CharacterName) VALUES('{login}', '{password}', '{name}');");

                    string gold = SQLiteDB.ExecuteRequestWithAnswer($"SELECT Gold FROM Accounts WHERE Login = '{login}';");

                    TargerCallbackRegistration(netIdentity.connectionToClient, $"true:{login}:{password}:{name}:0:0:0:0", "Access!");
                    Debug.Log($"[{name}] registration success!");
                }
            }
        }

        [Command]
        public void CmdReady(int idRoom, bool isReady)
        {
            MainServer.PlayerReady(this, idRoom, isReady);
        }

        [TargetRpc]
        public void TargetSetTextPlayersFound(NetworkConnection connection, string textPlayersFound)
        {
            uiController.SetFoundPlayersText(textPlayersFound);
        }

        public void SetReady(bool isReady)
        {
            CmdReady(currentRoom, isReady);
        }

        public void ForceGameStart(string port)
        {
            ClientGame.port = ushort.Parse(port);
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene("Game");
        }

        /// <summary>
        /// Fires when the search is full of players
        /// Disconnect from the main server to connect to the game
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="port"></param>
        [TargetRpc]
        public void TargetStartGame(NetworkConnection connection, ushort port)
        {
            ClientGame.port = port;
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene("Game");
        }

        private void Awake()
        {
            uiController = FindObjectOfType<UIController>();
            clientMenu = FindObjectOfType<ClientMenu>();
            roomManager = FindObjectOfType<RoomManager>();
            chatRoom = FindObjectOfType<ChatRoom>();
        }

        /// <summary>
        /// At startup, we immediately connect to the main server
        /// </summary>
        private void Start()
        {
            if (isServer)
            {
                MainServer.ConnectHandler.Invoke(this);
            }
            else if (isLocalPlayer)
            {
                ClientMenu.localPlayerInstance = this;

                if (Account.IsAuthentication)
                    Authentication(Account.Login, Account.Password);
            }
        }

        /// <summary>
        /// In order to avoid exceptions when destroying an object with a script, we disconnect from the main server
        /// </summary>
        private void OnDestroy()
        {
            if (isServer)
            {
                if (gameObject != null)
                    MainServer.DisconnectHandler.Invoke(this);
            }
        }
        #endregion
    }
}
