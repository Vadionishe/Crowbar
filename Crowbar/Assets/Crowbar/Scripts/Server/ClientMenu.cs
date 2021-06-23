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

        public Text textDebug;

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
                localPlayerInstance.ForceGameStart();
        }

        public void Authentication()
        {
            if (localPlayerInstance != null)
            {
                localPlayerInstance.Authentication(logAuth.text, passAuth.text);

                logAuth.text = string.Empty;
                passAuth.text = string.Empty;
            }
        }

        public void Registration()
        {
            if (localPlayerInstance != null)
            {
                localPlayerInstance.Registration(logReg.text, passReg.text, nameReg.text);

                logReg.text = string.Empty;
                passReg.text = string.Empty;
                nameReg.text = string.Empty;
            }
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
        }
        #endregion
    }
}
