using Components.Weapons;
using Components.Weapons.Enums;
using TMPro;
using UnityEngine;

namespace UI.WeaponAmmoUI
{
    public class WeaponAmmoUI : MonoBehaviour
    {
        [SerializeField] private WeaponHolder _weaponHolder;
        [SerializeField] private TextMeshProUGUI _ammoText;

        private Weapon _currentWeapon;

        private void UpdateAmmoText(int currentAmmo, int reserveAmmo)
        {
            _ammoText.text = _weaponHolder.CurrentWeapon.WeaponData.AmmoType == WeaponAmmoType.Magazine ? $"{currentAmmo}/{reserveAmmo}" : "∞";
        }
    
        private void ClearAmmoText()
        {
            _ammoText.text = "--/--";
        }

        private void OnAmmoChanged(int _, int currentAmmo)
        {
            UpdateAmmoText(currentAmmo, _currentWeapon.ReserveAmmo);
        }

        private void HandleWeaponChanged(Weapon weapon)
        {
            if (_currentWeapon != null)
                _currentWeapon.AmmoChanged -= OnAmmoChanged;
        
            _currentWeapon = weapon;

            if (weapon == null)
            {
                ClearAmmoText();
                return;
            }
            _currentWeapon.AmmoChanged += OnAmmoChanged;
        
            UpdateAmmoText(_currentWeapon.CurrentAmmo, _currentWeapon.ReserveAmmo);
        }

        private void OnEnable()
        {
            _weaponHolder.WeaponChanged += HandleWeaponChanged;
            HandleWeaponChanged(_weaponHolder.CurrentWeapon);
        }
    
        private void OnDisable()
        {
            _weaponHolder.WeaponChanged -= HandleWeaponChanged;
        
            if (_currentWeapon  != null)
                _currentWeapon.AmmoChanged -= OnAmmoChanged;
        }
    }
}
