using Systems.Game;
using UnityEngine;

namespace UI.Menus
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private SettingsPanelController _settingsPanel;
        [SerializeField] private GameObject _menuPanel;

        private void Awake()
        {
            if (_settingsPanel == null)
                throw new MissingReferenceException($"{name}: settings panel is not assigned.");

            if (_menuPanel == null)
                throw new MissingReferenceException($"{name}: menu panel is not assigned.");
        }

        private void OnEnable()
        {
            _settingsPanel.Closed += ShowMenuItems;
            _settingsPanel.Close();
            ShowMenuItems();
        }

        private void OnDisable()
        {
            _settingsPanel.Closed -= ShowMenuItems;
        }

        public void NewGame()
        {
            if (SceneLoader.Instance.IsLoading)
                return;

            RunManager.Instance.StartRun();
            GameManager.Instance.SetState(GameState.Running);
            SceneLoader.Instance.Load(SceneId.Gameplay);
        }

        public void OpenSettings()
        {
            SetMenuItemsVisible(false);
            _settingsPanel.Open();
        }

        public void Quit()
        {
            QuitGame.Quit();
        }
        
        private void SetMenuItemsVisible(bool visible)
        {
            foreach (Transform child in _menuPanel.transform)
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
