using UnityEngine;

namespace Systems.Game
{
    [RequireComponent(typeof(GameManager))]
    [RequireComponent(typeof(RunManager))]
    [RequireComponent(typeof(SceneLoader))]
    
    public class Bootstrap : MonoBehaviour
    {
        private void Start()
        {
            if (GameManager.Instance == null || RunManager.Instance == null || SceneLoader.Instance == null)
                throw new MissingReferenceException(
                    $"{name}: Bootstrap needs GameManager, RunManager and SceneLoader on the same persistent object.");

            GameManager.Instance.SetState(GameState.MainMenu);
            SceneLoader.Instance.Load(SceneId.MainMenu);
        }
    }
}
