using System;
using UnityEngine;
using Zenject;
using InputModule;

namespace InventoryModule
{
    public sealed class InventoryStartup : IInitializable, IInventoryReadyHandler
    {
        private readonly IInventoryManager _inventoryManager;
        private readonly IInputMap _inputMap;
        private readonly IInputService _inputService;
        private readonly CharacterInventory _characterInventory;
        private readonly GameObject _inventoryUI;
        private readonly IEquipmentSlotService _slotService;

        public event Action InventoryReady;

        public InventoryStartup(
            IInventoryManager inventoryManager,
            IInputMap inputMap,
            IInputService inputService,
            CharacterInventory characterInventory,
            [Inject(Id = "InventoryUI")] GameObject inventoryUI,
            IEquipmentSlotService slotService)
        {
            _inventoryManager = inventoryManager;
            _inputMap = inputMap;
            _inputService = inputService;
            _characterInventory = characterInventory;
            _inventoryUI = inventoryUI;
            _slotService = slotService;
        }

        public void Initialize()
        {
            _inventoryManager.SetInput(_inputMap, _inputService);

            if (_inventoryUI != null)
                _inventoryManager.SetInventoryUI(_inventoryUI);

            if (_characterInventory != null)
                _characterInventory.Initialize(_inventoryManager);

            if (_inventoryUI != null && _slotService != null)
            {
                var slots = _inventoryUI.GetComponentsInChildren<EquipmentSlot>(true);
                foreach (var slot in slots)
                {
                    slot.ForceAwake();
                }
            }

            InventoryReady?.Invoke();
        }
    }
}
