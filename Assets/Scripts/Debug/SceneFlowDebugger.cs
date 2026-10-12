using Systems.Game;
using UnityEngine;

namespace Debug
{
    /// <summary>
    /// Temporary Stage A verification tool. Exercises game states, scene transitions and run stats
    /// before the real menus exist. Remove once MainMenu/Pause menus are in place.
    /// </summary>
    public class SceneFlowDebugger : MonoBehaviour
    {
        [ContextMenu("Start Run")]
        public void StartRun()
        {
            if (!HasManagers())
                return;

            GameManager.Instance.SetState(GameState.Running);
            RunManager.Instance.StartRun();
            SceneLoader.Instance.Load(SceneId.Gameplay);
        }

        [ContextMenu("Return To Menu")]
        public void ReturnToMenu()
        {
            if (!HasManagers())
                return;

            RunManager.Instance.EndRun();
            GameManager.Instance.SetState(GameState.MainMenu);
            SceneLoader.Instance.Load(SceneId.MainMenu);
        }

        [ContextMenu("Toggle Pause")]
        public void TogglePause()
        {
            if (!HasManagers())
                return;

            GameManager.Instance.TogglePause();
        }

        [ContextMenu("Force Game Over")]
        public void ForceGameOver()
        {
            if (!HasManagers())
                return;

            GameManager.Instance.SetState(GameState.GameOver);
        }

        [ContextMenu("Simulate Kill")]
        public void SimulateKill()
        {
            if (!HasManagers())
                return;

            RunManager.Instance.AddKill();
        }

        [ContextMenu("Simulate Room Cleared")]
        public void SimulateRoomCleared()
        {
            if (!HasManagers())
                return;

            RunManager.Instance.AddRoomCleared();
        }

        [ContextMenu("Log State")]
        public void LogState()
        {
            if (!HasManagers())
                return;

            UnityEngine.Debug.Log(
                $"State: {GameManager.Instance.CurrentState} | TimeScale: {Time.timeScale} | " +
                $"Running: {RunManager.Instance.IsRunning} | Time: {RunManager.Instance.ElapsedTime:F1}s | " +
                $"Kills: {RunManager.Instance.Kills} | Rooms: {RunManager.Instance.RoomsCleared}");
        }

        private bool HasManagers()
        {
            if (GameManager.Instance != null && RunManager.Instance != null && SceneLoader.Instance != null)
                return true;

            UnityEngine.Debug.LogError(
                $"{name}: persistent managers not found. Press Play from the Bootstrap scene, " +
                "not from this scene directly.");
            return false;
        }
    }
}
