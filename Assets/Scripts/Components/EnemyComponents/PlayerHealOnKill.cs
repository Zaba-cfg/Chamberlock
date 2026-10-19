using Components.Base.HealthRelated;
using Entities.PlayerRelated;
using UnityEngine;

namespace Components.EnemyComponents
{
    [RequireComponent(typeof(Health))]
    public class PlayerHealOnKill : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _healAmount = 10f;

        private Health _health;
        private Health _playerHealth;

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
            Health playerHealth = PlayerHealth;

            if (playerHealth == null)
                return;

            playerHealth.Heal(_healAmount);
        }

        private Health PlayerHealth
        {
            get
            {
                if (_playerHealth == null)
                {
                    Player player = FindFirstObjectByType<Player>();
                    _playerHealth = player != null ? player.GetComponent<Health>() : null;
                }

                return _playerHealth;
            }
        }
    }
}
