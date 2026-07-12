using System;
using UnityEngine;

namespace WeaponModule
{
    public class WeaponModel
    {
        private int _currentMagazineAmmo;
        private int _reserveAmmo;
        private int _magazineCapacity;

        public event Action<int, int> AmmoChanged;

        public int CurrentMagazineAmmo => _currentMagazineAmmo;
        public int ReserveAmmo => _reserveAmmo;
        public int MagazineCapacity => _magazineCapacity;

        public bool IsFull => _currentMagazineAmmo >= _magazineCapacity;
        public bool IsMagazineEmpty => _currentMagazineAmmo <= 0;
        public bool CanReload => !IsFull && _reserveAmmo > 0;

        public void Initialize(int magazineCapacity, int currentMagazineAmmo, int reserveAmmo)
        {
            _magazineCapacity = Mathf.Max(1, magazineCapacity);
            _currentMagazineAmmo = Mathf.Clamp(currentMagazineAmmo, 0, _magazineCapacity);
            _reserveAmmo = Mathf.Max(0, reserveAmmo);

            NotifyAmmoChanged();
        }

        public bool TryConsumeAmmo()
        {
            if (_currentMagazineAmmo <= 0)
                return false;

            _currentMagazineAmmo--;
            NotifyAmmoChanged();
            return true;
        }

        public int Reload()
        {
            if (!CanReload)
                return 0;

            int ammoNeeded = _magazineCapacity - _currentMagazineAmmo;
            int ammoToLoad = Mathf.Min(ammoNeeded, _reserveAmmo);

            _currentMagazineAmmo += ammoToLoad;
            _reserveAmmo -= ammoToLoad;

            NotifyAmmoChanged();
            return ammoToLoad;
        }

        public int AddReserveAmmo(int amount)
        {
            if (amount <= 0)
                return 0;

            int added = amount;
            _reserveAmmo += added;
            NotifyAmmoChanged();
            return added;
        }

        public int GetReserveSpace()
        {
            return int.MaxValue - _reserveAmmo;
        }

        public void SetAmmo(int magazineAmmo, int reserveAmmo)
        {
            _currentMagazineAmmo = Mathf.Clamp(magazineAmmo, 0, _magazineCapacity);
            _reserveAmmo = Mathf.Max(0, reserveAmmo);
            NotifyAmmoChanged();
        }

        private void NotifyAmmoChanged() => AmmoChanged?.Invoke(_currentMagazineAmmo, _reserveAmmo);
    }
}