using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

        public GameObject notificationScreen;
        public Image notificationImage;
        public TextMeshProUGUI notificationText;

        public Image healthHUD;
        public Image oxygenHUD;
        public Image foodHUD;

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

        public void ShowNotification(Sprite image, string text, float time)
        {
            StopAllCoroutines();

            notificationImage.sprite = image;
            notificationText.text = text;

            StartCoroutine(ShowNotification(time));
        }

        private bool IsLockInput()
        {
            return menuScreen.activeSelf ||
                settingScreen.activeSelf ||
                requestDisconnect.activeSelf ||
                requestQuit.activeSelf ||
                Chat.lockInput;
        }

        private void UpdateHUD()
        {
            if (localStats != null)
            {
                healthHUD.fillAmount = localStats.health / localStats.maxHealth;
                oxygenHUD.fillAmount = localStats.oxygen / localStats.maxOxygen;
                foodHUD.fillAmount = localStats.food / localStats.maxFood;
            }
        }

        private IEnumerator ShowNotification(float time)
        {
            notificationScreen.SetActive(true);

            yield return new WaitForSeconds(time);

            notificationScreen.SetActive(false);
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

            UpdateHUD();
        }
    }
}
