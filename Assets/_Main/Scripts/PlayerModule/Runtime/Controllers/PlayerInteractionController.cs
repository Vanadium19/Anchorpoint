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
        private readonly IBuildingContainerService _buildingContainerService;
        private readonly Camera _camera;
        private readonly PlayerConfig _config;
        private readonly IDeathLootStorage _deathLootStorage;

        public event Action<LootItemView> LootHoverChanged;
        public event Action<IContainerUI> ContainerHoverChanged;

        private LootItemView _currentHoveredLoot;
        private IContainerUI _currentHoveredContainer;
        private Collider _lastHitCollider;

        public PlayerInteractionController(
            IInputMap input,
            IInventoryManager inventoryManager,
            IBuildingContainerService buildingContainerService,
            Camera camera,
            PlayerConfig config,
            IDeathLootStorage deathLootStorage)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
            _buildingContainerService = buildingContainerService;
            _camera = camera;
            _config = config;
            _deathLootStorage = deathLootStorage;
        }

        public void Initialize() { }
        public void Dispose() { }

        public void Tick()
        {
            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out var hit, _config.InteractionDistance, _config.InteractionLayer))
            {
                if (hit.collider != _lastHitCollider)
                {
                    _lastHitCollider = hit.collider;
                    FindAndCacheTargets(hit.collider);
                }

                if (_input.IsInteractPressed)
                    HandleInteraction();
            }
            else
            {
                if (_lastHitCollider != null)
                {
                    _lastHitCollider = null;
                    UpdateHover(null, null);
                }
            }
        }

        private void FindAndCacheTargets(Collider collider)
        {
            var current = collider.transform;

            while (current != null)
            {
                var container = current.GetComponent<IContainerUI>();

                if (container != null)
                {
                    UpdateHover(null, container);
                    return;
                }

                var loot = current.GetComponent<LootItemView>();
                
                if (loot != null)
                {
                    UpdateHover(loot, null);
                    return;
                }

                current = current.parent;
            }

            UpdateHover(null, null);
        }

        private void HandleInteraction()
        {
            if (_currentHoveredContainer != null)
                TryInteractWithContainer(_currentHoveredContainer);
            else if (_currentHoveredLoot != null)
                TryPickUpLoot(_currentHoveredLoot);
        }

        private void UpdateHover(LootItemView newLoot, IContainerUI newContainer)
        {
            if (_currentHoveredLoot == newLoot && _currentHoveredContainer == newContainer)
                return;

            _currentHoveredLoot = newLoot;
            _currentHoveredContainer = newContainer;

            LootHoverChanged?.Invoke(newLoot);
            ContainerHoverChanged?.Invoke(newContainer);
        }

        private void TryInteractWithContainer(IContainerUI container)
        {
            if (!_inventoryManager.IsInventoryOpen)
                _inventoryManager.OpenInventory();

            _buildingContainerService.OpenContainer(container);
        }

        private void TryPickUpLoot(LootItemView loot)
        {
            var itemData = loot.ItemData;

            if (itemData == null)
                return;

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
                if (loot.ItemTable != null)
                    _deathLootStorage.RemoveItem(loot.ItemTable);

                loot.PlayCollectEffects();
                _lastHitCollider = null;
                UpdateHover(null, null);
                UnityEngine.Object.Destroy(loot.gameObject);
            }
        }
    }
}