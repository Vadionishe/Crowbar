using Crowbar.Server;
using UnityEngine;

namespace Crowbar
{
    public class UIController : MonoBehaviour
    {
        public GameObject authenticationWindow;
        public GameObject registrationWindow;
        public GameObject mainMenu;
        public GameObject waitScreen;

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
            Account.IsAuthentication = false;

            SetActivateWindow(authenticationWindow);
            SetDeactivateWindow(mainMenu);
        }
    }
}
