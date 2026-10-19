using Components.Base;
using Components.Base.HealthRelated;
using Components.PlayerInput;
using Components.Weapons;
using Systems.Game;
using UnityEngine;

namespace Entities.PlayerRelated
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(Movement))]
    [RequireComponent(typeof(LookDirection))]
    [RequireComponent(typeof(WeaponHolder))]
    [RequireComponent(typeof(Interaction))]

    public class Player : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private LookDirection _lookDirection;
        private WeaponHolder _weaponHolder;
        private Interaction _interaction;
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _playerInput = GetComponent<PlayerInput>();
            _lookDirection = GetComponent<LookDirection>();
            _weaponHolder = GetComponent<WeaponHolder>();
            _interaction = GetComponent<Interaction>();
        }
        
        private static bool IsInputEnabled => GameManager.Instance == null || GameManager.Instance.IsRunning;

        private void Update()
        {
            if (!IsInputEnabled)
                return;

            UpdateLookDirection();
            UpdateFire();
        }
    
        private void UpdateFire()
        {
            if (!_playerInput.FireHeld)
                return;
            
            _weaponHolder.TryFire(_lookDirection.Forward);
            
            if (_weaponHolder.CurrentWeapon == null)
                return;
            
            if (_weaponHolder.CurrentWeapon.CurrentAmmo == 0 )
                _weaponHolder.Reload();
        }

        private void UpdateLookDirection()
        {
            if (_playerInput.CurrentAimDevice == AimDevice.Gamepad)
            {
                if (_playerInput.LookStick != Vector2.zero)
                {
                    _lookDirection.SetDirection(_playerInput.LookStick);
                }

                return;
            }

            Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(_playerInput.LookPointer);

            Vector2 direction = (Vector2)mouseWorldPosition - (Vector2)transform.position;

            _lookDirection.SetDirection(direction);
        }
    
        private void HandleFire()
        {
            if (!IsInputEnabled)
                return;

            _weaponHolder.TryFire(_lookDirection.Forward);
        }
    
        private void HandleReload()
        {
            if (!IsInputEnabled)
                return;

            _weaponHolder.Reload();
        }
    
        private void HandleInteract()
        {
            if (!IsInputEnabled)
                return;

            _interaction.Interact();
        }
    
        private void OnEnable()
        {
            _playerInput.ReloadPressed += HandleReload;
            _playerInput.FirePressed += HandleFire;
            _playerInput.InteractPressed += HandleInteract;
        }

        private void OnDisable()
        {
            _playerInput.ReloadPressed -= HandleReload;
            _playerInput.FirePressed -= HandleFire;
            _playerInput.InteractPressed -= HandleInteract;
        }
    }
}
