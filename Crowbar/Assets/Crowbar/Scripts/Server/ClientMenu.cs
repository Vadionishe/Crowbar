using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace Crowbar.Server
{
    /// <summary>
    /// Client logic for connecting to main server
    /// </summary>
    public class ClientMenu : MonoBehaviour
    {
        public static PlayerInstance localPlayerInstance;

        public InputField logReg;
        public InputField passReg;
        public InputField nameReg;

        public InputField logAuth;
        public InputField passAuth;

        public InputField portForce;

        public int minLoginLenght = 3;
        public int minPasswordLenght = 6;
        public int minNameLenght = 2;

        public Text textDebug;

        public Texture2D cursor;

        #region Functions
        public void SetInfoAccount()
        {
            textDebug.text = $"Name: {Account.Name}\nGold: {Account.Gold}";
        }

        public void SetReady(bool isReady)
        {
            if (localPlayerInstance != null && Account.IsAuthentication)
                localPlayerInstance.SetReady(isReady);
        }

        public void ForceStartGame()
        {
            if (localPlayerInstance != null)
                localPlayerInstance.ForceGameStart(portForce.text);
        }

        public void Authentication()
        {
            if (localPlayerInstance != null)
            {
                localPlayerInstance.Authentication(logAuth.text, passAuth.text);

                if (Settings.remember)
                {
                    Settings.SaveAuth(logAuth.text, passAuth.text);
                }
                else
                {
                    logAuth.text = string.Empty;
                    passAuth.text = string.Empty;
                }
            }
        }

        public void Registration()
        {
            if (localPlayerInstance != null)
            {
                bool isValid = IsValidRegistration(out string message);

                if (isValid)
                {
                    localPlayerInstance.Registration(logReg.text, passReg.text, nameReg.text);

                    logReg.text = string.Empty;
                    passReg.text = string.Empty;
                    nameReg.text = string.Empty;
                }
                else
                {
                    FindObjectOfType<UIController>().SetMessageScreen(message);
                }
            }
        }

        private bool IsValidRegistration(out string messageValidate)
        {
            messageValidate = string.Empty;

            if (logReg.text.Length < minLoginLenght) 
            {
                messageValidate = $"Login must be more than {minLoginLenght - 1} characters";

                return false;
            }

            if (passReg.text.Length < minPasswordLenght)
            {
                messageValidate = $"Password must be more than {minPasswordLenght - 1} characters";

                return false;
            }

            if (nameReg.text.Length < minNameLenght)
            {
                messageValidate = $"Nickname must be more than {minNameLenght - 1} characters";

                return false;
            }

            return true;
        }

        private void CheckFailConnect()
        {
            if (!NetworkClient.isConnected)
                FindObjectOfType<UIController>().SetMessageScreen("Connection error! Please, restart game.");
        }

        private void Awake()
        {
            Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
        }

        /// <summary>
        /// At startup, connect to the main server
        /// </summary>
        private void Start()
        {
            UIController uIController = FindObjectOfType<UIController>();

            NetworkManager.singleton.StartClient();

            if (Account.IsAuthentication)
            {
                uIController.SetActivateWindow(uIController.mainMenu);
            }
            else
            {
                uIController.SetActivateWindow(uIController.authenticationWindow);
            }

            Invoke(nameof(CheckFailConnect), 4f);
        }
        #endregion
    }
}
