using Systems.Game;
using TMPro;
using UnityEngine;

namespace UI.Menus
{
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _killsText;
        [SerializeField] private TextMeshProUGUI _roomsText;

        private void Awake()
        {
            if (_panel == null)
                throw new MissingReferenceException($"{name}: panel is not assigned.");

            if (_timeText == null)
                throw new MissingReferenceException($"{name}: time text is not assigned.");

            if (_killsText == null)
                throw new MissingReferenceException($"{name}: kills text is not assigned.");

            if (_roomsText == null)
                throw new MissingReferenceException($"{name}: rooms text is not assigned.");

            if (GameManager.Instance == null)
                throw new MissingReferenceException($"{name}: no GameManager found. Start the game from the Bootstrap scene.");

            _panel.SetActive(false);
        }

        private void OnEnable()
        {
            GameManager.Instance.StateChanged += HandleStateChanged;

            if (GameManager.Instance.CurrentState == GameState.GameOver)
                Show();
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StateChanged -= HandleStateChanged;
        }

        public void Restart()
        {
            if (SceneLoader.Instance.IsLoading)
                return;

            RunManager.Instance.StartRun();
            GameManager.Instance.SetState(GameState.Running);
            SceneLoader.Instance.Load(SceneId.Gameplay);
        }

        public void GoToMainMenu()
        {
            if (SceneLoader.Instance.IsLoading)
                return;

            RunManager.Instance.EndRun();
            GameManager.Instance.SetState(GameState.MainMenu);
            SceneLoader.Instance.Load(SceneId.MainMenu);
        }

        private void HandleStateChanged(GameState previous, GameState current)
        {
            if (current == GameState.GameOver)
            {
                Show();
                return;
            }

            _panel.SetActive(false);
        }

        private void Show()
        {
            _timeText.text = FormatTime(RunManager.Instance.ElapsedTime);
            _killsText.text = RunManager.Instance.Kills.ToString();
            _roomsText.text = RunManager.Instance.RoomsCleared.ToString();

            _panel.SetActive(true);
        }

        private static string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.FloorToInt(seconds);
            int minutes = totalSeconds / 60;
            int remainingSeconds = totalSeconds % 60;
            return $"{minutes:00}:{remainingSeconds:00}";
        }
    }
}
