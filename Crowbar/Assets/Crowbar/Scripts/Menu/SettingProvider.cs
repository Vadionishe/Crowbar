using Crowbar.Server;
using UnityEngine;
using UnityEngine.UI;

namespace Crowbar
{
    public class SettingProvider : MonoBehaviour
    {
        public Toggle toggleFullScreen;
        public Toggle rememberAccount;
        public Slider sliderVolume;

        public void SetFullScreen(bool fullScreen)
        {
            Settings.SetFullScreen(fullScreen);
        }

        public void SetVolume(float volume)
        {
            Settings.SetVolume(volume);
        }

        public void SetRemember(bool isRemember)
        {
            Settings.SetRemember(isRemember);
        }

        private void Start()
        {
            Settings.LoadSettings();

            toggleFullScreen.isOn = Settings.fullScreen;
            sliderVolume.value = Settings.volume;
            rememberAccount.isOn = Settings.remember;

            if (Settings.remember)
            {
                ClientMenu clientMenu = FindObjectOfType<ClientMenu>();

                clientMenu.logAuth.text = Settings.login;
                clientMenu.passAuth.text = Settings.password;
            }
        }
    }
}
