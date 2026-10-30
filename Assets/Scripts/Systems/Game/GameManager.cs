using System;
using UnityEngine;

namespace Systems.Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        private GameState _previousState = GameState.MainMenu;

        public event Action<GameState, GameState> StateChanged;

        public bool IsRunning => CurrentState == GameState.Running;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                throw new InvalidOperationException($"{name}: only one GameManager is allowed.");

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Time.timeScale = TimeScaleFor(CurrentState);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetState(GameState state)
        {
            if (state == CurrentState)
                return;

            _previousState = CurrentState;
            CurrentState = state;
            Time.timeScale = TimeScaleFor(state);

            StateChanged?.Invoke(_previousState, CurrentState);
        }
        
        public void TogglePause()
        {
            if (CurrentState == GameState.Running)
                SetState(GameState.Paused);
            else if (CurrentState == GameState.Paused)
                SetState(GameState.Running);
        }

        private static float TimeScaleFor(GameState state)
        {
            return state == GameState.Paused || state == GameState.GameOver || state == GameState.Win ? 0f : 1f;
        }
    }
}
