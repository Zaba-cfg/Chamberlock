using System;
using Components.Base.Weapon;
using Components.Weapons;
using UnityEngine;

namespace Components.EnemyComponents
{
    [RequireComponent(typeof(WeaponDetection))]
    [RequireComponent(typeof(WeaponHolder))]

    public class EnemyWeaponDecision : MonoBehaviour
    {
        [SerializeField] private float _pickupRange = 0.75f;
    
        private WeaponDetection _weaponDetection;
        private WeaponHolder _weaponHolder;

        private void Awake()
        {
            _weaponDetection = GetComponent<WeaponDetection>();
            _weaponHolder = GetComponent<WeaponHolder>();
        }

        public bool TryEquipNearbyWeapon(Func<Weapon, bool> filter = null)
        {
            if (_weaponHolder.HasWeapon)
                return false;

            Weapon weapon = GetClosestAvailableWeapon(filter);

            if (weapon == null)
                return false;

            _weaponHolder.Equip(weapon);
            return true;
        }

        public Weapon GetClosestAvailableWeapon(Func<Weapon, bool> filter = null)
        {
            Weapon closestWeapon = null;
            float closestDistance = float.MaxValue;

            foreach (Weapon weapon in _weaponDetection.Weapons)
            {
                if (weapon.IsEquipped)
                    continue;

                if (weapon.IsEmpty)
                    continue;

                if (filter != null && !filter(weapon))
                    continue;

                float distance = (weapon.transform.position - transform.position).sqrMagnitude;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestWeapon = weapon;
                }
            }

            return closestWeapon;
        }

        public bool IsWeaponInPickupRange(Weapon weapon)
        {
            if (weapon == null)
                return false;

            float distanceSquared =
                (weapon.transform.position - transform.position).sqrMagnitude;

            return distanceSquared <= _pickupRange * _pickupRange;
        }
    }
}
