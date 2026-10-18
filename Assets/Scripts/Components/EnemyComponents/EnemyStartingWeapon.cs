using Components.Weapons;
using UnityEngine;

namespace Components.EnemyComponents
{
    [RequireComponent(typeof(WeaponHolder))]

    public class EnemyStartingWeapon : MonoBehaviour
    {
        [SerializeField] private Weapon _weaponPrefab;

        private WeaponHolder _weaponHolder;

        private void Awake()
        {
            _weaponHolder = GetComponent<WeaponHolder>();
        }

        private void Start()
        {
            if (_weaponPrefab == null)
                throw new MissingReferenceException($"{name} is missing a starting weapon prefab.");

            if (_weaponHolder.HasWeapon)
                return;

            Weapon weapon = Instantiate(_weaponPrefab, transform.position, Quaternion.identity);

            _weaponHolder.Equip(weapon);
        }
    }
}
