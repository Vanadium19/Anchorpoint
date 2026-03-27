using InputModule;
using WeaponModule;
using Zenject;

namespace PlayerModule
{
    public class WeaponInputController : IInitializable, ITickable, ILateTickable
    {
        private readonly IWeaponInventory _weaponInventory;
        private readonly IInputMap _input;

        public WeaponInputController(
            IWeaponInventory weaponInventory,
            IInputMap input)
        {
            _weaponInventory = weaponInventory;
            _input = input;
        }

        public void Initialize()
        {
            _weaponInventory.Initialize();

            if (_weaponInventory.WeaponsCount > 0)
                _weaponInventory.EquipWeapon(0);
        }

        public void Tick()
        {
            HandleInput();

            if (_weaponInventory.CurrentWeapon is WeaponController controller)
                controller.Tick();
        }

        public void LateTick()
        {
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