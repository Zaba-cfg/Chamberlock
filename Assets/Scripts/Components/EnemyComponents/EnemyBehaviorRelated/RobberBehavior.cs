using Components.Weapons;
using Entities.PlayerRelated;
using UnityEngine;

namespace Components.EnemyComponents.EnemyBehaviorRelated
{
    public class RobberBehavior : EnemyBehavior
    {
        [SerializeField, Min(0f)] private float _fleeDuration = 2.5f;

        private WeaponHolder _playerWeaponHolder;
        private float _fleeEndTime;

        protected override EnemyState InitialState => EnemyState.Hiding;

        protected override void HandleState(EnemyState state)
        {
            switch (state)
            {
                case EnemyState.Hiding:
                    HandleHiding();
                    break;

                case EnemyState.Stealing:
                    HandleStealing();
                    break;

                case EnemyState.Fleeing:
                    HandleFleeing();
                    break;

                default:
                    base.HandleState(state);
                    break;
            }
        }

        protected override void HandleAttacking()
        {
            Weapon weapon = _weaponHolder.HasWeapon ? _weaponHolder.CurrentWeapon : null;

            if (weapon == null || weapon.IsEmpty)
            {
                if (weapon != null)
                    _weaponHolder.DropCurrentWeapon();

                CurrentState = PlayerHasWeapon() ? EnemyState.Stealing : EnemyState.Hiding;
                return;
            }

            _aiInput.SetTarget(Target);
            _lookDirection.SetDirection(Target.position - transform.position);

            if (!_combat.CanAttack())
            {
                _aiInput.SetMoveEnabled(true);
                _aiInput.SetRetreating(false);
                return;
            }

            _aiInput.SetMoveEnabled(false);

            if (weapon.CurrentAmmo <= 0)
            {
                weapon.Reload();
                return;
            }

            _combat.Attack(_lookDirection.Forward);
        }

        private void HandleHiding()
        {
            if (PlayerHasWeapon())
            {
                CurrentState = EnemyState.Stealing;
                return;
            }

            _aiInput.SetTarget(Target);
            _lookDirection.SetDirection(Target.position - transform.position);

            bool playerClose = _combat.CanAttack();

            _aiInput.SetMoveEnabled(playerClose);
            _aiInput.SetRetreating(true);
        }

        private void HandleStealing()
        {
            Weapon playerWeapon = PlayerWeaponHolder != null
                ? PlayerWeaponHolder.CurrentWeapon
                : null;

            if (playerWeapon == null)
            {
                CurrentState = EnemyState.Hiding;
                return;
            }

            _aiInput.SetTarget(playerWeapon.transform);
            _aiInput.SetMoveEnabled(true);
            _aiInput.SetRetreating(false);
            _lookDirection.SetDirection(playerWeapon.transform.position - transform.position);

            if (!_weaponDecision.IsWeaponInPickupRange(playerWeapon))
                return;

            _weaponHolder.Equip(playerWeapon);

            _fleeEndTime = Time.time + _fleeDuration;
            CurrentState = EnemyState.Fleeing;
        }

        private void HandleFleeing()
        {
            _aiInput.SetTarget(Target);
            _aiInput.SetMoveEnabled(true);
            _aiInput.SetRetreating(true);
            _lookDirection.SetDirection(Target.position - transform.position);

            if (Time.time < _fleeEndTime)
                return;

            CurrentState = EnemyState.Attacking;
        }

        private bool PlayerHasWeapon()
        {
            return PlayerWeaponHolder != null && PlayerWeaponHolder.HasWeapon;
        }

        private WeaponHolder PlayerWeaponHolder
        {
            get
            {
                if (_playerWeaponHolder == null)
                {
                    Player player = FindFirstObjectByType<Player>();
                    _playerWeaponHolder = player != null ? player.GetComponent<WeaponHolder>() : null;
                }

                return _playerWeaponHolder;
            }
        }
    }
}
