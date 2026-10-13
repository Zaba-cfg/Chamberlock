using System;
using Entities.PlayerRelated;
using UnityEngine;

namespace Components.Rooms
{
    public class RoomDoor : MonoBehaviour
    {
        [SerializeField] private DoorDirection _direction;
        [SerializeField] private Collider2D _blocker;

        public event Action<RoomDoor> PlayerEntered;

        public DoorDirection Direction => _direction;
        public bool IsLocked { get; private set; }

        private void Awake()
        {
            if (_blocker == null)
                throw new MissingReferenceException($"{name} is missing a blocker collider.");
        }

        public void Lock()
        {
            if (IsLocked)
                return;

            IsLocked = true;
            _blocker.enabled = true;
        }

        public void Unlock()
        {
            if (!IsLocked)
                return;

            IsLocked = false;
            _blocker.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsLocked)
                return;

            if (!other.TryGetComponent(out Player _))
                return;

            PlayerEntered?.Invoke(this);
        }
    }
}