using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

namespace Crowbar.Server
{    
    /// <summary>
    /// Main logic for connecting to main server
    /// </summary>
    public class PlayerInstance : NetworkBehaviour
    {
        private UIController uIController;

        #region Functions
        [TargetRpc]
        public void TargerCallbackAuthentication(NetworkConnection connection, string data)
        {            
            string[] parceData = data.Split(':');

            if (parceData[0] == "true")
            {
                Account.IsAuthentication = true;
                Account.IsGuest = false;
                Account.Login = parceData[1];
                Account.Password = parceData[2];
                Account.Name = parceData[3];
                Account.Gold = int.Parse(parceData[4]);

                uIController.SetActivateWindow(uIController.mainMenu);
                uIController.SetDeactivateWindow(uIController.authenticationWindow);

                FindObjectOfType<ClientMenu>().SetInfoAccount();
            }
            else
            {
                Account.IsAuthentication = false;
                Debug.Log("Failure Authentication!");
            }

            uIController.SetWaitScreen(false);
        }

        [TargetRpc]
        public void TargerCallbackRegistration(NetworkConnection connection, string data)
        {
            string[] parceData = data.Split(':');

            if (parceData[0] == "true")
            {                
                Account.IsAuthentication = true;
                Account.IsGuest = false;
                Account.Login = parceData[1];
                Account.Password = parceData[2];
                Account.Name = parceData[3];
                Account.Gold = int.Parse(parceData[4]);

                uIController.SetActivateWindow(uIController.mainMenu);
                uIController.SetDeactivateWindow(uIController.registrationWindow);

                FindObjectOfType<ClientMenu>().SetInfoAccount();
            }
            else
            {
                Account.IsAuthentication = false;
                Debug.Log("Failure Registration!");
            }

            uIController.SetWaitScreen(false);
        }

        public void Authentication(string login, string password)
        {
            uIController.SetWaitScreen(true);
            CmdAuthentication(login, password);
        }

        public void Registration(string login, string password, string name)
        {
            uIController.SetWaitScreen(true);
            CmdRegistration(login, password, name);
        }

        [Command]
        public void CmdAuthentication(string login, string password)
        {
            string _password = SQLiteDB.ExecuteRequestWithAnswer($"SELECT Password FROM Accounts WHERE Login = '{login}';");

            if (password == _password)
            {
                string name = SQLiteDB.ExecuteRequestWithAnswer($"SELECT CharacterName FROM Accounts WHERE Login = '{login}';");
                string gold = SQLiteDB.ExecuteRequestWithAnswer($"SELECT Gold FROM Accounts WHERE Login = '{login}';");

                TargerCallbackAuthentication(netIdentity.connectionToClient, $"true:{login}:{password}:{name}:{gold}");
                Debug.Log($"[{name}] login!");
            }
            else
            {
                TargerCallbackAuthentication(netIdentity.connectionToClient, $"false:{login}:{password}");
                Debug.Log($"[{password}] incorrect!");
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
                TargerCallbackRegistration(netIdentity.connectionToClient, $"false:{login}");
                Debug.Log($"[{login}] exist!");
            }
            else
            {
                if (existNickname)
                {
                    TargerCallbackRegistration(netIdentity.connectionToClient, $"false:{login}");
                    Debug.Log($"[{name}] exist!");
                }
                else
                {
                    SQLiteDB.ExecuteRequestWithoutAnswer($"INSERT INTO Accounts (Login, Password, CharacterName) VALUES('{login}', '{password}', '{name}');");

                    string gold = SQLiteDB.ExecuteRequestWithAnswer($"SELECT Gold FROM Accounts WHERE Login = '{login}';");

                    TargerCallbackRegistration(netIdentity.connectionToClient, $"true:{login}:{password}:{name}:{gold}");
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

        public void SetReady(bool isReady)
        {
            CmdReady(isReady);
        }

        public void ForceGameStart()
        {
            ClientGame.port = 8001;
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
                MainServer.DisconnectHandler.Invoke(this);
            }
        }
        #endregion
    }
}
