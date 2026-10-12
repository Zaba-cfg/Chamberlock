using System;
using System.Collections.Generic;
using Systems.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menus
{
    public class SettingsPanelController : MonoBehaviour
    {
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        [SerializeField] private Button _backButton;

        private readonly List<Vector2Int> _resolutions = new();
        private bool _isInitializing;
        
        public bool IsOpen => gameObject.activeInHierarchy;

        public event Action Opened;
        public event Action Closed;

        public void Open()
        {
            if (gameObject.activeSelf)
                return;

            gameObject.SetActive(true);
            Opened?.Invoke();
        }

        public void Close()
        {
            if (!gameObject.activeSelf)
                return;

            gameObject.SetActive(false);
            Closed?.Invoke();
        }

        public void OnBackPressed()
        {
            Close();
        }

        private void Awake()
        {
            if (_musicSlider == null || _sfxSlider == null || _fullscreenToggle == null ||
                _resolutionDropdown == null || _backButton == null)
                throw new MissingReferenceException($"{name}: one or more settings UI references are not assigned.");
        }

        private void OnEnable()
        {
            _isInitializing = true;

            _musicSlider.value = GameSettings.MusicVolume;
            _sfxSlider.value = GameSettings.SfxVolume;
            _fullscreenToggle.isOn = GameSettings.Fullscreen;

            BuildResolutionOptions();

            _isInitializing = false;
        }

        private void OnDisable()
        {
            GameSettings.Save();
        }

        public void OnMusicVolumeChanged(float value)
        {
            if (_isInitializing)
                return;

            GameSettings.SetMusicVolume(value);
        }

        public void OnSfxVolumeChanged(float value)
        {
            if (_isInitializing)
                return;

            GameSettings.SetSfxVolume(value);
        }

        public void OnFullscreenChanged(bool value)
        {
            if (_isInitializing)
                return;

            GameSettings.SetFullscreen(value);
        }

        public void OnResolutionChanged(int index)
        {
            if (_isInitializing)
                return;

            if (index < 0 || index >= _resolutions.Count)
                return;

            Vector2Int size = _resolutions[index];
            GameSettings.SetResolution(size.x, size.y);
        }

        private void BuildResolutionOptions()
        {
            _resolutions.Clear();
            List<string> options = new();
            int selectedIndex = 0;

            foreach (Resolution resolution in Screen.resolutions)
            {
                Vector2Int size = new(resolution.width, resolution.height);

                if (_resolutions.Contains(size))
                    continue;

                _resolutions.Add(size);
                options.Add($"{size.x} × {size.y}");

                if (size.x == GameSettings.ResolutionWidth && size.y == GameSettings.ResolutionHeight)
                    selectedIndex = _resolutions.Count - 1;
            }

            if (_resolutions.Count == 0)
            {
                _resolutions.Add(new Vector2Int(GameSettings.ResolutionWidth, GameSettings.ResolutionHeight));
                options.Add($"{GameSettings.ResolutionWidth} × {GameSettings.ResolutionHeight}");
                selectedIndex = 0;
            }

            _resolutionDropdown.ClearOptions();
            _resolutionDropdown.AddOptions(options);
            _resolutionDropdown.SetValueWithoutNotify(selectedIndex);
            _resolutionDropdown.RefreshShownValue();
        }
    }
}
