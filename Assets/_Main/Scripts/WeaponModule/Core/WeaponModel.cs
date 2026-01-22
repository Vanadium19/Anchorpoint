using System;

namespace WeaponModule.Core
{
    public class WeaponModel
    {
        private int _currentAmmo;
        private int _maxAmmo;

        public event Action<int, int> AmmoChanged; // Текущие, Макс

        public int CurrentAmmo => _currentAmmo;
        public bool IsFull => _currentAmmo == _maxAmmo;
        public bool IsEmpty => _currentAmmo <= 0;

        public void Initialize(int maxAmmo)
        {
            _maxAmmo = maxAmmo;
            _currentAmmo = maxAmmo;
            Notify();
        }

        public bool TryConsumeAmmo()
        {
            if (_currentAmmo <= 0) return false;

            _currentAmmo--;
            Notify();
            return true;
        }

        public void Reload()
        {
            _currentAmmo = _maxAmmo;
            Notify();
        }

        private void Notify() => AmmoChanged?.Invoke(_currentAmmo, _maxAmmo);
    }
}