using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace WeaponModule
{
    public class WeaponInventory : IWeaponInventory
    {
        private readonly WeaponFactory _weaponFactory;
        private readonly List<WeaponSetupData> _loadout;

        private readonly List<IWeapon> _weapons = new();

        private IWeapon _currentWeapon;
        private int _currentIndex = -1;
        private int _lastEquippedIndex;

        public event Action<IWeapon> CurrentWeaponChanged;

        public WeaponInventory(WeaponFactory weaponFactory, List<WeaponSetupData> loadout)
        {
            _weaponFactory = weaponFactory;
            _loadout = loadout;
        }

        public IWeapon CurrentWeapon => _currentWeapon;

        public int WeaponsCount => _weapons.Count;

        public void Initialize()
        {
            foreach (var setup in _loadout)
            {
                var weapon = _weaponFactory.Create(setup.Config, setup.View);
                weapon.Initialize();
                setup.View.gameObject.SetActive(false);
                _weapons.Add(weapon);
            }
        }

        public void EquipWeapon(int index)
        {
            if (index < 0 || index >= _weapons.Count)
                return;

            if (_weapons[index] == _currentWeapon)
            {
                UnequipCurrentWeapon();
                return;
            }

            if (_currentWeapon != null)
                HideWeapon(_currentWeapon);

            _currentIndex = index;
            _currentWeapon = _weapons[index];
            _currentWeapon.Equip();
            CurrentWeaponChanged?.Invoke(_currentWeapon);
        }

        public void UnequipCurrentWeapon()
        {
            if (_currentWeapon == null)
                return;

            _lastEquippedIndex = _currentIndex;
            _currentWeapon.Unequip().Forget();
            _currentWeapon = null;
            _currentIndex = -1;
            CurrentWeaponChanged?.Invoke(_currentWeapon);
        }

        public void EquipLastWeapon()
        {
            if (_lastEquippedIndex >= 0 && _lastEquippedIndex < _weapons.Count)
                EquipWeapon(_lastEquippedIndex);
        }

        private void HideWeapon(IWeapon weapon)
        {
            if (weapon is WeaponController controller)
                controller.Hide();
        }
    }
}