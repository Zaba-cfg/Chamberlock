using System;
using UnityEngine;

namespace Systems.Settings
{
    public static class GameSettings
    {
        private const string MusicVolumeKey = "settings.musicVolume";
        private const string SfxVolumeKey = "settings.sfxVolume";
        private const string FullscreenKey = "settings.fullscreen";
        private const string ResolutionWidthKey = "settings.resolutionWidth";
        private const string ResolutionHeightKey = "settings.resolutionHeight";

        public const float DefaultMusicVolume = 0.8f;
        public const float DefaultSfxVolume = 1f;

        public static float MusicVolume { get; private set; } = DefaultMusicVolume;
        public static float SfxVolume { get; private set; } = DefaultSfxVolume;
        public static bool Fullscreen { get; private set; } = true;
        public static int ResolutionWidth { get; private set; } = Screen.width;
        public static int ResolutionHeight { get; private set; } = Screen.height;

        public static event Action<float> MusicVolumeChanged;
        public static event Action<float> SfxVolumeChanged;
        public static event Action DisplayChanged;
        
        public static void Load()
        {
            MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, DefaultMusicVolume);
            SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, DefaultSfxVolume);
            Fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
            ResolutionWidth = PlayerPrefs.GetInt(ResolutionWidthKey, Screen.width);
            ResolutionHeight = PlayerPrefs.GetInt(ResolutionHeightKey, Screen.height);
        }
        
        public static void Save()
        {
            PlayerPrefs.Save();
        }

        public static void SetMusicVolume(float value)
        {
            value = Mathf.Clamp01(value);

            if (Mathf.Approximately(value, MusicVolume))
                return;

            MusicVolume = value;
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
            MusicVolumeChanged?.Invoke(value);
        }

        public static void SetSfxVolume(float value)
        {
            value = Mathf.Clamp01(value);

            if (Mathf.Approximately(value, SfxVolume))
                return;

            SfxVolume = value;
            PlayerPrefs.SetFloat(SfxVolumeKey, value);
            SfxVolumeChanged?.Invoke(value);
        }

        public static void SetFullscreen(bool value)
        {
            if (value == Fullscreen)
                return;

            Fullscreen = value;
            PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);

            ApplyDisplaySettings();
            DisplayChanged?.Invoke();
        }

        public static void SetResolution(int width, int height)
        {
            if (width == ResolutionWidth && height == ResolutionHeight)
                return;

            ResolutionWidth = width;
            ResolutionHeight = height;
            PlayerPrefs.SetInt(ResolutionWidthKey, width);
            PlayerPrefs.SetInt(ResolutionHeightKey, height);

            ApplyDisplaySettings();
            DisplayChanged?.Invoke();
        }

        public static void ApplyDisplaySettings()
        {
            if (ResolutionWidth <= 0 || ResolutionHeight <= 0)
                return;

            Screen.SetResolution(ResolutionWidth, ResolutionHeight, Fullscreen);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            MusicVolume = DefaultMusicVolume;
            SfxVolume = DefaultSfxVolume;
            Fullscreen = true;
            ResolutionWidth = Screen.width;
            ResolutionHeight = Screen.height;

            MusicVolumeChanged = null;
            SfxVolumeChanged = null;
            DisplayChanged = null;
        }
    }
}
