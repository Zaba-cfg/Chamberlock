using System;
using Entities.PlayerRelated;
using UnityEngine;

namespace Components.Rooms
{
    [RequireComponent(typeof(SpriteRenderer))]
    [ExecuteAlways]
    public class RoomDoor : MonoBehaviour
    {
        [SerializeField] private DoorDirection _direction;
        [SerializeField] private Collider2D _blocker;
        [SerializeField] private Sprite _doorClosedSprite;
        [SerializeField] private Sprite _doorLockedSprite;
        [SerializeField] private Sprite _doorUnlockedSprite;
        [SerializeField] private bool _manualLockDoor;
        
        public event Action<RoomDoor> PlayerEntered;

        public DoorDirection Direction => _direction;
        public bool IsLocked { get; private set; }

        private SpriteRenderer _spriteRenderer;
        
        private void Awake()
        {
            if (_blocker == null)
                throw new MissingReferenceException($"{name} is missing a blocker collider.");
            
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (!IsLocked && !_manualLockDoor)
                _spriteRenderer.sprite = _doorUnlockedSprite;
        }

        private void OnValidate()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = _manualLockDoor ? _doorLockedSprite : _doorClosedSprite;
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
            if (_manualLockDoor)
                return;
            
            if (!IsLocked)
                return;

            IsLocked = false;
            _blocker.enabled = false;
            _spriteRenderer.sprite = _doorUnlockedSprite;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsLocked || _manualLockDoor)
                return;

            if (!other.TryGetComponent(out Player _))
                return;

            PlayerEntered?.Invoke(this);
        }
    }
}