using System;
using UnityEngine;

namespace Crowbar
{
    public static class Settings
    {
        public static bool fullScreen;
        public static float volume;

        public static void SaveSettings()
        {
            PlayerPrefs.SetInt("fullScreen", Convert.ToInt32(fullScreen));
            PlayerPrefs.SetFloat("volume", volume);
            PlayerPrefs.Save();
        }

        public static void LoadSettings()
        {
            if (PlayerPrefs.HasKey("fullScreen"))
            {
                fullScreen = Convert.ToBoolean(PlayerPrefs.GetInt("fullScreen"));
            }
            else
            {
                fullScreen = true;
                PlayerPrefs.SetInt("fullScreen", Convert.ToInt32(fullScreen));
            }

            if (PlayerPrefs.HasKey("volume"))
            {
                volume = PlayerPrefs.GetFloat("volume");
            }
            else
            {
                volume = 0.5f;
                PlayerPrefs.SetFloat("volume", volume);
            }

            SetFullScreen(fullScreen);
            SetVolume(volume);
        }

        public static void SetFullScreen(bool isFullScreen)
        {
            fullScreen = isFullScreen;
            Screen.fullScreenMode = isFullScreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;

            Screen.SetResolution(1600, 900, fullScreen);

            SaveSettings();
        }

        public static void SetVolume(float volume)
        {
            Settings.volume = volume;

            SaveSettings();
        }
    }
}
