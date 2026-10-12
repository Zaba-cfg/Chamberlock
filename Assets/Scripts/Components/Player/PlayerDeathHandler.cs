using Components.Base.HealthRelated;
using Systems.Game;
using UnityEngine;

namespace Components.Player
{
    [RequireComponent(typeof(Health))]
    public class PlayerDeathHandler : MonoBehaviour
    {
        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            _health.Died += HandleDied;
        }

        private void OnDisable()
        {
            _health.Died -= HandleDied;
        }

        private void HandleDied()
        {
            if (GameManager.Instance == null)
                return;

            if (RunManager.Instance != null)
                RunManager.Instance.EndRun();

            GameManager.Instance.SetState(GameState.GameOver);
        }
    }
}
