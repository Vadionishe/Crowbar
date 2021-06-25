using UnityEngine;
using UnityEngine.UI;

namespace Crowbar
{
    public class SettingProvider : MonoBehaviour
    {
        public Toggle toggleFullScreen;
        public Slider sliderVolume;

        public void SetFullScreen(bool fullScreen)
        {
            Settings.SetFullScreen(fullScreen);
        }

        public void SetVolume(float volume)
        {
            Settings.SetVolume(volume);
        }

        private void Start()
        {
            Settings.LoadSettings();

            toggleFullScreen.isOn = Settings.fullScreen;
            sliderVolume.value = Settings.volume;
        }
    }
}
