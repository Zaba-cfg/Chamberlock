using Components.Base.HealthRelated;
using Systems.Game;
using UnityEngine;

namespace Components.EnemyComponents
{
    [RequireComponent(typeof(Health))]
    public class EnemyKillCounter : MonoBehaviour
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
            if (RunManager.Instance == null)
                return;

            RunManager.Instance.AddKill();
        }
    }
}
