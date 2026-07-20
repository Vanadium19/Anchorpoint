using System;
using BaseModule;
using InputModule;
using WeaponModule;
using Zenject;

namespace PlayerModule
{
    public class WeaponInputController : IInitializable, ITickable, ILateTickable, IDisposable, IPausable
    {
        private readonly IWeaponInventory _weaponInventory;
        private readonly IInputMap _input;
        private readonly IPauseManager _pauseManager;

        private bool _isPaused;

        public WeaponInputController(
            IWeaponInventory weaponInventory,
            IInputMap input,
            IPauseManager pauseManager)
        {
            _weaponInventory = weaponInventory;
            _input = input;
            _pauseManager = pauseManager;
        }

        public void Initialize()
        {
            _pauseManager.Register(this);
            _weaponInventory.Initialize();

            if (_weaponInventory.WeaponsCount > 0)
                _weaponInventory.EquipWeapon(0);
        }

        public void Dispose() => _pauseManager.Unregister(this);

        public void SetPaused(bool isPaused) => _isPaused = isPaused;

        public void Tick()
        {
            if (_isPaused)
                return;

            HandleInput();

            if (_weaponInventory.CurrentWeapon is WeaponController controller)
                controller.Tick();
        }

        public void LateTick()
        {
            if (_isPaused)
                return;

            if (_weaponInventory.CurrentWeapon is WeaponController controller)
                controller.LateTick();
        }

        private void HandleInput()
        {
            int requestedIndex = _input.SelectWeaponIndex;

            if (requestedIndex != -1)
            {
                _weaponInventory.EquipWeapon(requestedIndex);
                return;
            }

            float scroll = _input.WeaponScroll;

            if (scroll > 0.1f)
                EquipNext();
            else if (scroll < -0.1f)
                EquipPrevious();
        }

        private void EquipNext()
        {
            _weaponInventory.EquipWeapon(0);
        }

        private void EquipPrevious()
        {
            _weaponInventory.EquipWeapon(_weaponInventory.WeaponsCount - 1);
        }
    }
}