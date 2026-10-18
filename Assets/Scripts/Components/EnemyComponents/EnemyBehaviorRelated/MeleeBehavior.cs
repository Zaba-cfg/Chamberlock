using Components.Weapons;

namespace Components.EnemyComponents.EnemyBehaviorRelated
{
    public class MeleeBehavior : EnemyBehavior
    {
        protected override bool CanAcceptWeapon(Weapon weapon)
        {
            return weapon.WeaponData.WeaponFireStrategy is MeleeFireStrategy;
        }
    }
}
