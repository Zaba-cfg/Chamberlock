using Systems.Audio;
using Systems.Settings;
using UnityEngine;

namespace Systems.Game
{
    [RequireComponent(typeof(GameManager))]
    [RequireComponent(typeof(RunManager))]
    [RequireComponent(typeof(SceneLoader))]
    [RequireComponent(typeof(AudioManager))]
    
    public class Bootstrap : MonoBehaviour
    {
        private void Start()
        {
            if (GameManager.Instance == null || RunManager.Instance == null || SceneLoader.Instance == null ||
                AudioManager.Instance == null)
                throw new MissingReferenceException(
                    $"{name}: Bootstrap needs GameManager, RunManager, SceneLoader and AudioManager on the same persistent object.");

            GameSettings.Load();
            GameSettings.ApplyDisplaySettings();
            AudioManager.Instance.ApplyVolumes();

            GameManager.Instance.SetState(GameState.MainMenu);
            SceneLoader.Instance.Load(SceneId.MainMenu);
        }
    }
}
