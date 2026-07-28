using System;
using System.Linq;
using BaseModule;
using InputModule;
using InventoryModule;
using WeaponModule;
using Zenject;

namespace PlayerModule
{
    public class WeaponInputController : IInitializable, ITickable, ILateTickable, IDisposable, IPausable
    {
        private readonly IWeaponInventory _weaponInventory;
        private readonly IInputMap _input;
        private readonly IEquipmentSlotService _slotService;
        private readonly IPauseManager _pauseManager;

        private bool _isPaused;

        public WeaponInputController(
            IWeaponInventory weaponInventory,
            IInputMap input,
            IEquipmentSlotService slotService,
            IPauseManager pauseManager)
        {
            _weaponInventory = weaponInventory;
            _input = input;
            _slotService = slotService;
            _pauseManager = pauseManager;
        }

        public void Initialize()
        {
            _pauseManager.Register(this);
            _weaponInventory.Initialize();
        }

        public void Dispose() => _pauseManager.Unregister(this);

        public void SetPaused(bool isPaused) => _isPaused = isPaused;

        public void Tick()
        {
            if (_isPaused)
                return;

            HandleInput();

            if (_weaponInventory.CurrentWeapon is WeaponController controller)
            {
                controller.SetMovementInput(_input.MoveInput);
                controller.SetAimInput(_input.IsAimPressed, _input.IsAimTriggered);
                controller.SetFireInput(_input.IsFireHeld);
                controller.SetReloadInput(_input.IsReloadPressed);

                controller.Tick();
            }
        }

        public void LateTick()
        {
            if (_isPaused)
                return;

            if (_weaponInventory.CurrentWeapon is WeaponController controller)
            {
                controller.SetLookInput(_input.LookInput);
                controller.LateTick();
            }
        }

        private static readonly EquipmentSlotType[] WeaponKeyMap = {
            EquipmentSlotType.PrimaryWeapon1,
            EquipmentSlotType.PrimaryWeapon2,
            EquipmentSlotType.WeaponSecondary,
        };

        private void HandleInput()
        {
            int requestedIndex = _input.SelectWeaponIndex;

            if (requestedIndex >= 0 && requestedIndex < WeaponKeyMap.Length)
            {
                var slotType = WeaponKeyMap[requestedIndex];
                var slot = _slotService.GetSlot(slotType);

                if (slot != null && slot.IsEquipped)
                {
                    _weaponInventory.EquipWeapon(slot.EquippedItem);
                }
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
            var weaponSlots = _slotService.GetAllSlots()
                .Where(s => s.IsEquipped && s.EquippedItem.ItemDataSo is WeaponItemSo)
                .OrderBy(s => s.SlotType)
                .ToList();

            if (weaponSlots.Count == 0) 
                return;

            ItemTable currentItem = null;

            if (_weaponInventory.CurrentWeapon is WeaponController controller)
                currentItem = controller.GetItemTable();

            if (currentItem == null)
            {
                _weaponInventory.EquipWeapon(weaponSlots[0].EquippedItem);
                return;
            }

            var currentSlot = weaponSlots.FirstOrDefault(s => s.EquippedItem == currentItem);
            int currentIndex = weaponSlots.IndexOf(currentSlot);
            int nextIndex = (currentIndex + 1) % weaponSlots.Count;
            _weaponInventory.EquipWeapon(weaponSlots[nextIndex].EquippedItem);
        }

        private void EquipPrevious()
        {
            var weaponSlots = _slotService.GetAllSlots()
                .Where(s => s.IsEquipped && s.EquippedItem.ItemDataSo is WeaponItemSo)
                .OrderBy(s => s.SlotType)
                .ToList();

            if (weaponSlots.Count == 0) 
                return;

            ItemTable currentItem = null;

            if (_weaponInventory.CurrentWeapon is WeaponController controller)
                currentItem = controller.GetItemTable();

            if (currentItem == null)
            {
                _weaponInventory.EquipWeapon(weaponSlots[0].EquippedItem);
                return;
            }

            var currentSlot = weaponSlots.FirstOrDefault(s => s.EquippedItem == currentItem);
            int currentIndex = weaponSlots.IndexOf(currentSlot);
            int prevIndex = (currentIndex - 1 + weaponSlots.Count) % weaponSlots.Count;
            _weaponInventory.EquipWeapon(weaponSlots[prevIndex].EquippedItem);
        }
    }
}
