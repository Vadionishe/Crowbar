using Crowbar.Server;
using UnityEngine;
using UnityEngine.UI;

namespace Crowbar
{
    public class UIController : MonoBehaviour
    {
        public GameObject authenticationWindow;
        public GameObject registrationWindow;
        public GameObject mainMenu;
        public GameObject waitScreen;
        public GameObject messageScreen;
        public GameObject settingScreen;
        public GameObject recordsScreen;
        public GameObject achivmentScreen;

        public Text textMessage;
        public Text textFoundPlayers;

        public void SetFoundPlayersText(string playerFoundText)
        {
            textFoundPlayers.text = playerFoundText;
        }

        public void SetMessageScreen(string message)
        {
            messageScreen.SetActive(true);

            textMessage.text = message;
        }

        public void SetWaitScreen(bool isActive)
        {
            waitScreen.SetActive(isActive);
        }

        public void SetActivateWindow(GameObject window)
        {
            window.SetActive(true);
        }

        public void SetDeactivateWindow(GameObject window)
        {
            window.SetActive(false);
        }

        public void Logout()
        {
            Account.Reset();

            SetActivateWindow(authenticationWindow);
            SetDeactivateWindow(mainMenu);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
