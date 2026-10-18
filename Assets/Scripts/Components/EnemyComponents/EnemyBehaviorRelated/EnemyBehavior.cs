using Components.Base;
using Components.Weapons;
using UnityEngine;

namespace Components.EnemyComponents.EnemyBehaviorRelated
{
    [RequireComponent(typeof(EnemyAIInput))]
    [RequireComponent(typeof(EnemyCombat))]
    [RequireComponent(typeof(EnemyWeaponDecision))]
    [RequireComponent(typeof(LookDirection))]
    [RequireComponent(typeof(WeaponHolder))]

    public class EnemyBehavior : MonoBehaviour
    {
        public EnemyState CurrentState { get; private set; }
    
        protected EnemyAIInput _aiInput;
        protected EnemyCombat _combat;
        protected EnemyWeaponDecision _weaponDecision;
        protected LookDirection _lookDirection;
        protected WeaponHolder _weaponHolder;
    
        [SerializeField] private Transform _target;
        public Transform Target => _target;

        public void SetTarget(Transform target)
        {
            if (target == null)
                throw new System.ArgumentNullException(nameof(target));

            _target = target;
        }

        private void Awake()
        {
            if (_target == null)
                throw new MissingReferenceException($"{name} is missing a target.");
        
            _aiInput = GetComponent<EnemyAIInput>();
            _combat = GetComponent<EnemyCombat>();
            _weaponDecision = GetComponent<EnemyWeaponDecision>();
            _lookDirection = GetComponent<LookDirection>();
            _weaponHolder = GetComponent<WeaponHolder>();

            CurrentState = EnemyState.Chasing;
        }
    
        private void Update()
        {
            switch (CurrentState)
            {
                case EnemyState.Chasing:
                    HandleChasing();
                    break;

                case EnemyState.SeekingWeapon:
                    HandleSeekingWeapon();
                    break;

                case EnemyState.Attacking:
                    HandleAttacking();
                    break;
            }
        }

        protected virtual void HandleChasing()
        {
            _aiInput.SetTarget(_target);
            _lookDirection.SetDirection(_target.position - transform.position);
        
            if (!_weaponHolder.HasWeapon)
            {
                CurrentState = EnemyState.SeekingWeapon;
                return;
            }

            if (_weaponHolder.CurrentWeapon.IsEmpty)
            {
                _weaponHolder.DropCurrentWeapon();
                CurrentState = EnemyState.SeekingWeapon;
                return;
            }

            if (!CanAcceptWeapon(_weaponHolder.CurrentWeapon))
            {
                _weaponHolder.DropCurrentWeapon();
                CurrentState = EnemyState.SeekingWeapon;
                return;
            }
        
            if (_combat.CanAttack())
            {
                CurrentState = EnemyState.Attacking;
                return;
            }
        }
    
        protected virtual void HandleSeekingWeapon()
        {
            Weapon weapon = _weaponDecision.GetClosestAvailableWeapon(CanAcceptWeapon);

            if (weapon == null)
            {
                CurrentState = EnemyState.Chasing;
                return;
            }

            _aiInput.SetTarget(weapon.transform);
            _lookDirection.SetDirection(weapon.transform.position - transform.position);

            if (!_weaponDecision.IsWeaponInPickupRange(weapon))
                return;

            if (_weaponDecision.TryEquipNearbyWeapon(CanAcceptWeapon))
            {
                CurrentState = EnemyState.Chasing;
            }
        }
    
        protected virtual void HandleAttacking()
        {
            if (!_weaponHolder.HasWeapon)
            {
                CurrentState = EnemyState.SeekingWeapon;
                return;
            }

            Weapon weapon = _weaponHolder.CurrentWeapon;

            if (weapon.IsEmpty)
            {
                _weaponHolder.DropCurrentWeapon();
                CurrentState = EnemyState.SeekingWeapon;
                return;
            }

            if (!CanAcceptWeapon(weapon))
            {
                _weaponHolder.DropCurrentWeapon();
                CurrentState = EnemyState.SeekingWeapon;
                return;
            }

            _lookDirection.SetDirection(_target.position - transform.position);

            if (!_combat.CanAttack())
            {
                CurrentState = EnemyState.Chasing;
                return;
            }
        
            if (weapon.CurrentAmmo <= 0)
            {
                weapon.Reload();
                return;
            }

            _combat.Attack(_lookDirection.Forward);
        }

        protected virtual bool CanAcceptWeapon(Weapon weapon)
        {
            return true;
        }
    }
}
