using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using InputModule;
using WeaponModule.Core;
using WeaponModule.Configs;
using WeaponModule.View;

namespace WeaponModule.Controllers
{
    public class WeaponInventory : IInitializable, ITickable, ILateTickable
    {
        private readonly WeaponController.Factory _factory;
        private readonly IInputMap _input;
        private readonly List<WeaponSetupData> _loadout;
        private readonly List<IWeapon> _weapons = new();
        private IWeapon _currentWeapon;
        private int _currentIndex = -1;
        private bool _isSwitching = false;

        public WeaponInventory(WeaponController.Factory factory,
            IInputMap input,
            List<WeaponSetupData> loadout)
        {
            _factory = factory;
            _input = input;
            _loadout = loadout;
        }

        public void Initialize()
        {
            foreach (var setup in _loadout)
            {
                var weapon = _factory.Create(setup.Config, setup.View);
                weapon.Initialize();
                setup.View.gameObject.SetActive(false);
                _weapons.Add(weapon);
            }

            if (_weapons.Count > 0)
            {
                EquipWeapon(0).Forget();
            }
        }

        public void Tick()
        {
            if (_isSwitching) return;
            _currentWeapon?.Tick();
            HandleInput();
        }

        public void LateTick()
        {
            _currentWeapon?.LateTick();
        }

        private void HandleInput()
        {
            int requestedIndex = _input.SelectWeaponIndex;

            if (requestedIndex != -1)
            {
                EquipWeapon(requestedIndex).Forget();
                return;
            }

            float scroll = _input.WeaponScroll;

            if (scroll > 0.1f)
            {
                EquipNext();
            }
            else if (scroll < -0.1f)
            {
                EquipPrevious();
            }
        }

        private void EquipNext()
        {
            if (_currentIndex == -1)
            {
                EquipWeapon(0).Forget();
                return;
            }

            int nextIndex = _currentIndex + 1;
            if (nextIndex >= _weapons.Count) nextIndex = 0;

            EquipWeapon(nextIndex).Forget();
        }

        private void EquipPrevious()
        {
            if (_currentIndex == -1)
            {
                EquipWeapon(_weapons.Count - 1).Forget();
                return;
            }

            int prevIndex = _currentIndex - 1;
            if (prevIndex < 0) prevIndex = _weapons.Count - 1;

            EquipWeapon(prevIndex).Forget();
        }

        private async UniTaskVoid EquipWeapon(int index)
        {
            if (index < 0 || index >= _weapons.Count) return;
            _isSwitching = true;

            if (_weapons[index] == _currentWeapon)
            {
                if (_currentWeapon != null)
                {
                    await _currentWeapon.Unequip();
                }

                _currentWeapon = null;
                _currentIndex = -1;

                _isSwitching = false;
                return;
            }

            if (_currentWeapon != null)
            {
                await _currentWeapon.Unequip();
            }

            _currentIndex = index;
            _currentWeapon = _weapons[index];
            _currentWeapon.Equip();
            _isSwitching = false;
        }
    }

    [System.Serializable]
    public class WeaponSetupData
    {
        public string Name;
        public WeaponConfig Config;
        public WeaponView View;
    }
}