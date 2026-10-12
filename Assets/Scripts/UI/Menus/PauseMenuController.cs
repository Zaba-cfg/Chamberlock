using Systems.Game;
using UnityEngine;

namespace UI.Menus
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject _menuRoot;
        [SerializeField] private SettingsPanelController _settingsPanel;

        private void Awake()
        {
            if (_menuRoot == null)
                throw new MissingReferenceException($"{name}: menu root is not assigned.");

            if (_settingsPanel == null)
                throw new MissingReferenceException($"{name}: settings panel is not assigned.");

            if (GameManager.Instance == null)
                throw new MissingReferenceException($"{name}: no GameManager found. Start the game from the Bootstrap scene.");

            _menuRoot.SetActive(false);
        }

        private void OnEnable()
        {
            PauseInput.Instance.PausePressed += HandlePausePressed;
            GameManager.Instance.StateChanged += HandleStateChanged;
            _settingsPanel.Closed += ShowMenuItems;
        }

        private void OnDisable()
        {
            if (PauseInput.Instance != null)
                PauseInput.Instance.PausePressed -= HandlePausePressed;

            if (GameManager.Instance != null)
                GameManager.Instance.StateChanged -= HandleStateChanged;

            _settingsPanel.Closed -= ShowMenuItems;
        }

        public void Resume()
        {
            GameManager.Instance.SetState(GameState.Running);
        }

        public void OpenSettings()
        {
            SetMenuItemsVisible(false);
            _settingsPanel.Open();
        }

        public void QuitToMenu()
        {
            if (SceneLoader.Instance.IsLoading)
                return;

            RunManager.Instance.EndRun();
            GameManager.Instance.SetState(GameState.MainMenu);
            SceneLoader.Instance.Load(SceneId.MainMenu);
        }

        private void HandlePausePressed()
        {
            if (_settingsPanel.IsOpen)
            {
                _settingsPanel.Close();
                return;
            }

            GameManager.Instance.TogglePause();
        }

        private void HandleStateChanged(GameState previous, GameState current)
        {
            if (current == GameState.Paused)
            {
                _settingsPanel.Close();
                _menuRoot.SetActive(true);
                ShowMenuItems();
                return;
            }

            _settingsPanel.Close();
            _menuRoot.SetActive(false);
        }
        
        private void SetMenuItemsVisible(bool visible)
        {
            foreach (Transform child in _menuRoot.transform)
            {
                if (child == _settingsPanel.transform)
                    continue;

                child.gameObject.SetActive(visible);
            }
        }

        private void ShowMenuItems()
        {
            SetMenuItemsVisible(true);
        }
    }
}
