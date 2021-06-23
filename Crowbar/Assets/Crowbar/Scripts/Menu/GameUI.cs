using UnityEngine;

namespace Crowbar
{
    public class GameUI : MonoBehaviour
    {
        public static GameUI gameUI;
        public static CharacterStats localStats;
        public static bool lockInput;
        
        public GameObject loadScreen;
        public GameObject menuScreen;
        public GameObject settingScreen;
        public GameObject requestQuit;
        public GameObject requestDisconnect;

        public static void Initialize(Character localCharacter)
        {
            localStats = localCharacter.GetComponent<CharacterStats>();
        }

        public static void SetLoadScreen(bool isActive)
        {
            gameUI.loadScreen.SetActive(isActive);
        }

        public void CloseAllWindow()
        {
            SetWindowDeactive(menuScreen);
            SetWindowDeactive(settingScreen);
            SetWindowDeactive(requestQuit);
            SetWindowDeactive(requestDisconnect);
        }

        public void SetWindowActive(GameObject window)
        {
            window.SetActive(true);
        }

        public void SetWindowDeactive(GameObject window)
        {
            window.SetActive(false);
        }

        public void RequestDisconnect()
        {
            SetWindowDeactive(menuScreen);
            SetWindowActive(requestDisconnect);
        }

        public void RequestQuit()
        {
            SetWindowDeactive(menuScreen);
            SetWindowActive(requestQuit);
        }

        private bool IsLockInput()
        {
            return menuScreen.activeSelf ||
                settingScreen.activeSelf ||
                requestDisconnect.activeSelf ||
                requestQuit.activeSelf ||
                Chat.lockInput;
        }

        private void Awake()
        {
            gameUI = this;
        }

        private void Update()
        {
            lockInput = IsLockInput();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (lockInput)
                {
                    CloseAllWindow();

                    return;
                }
                else
                {
                    SetWindowActive(menuScreen);
                }
            }
        }
    }
}
