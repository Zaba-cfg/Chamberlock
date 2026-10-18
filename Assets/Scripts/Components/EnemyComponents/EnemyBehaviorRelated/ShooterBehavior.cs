using Components.Projectile;
using Components.Weapons;
using UnityEngine;

namespace Components.EnemyComponents.EnemyBehaviorRelated
{
    public class ShooterBehavior : EnemyBehavior
    {
        protected override bool CanAcceptWeapon(Weapon weapon)
        {
            return weapon.WeaponData.WeaponFireStrategy is ProjectileFireStrategy;
        }

        protected override void HandleChasing()
        {
            _aiInput.SetRetreating(false);
            base.HandleChasing();
        }

        protected override void HandleSeekingWeapon()
        {
            if (_weaponDecision.GetClosestAvailableWeapon(CanAcceptWeapon) == null)
            {
                _aiInput.SetTarget(Target);
                _lookDirection.SetDirection(Target.position - transform.position);
                _aiInput.SetRetreating(true);
                return;
            }

            _aiInput.SetRetreating(false);
            base.HandleSeekingWeapon();
        }

        protected override void HandleAttacking()
        {
            _aiInput.SetRetreating(true);
            base.HandleAttacking();
        }
    }
}
