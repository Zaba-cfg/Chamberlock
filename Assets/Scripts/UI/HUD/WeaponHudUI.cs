using Components.Weapons;
using Components.Weapons.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
    public class WeaponHudUI : MonoBehaviour
    {
        [SerializeField] private WeaponHolder _weaponHolder;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _ammoText;
        [SerializeField] private GameObject _reloadIndicator;

        private Weapon _currentWeapon;

        private void Awake()
        {
            if (_weaponHolder == null)
                throw new MissingReferenceException($"{name}: no WeaponHolder assigned.");

            if (_icon == null)
                throw new MissingReferenceException($"{name}: no icon Image assigned.");

            if (_nameText == null)
                throw new MissingReferenceException($"{name}: no name text assigned.");

            if (_ammoText == null)
                throw new MissingReferenceException($"{name}: no ammo text assigned.");

            if (_reloadIndicator == null)
                throw new MissingReferenceException($"{name}: no reload indicator assigned.");
        }

        private void OnEnable()
        {
            _weaponHolder.WeaponChanged += HandleWeaponChanged;
            HandleWeaponChanged(_weaponHolder.CurrentWeapon);
        }

        private void OnDisable()
        {
            _weaponHolder.WeaponChanged -= HandleWeaponChanged;
            UnsubscribeFromWeapon();
        }

        private void HandleWeaponChanged(Weapon weapon)
        {
            UnsubscribeFromWeapon();

            _currentWeapon = weapon;

            if (weapon == null)
            {
                ClearDisplay();
                return;
            }

            weapon.AmmoChanged += HandleAmmoChanged;
            weapon.ReloadStarted += HandleReloadStarted;
            weapon.ReloadCompleted += HandleReloadCompleted;

            RefreshWeapon();
        }

        private void UnsubscribeFromWeapon()
        {
            if (_currentWeapon == null)
                return;

            _currentWeapon.AmmoChanged -= HandleAmmoChanged;
            _currentWeapon.ReloadStarted -= HandleReloadStarted;
            _currentWeapon.ReloadCompleted -= HandleReloadCompleted;
        }

        private void RefreshWeapon()
        {
            WeaponData data = _currentWeapon.WeaponData;

            _icon.sprite = data.Icon;
            _icon.enabled = data.Icon != null;
            _nameText.text = data.WeaponName;

            RefreshAmmo();
            SetReloadIndicator(_currentWeapon.IsReloading);
        }

        private void HandleAmmoChanged(int previousAmmo, int currentAmmo)
        {
            RefreshAmmo();
        }

        private void RefreshAmmo()
        {
            if (_currentWeapon == null)
                return;

            _ammoText.text = _currentWeapon.WeaponData.AmmoType == WeaponAmmoType.Magazine
                ? $"{_currentWeapon.CurrentAmmo}/{_currentWeapon.ReserveAmmo}"
                : "∞";
        }

        private void HandleReloadStarted()
        {
            SetReloadIndicator(true);
        }

        private void HandleReloadCompleted()
        {
            SetReloadIndicator(false);
        }

        private void SetReloadIndicator(bool active)
        {
            _reloadIndicator.SetActive(active);
        }

        private void ClearDisplay()
        {
            _icon.sprite = null;
            _icon.enabled = false;
            _nameText.text = "--";
            _ammoText.text = "--/--";
            SetReloadIndicator(false);
        }
    }
}
