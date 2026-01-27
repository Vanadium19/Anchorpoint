using System;

namespace WeaponModule
{
    public class WeaponModel
    {
        private int _currentAmmo;
        private int _maxAmmo;

        public event Action<int, int> AmmoChanged;

        public int CurrentAmmo => _currentAmmo;
        public bool IsFull => _currentAmmo == _maxAmmo;
        public bool IsEmpty => _currentAmmo <= 0;

        public void Initialize(int maxAmmo)
        {
            _maxAmmo = maxAmmo;
            _currentAmmo = maxAmmo;
            AmmoChanged?.Invoke(_currentAmmo, _maxAmmo);
        }

        public bool TryConsumeAmmo()
        {
            if (_currentAmmo <= 0)
                return false;

            _currentAmmo--;
            AmmoChanged?.Invoke(_currentAmmo, _maxAmmo);
            return true;
        }

        public void Reload()
        {
            _currentAmmo = _maxAmmo;
            AmmoChanged?.Invoke(_currentAmmo, _maxAmmo);
        }
    }
}