using InputModule;
using InventoryModule;
using System;
using UIModule;
using UnityEngine;
using Zenject;

namespace PlayerModule
{
    public sealed class PlayerInteractionController : IInitializable, ITickable, IDisposable
    {
        private readonly IInputMap _input;
        private readonly IInventoryManager _inventoryManager;
        private readonly Camera _camera;
        private readonly PlayerConfig _config;

        public event Action<LootItemView> HoverChanged;
        private LootItemView _currentHoveredLoot;

        public PlayerInteractionController(
            IInputMap input,
            IInventoryManager inventoryManager,
            PlayerConfig config)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
            _config = config;
            _camera = Camera.main;
        }

        public void Initialize() { }
        public void Dispose() { }

        public void Tick()
        {
            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out var hit, _config.InteractionDistance, _config.InteractionLayer))
            {
                var loot = hit.collider.GetComponentInParent<LootItemView>();
                UpdateHover(loot);

                if (loot != null && _input.IsInteractPressed)
                    TryPickUpLoot(loot);
            }
            else
                UpdateHover(null);
        }

        private void UpdateHover(LootItemView newLoot)
        {
            if (_currentHoveredLoot == null && newLoot == null) 
                return;

            if (_currentHoveredLoot != null && newLoot == null)
            {
                _currentHoveredLoot = null;
                HoverChanged?.Invoke(null);
                return;
            }

            if (_currentHoveredLoot == newLoot) 
                return;

            _currentHoveredLoot = newLoot;
            HoverChanged?.Invoke(_currentHoveredLoot);
        }

        private void TryPickUpLoot(LootItemView loot)
        {
            var itemData = loot.ItemData;

            if (itemData == null)
            {
                Debug.LogWarning("[PlayerInteractionController] Loot has no ItemData");
                return;
            }

            bool success;

            if (itemData.IsEquippable)
            {
                if (loot.ItemTable != null)
                    success = _inventoryManager.TryAutoEquipItem(loot.ItemTable);
                else
                {
                    var newItem = new ItemTable(itemData) { StackCount = loot.Amount };
                    success = _inventoryManager.TryAutoEquipItem(newItem);
                }

                if (!success)
                {
                    if (loot.ItemTable != null)
                        success = _inventoryManager.AddExistingItemToInventory(loot.ItemTable);
                    else
                        success = _inventoryManager.AddItemToInventory(itemData, loot.Amount);
                }
            }
            else
            {
                if (loot.ItemTable != null)
                    success = _inventoryManager.AddExistingItemToInventory(loot.ItemTable);
                else
                    success = _inventoryManager.AddItemToInventory(itemData, loot.Amount);
            }

            if (success)
            {
                loot.PlayCollectEffects();
                UpdateHover(null);
                UnityEngine.Object.Destroy(loot.gameObject);
            }
        }
    }
}
