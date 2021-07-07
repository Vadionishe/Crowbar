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
        private UIController uIController;

        #region Functions
        public override void OnStopClient()
        {
            base.OnStopClient();

            if (isLocalPlayer)
                uIController.SetMessageScreen("Connection lost! Please, restart game.");
        }

        public void GetRecords(int countRecords)
        {
            CmdGetRecords(countRecords);

            uIController.SetWaitScreen(true);
        }

        public void BuyHat(int id, string name, int price)
        {
            CmdBuyHat(id, name, price);

            uIController.SetWaitScreen(true);
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
            uIController.SetWaitScreen(false);

            if (!string.IsNullOrEmpty(message))
                uIController.SetMessageScreen(message);

            if (isAccess)
            {
                SkinHat skinHat = FindObjectOfType<SkinShop>().hats.Find(h => h.id == id);

                if (skinHat != null) 
                {
                    Account.Gold -= skinHat.price;

                    skinHat.SetBuy();
                    FindObjectOfType<ClientMenu>().SetInfoAccount();
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

            uIController.SetWaitScreen(false);
        }

        [TargetRpc]
        public void TargerCallbackAuthentication(NetworkConnection connection, string data, string message)
        {            
            string[] parceData = data.Split(':');

            if (parceData[0] == "true")
            {
                Account.MapAccount(data);

                uIController.SetActivateWindow(uIController.mainMenu);
                uIController.SetDeactivateWindow(uIController.authenticationWindow);

                FindObjectOfType<ClientMenu>().SetInfoAccount();
                FindObjectOfType<ManagerAchivments>().SetActiveAchivments(Account.idAchivments);
                FindObjectOfType<SkinShop>().LoadHats(Account.idHats, Account.idCurrentHat);
            }
            else
            {
                Account.Reset();

                uIController.SetMessageScreen(message);
            }

            uIController.SetWaitScreen(false);
        }

        [TargetRpc]
        public void TargerCallbackRegistration(NetworkConnection connection, string data, string message)
        {
            string[] parceData = data.Split(':');

            if (parceData[0] == "true")
            {
                Account.MapAccount(data);

                uIController.SetActivateWindow(uIController.mainMenu);
                uIController.SetDeactivateWindow(uIController.registrationWindow);

                FindObjectOfType<ClientMenu>().SetInfoAccount();
            }
            else
            {
                Account.Reset();

                uIController.SetMessageScreen(message);
            }

            uIController.SetWaitScreen(false);
        }

        public void Authentication(string login, string password)
        {
            uIController.SetWaitScreen(true);
            CmdAuthentication(login, password, Application.version);
        }

        public void Registration(string login, string password, string name)
        {
            uIController.SetWaitScreen(true);
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
                    string name = SQLiteDB.ExecuteRequestWithAnswer($"SELECT CharacterName FROM Accounts WHERE Login = '{login}';");
                    string gold = SQLiteDB.ExecuteRequestWithAnswer($"SELECT Gold FROM Accounts WHERE Login = '{login}';");
                    string currentHat = SQLiteDB.ExecuteRequestWithAnswer($"SELECT Skin FROM Accounts WHERE Login = '{login}';");
                    string achivments = SQLiteDB.ExecuteRequestWithAnswer($"SELECT AchivmentsId FROM Accounts WHERE Login = '{login}';");
                    string hats = SQLiteDB.ExecuteRequestWithAnswer($"SELECT SkinsId FROM Accounts WHERE Login = '{login}';");

                    TargerCallbackAuthentication(netIdentity.connectionToClient, $"true:{login}:{password}:{name}:{gold}:{achivments}:{currentHat}:{hats}", "Access!");
                    Debug.Log($"[{name}] login!");
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
        public void CmdReady(bool isReady)
        {
            if (isReady)
            {
                MainServer.ReadyHandler?.Invoke(this);
            }
            else
            {
                MainServer.DontReadyHandler?.Invoke(this);
            }
        }

        [TargetRpc]
        public void TargetSetTextPlayersFound(NetworkConnection connection, string textPlayersFound)
        {
            uIController.SetFoundPlayersText(textPlayersFound);
        }

        public void SetReady(bool isReady)
        {
            CmdReady(isReady);

            if (!isReady)
                uIController.SetFoundPlayersText(string.Empty);
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
                uIController = FindObjectOfType<UIController>();
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
