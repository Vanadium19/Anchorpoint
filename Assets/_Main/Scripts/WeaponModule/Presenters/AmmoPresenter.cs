using System;
using Zenject;

namespace WeaponModule
{
    public class AmmoPresenter : IInitializable, IDisposable
    {
        private readonly WeaponInventory _weaponInventory;
        private readonly AmmoView _ammoView;

        private WeaponController _currentWeapon;

        public AmmoPresenter(WeaponInventory weaponInventory, AmmoView ammoView)
        {
            _weaponInventory = weaponInventory;
            _ammoView = ammoView;
        }

        public void Initialize()
        {
            _weaponInventory.CurrentWeaponChanged += OnCurrentWeaponChanged;
            OnCurrentWeaponChanged(_weaponInventory.CurrentWeapon);
        }

        public void Dispose()
        {
            _weaponInventory.CurrentWeaponChanged -= OnCurrentWeaponChanged;
            UnsubscribeFromCurrentWeapon();
        }

        private void OnCurrentWeaponChanged(WeaponController weapon)
        {
            UnsubscribeFromCurrentWeapon();

            _currentWeapon = weapon;

            if (_currentWeapon == null)
            {
                _ammoView.Clear();
                return;
            }

            _currentWeapon.Model.AmmoChanged += OnAmmoChanged;
            OnAmmoChanged(_currentWeapon.Model.CurrentMagazineAmmo, _currentWeapon.Model.ReserveAmmo);
        }

        private void OnAmmoChanged(int currentMagazineAmmo, int reserveAmmo)
        {
            _ammoView.SetAmmo(currentMagazineAmmo, reserveAmmo);
        }

        private void UnsubscribeFromCurrentWeapon()
        {
            if (_currentWeapon == null)
                return;

            _currentWeapon.Model.AmmoChanged -= OnAmmoChanged;
            _currentWeapon = null;
        }
    }
}