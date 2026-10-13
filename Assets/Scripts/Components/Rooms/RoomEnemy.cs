using Components.Base.HealthRelated;
using UnityEngine;

namespace Components.Rooms
{
    [RequireComponent(typeof(Health))]

    public class RoomEnemy : MonoBehaviour
    {
        private Health _health;
        private RoomController _room;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            _room = GetComponentInParent<RoomController>();

            if (_room == null)
                return;

            _room.RegisterEnemy(this);
            _health.Died += HandleDied;
        }

        private void OnDisable()
        {
            if (_room == null)
                return;

            _health.Died -= HandleDied;
            _room.UnregisterEnemy(this);
            _room = null;
        }

        private void HandleDied()
        {
            if (_room != null)
                _room.NotifyEnemyDied(this);
        }
    }
}