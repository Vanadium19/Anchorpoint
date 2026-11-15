using System;
using Zenject;

namespace WeaponModule
{
    public class AmmoPresenter : IInitializable, IDisposable
    {
        private readonly WeaponInventory _weaponInventory;
        private readonly AmmoView _ammoView;

        private IWeapon _currentWeapon;

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

        private void OnCurrentWeaponChanged(IWeapon weapon)
        {
            UnsubscribeFromCurrentWeapon();

            _currentWeapon = weapon;

            if (_currentWeapon == null)
            {
                _ammoView.Clear();
                return;
            }

            _currentWeapon.Model.AmmoChanged += OnAmmoChanged;
            _currentWeapon.Wear.WearChanged += OnWearChanged;
            _currentWeapon.Wear.MalfunctionChanged += OnMalfunctionChanged;

            OnAmmoChanged(_currentWeapon.Model.CurrentMagazineAmmo, _currentWeapon.Model.ReserveAmmo);
            OnWearChanged(_currentWeapon.Wear.Wear);
            OnMalfunctionChanged(_currentWeapon.Wear.Malfunction);
        }

        private void OnAmmoChanged(int currentMagazineAmmo, int reserveAmmo)
        {
            _ammoView.SetAmmo(currentMagazineAmmo, reserveAmmo);
        }

        private void OnWearChanged(float wear) => _ammoView.SetWear(wear);

        private void OnMalfunctionChanged(WeaponMalfunction malfunction) => _ammoView.SetMalfunction(malfunction);

        private void UnsubscribeFromCurrentWeapon()
        {
            if (_currentWeapon == null)
                return;

            _currentWeapon.Model.AmmoChanged -= OnAmmoChanged;
            _currentWeapon.Wear.WearChanged -= OnWearChanged;
            _currentWeapon.Wear.MalfunctionChanged -= OnMalfunctionChanged;
            _currentWeapon = null;
        }
    }
}
