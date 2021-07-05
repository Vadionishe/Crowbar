using System;
using UnityEngine;

namespace Crowbar
{
    public static class Settings
    {
        public static bool fullScreen;
        public static bool remember;
        public static float volume;

        public static string login;
        public static string password;

        public static void SaveSettings()
        {
            PlayerPrefs.SetInt("fullScreen", Convert.ToInt32(fullScreen));
            PlayerPrefs.SetFloat("volume", volume);
            PlayerPrefs.SetInt("remember", Convert.ToInt32(remember));
            PlayerPrefs.Save();
        }

        public static void SaveAuth(string login, string password)
        {
            PlayerPrefs.SetString("login", login);
            PlayerPrefs.SetString("password", password);
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

            if (PlayerPrefs.HasKey("remember"))
            {
                remember = Convert.ToBoolean(PlayerPrefs.GetInt("remember"));
            }
            else
            {
                remember = false;
                PlayerPrefs.SetInt("remember", Convert.ToInt32(remember));
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

            if (PlayerPrefs.HasKey("login"))
            {
                login = PlayerPrefs.GetString("login");
            }
            else
            {
                PlayerPrefs.SetString("login", login);
            }

            if (PlayerPrefs.HasKey("password"))
            {
                password = PlayerPrefs.GetString("password");
            }
            else
            {
                PlayerPrefs.SetString("password", password);
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

        public static void SetRemember(bool isRemember)
        {
            remember = isRemember;

            SaveSettings();
        }
    }
}
