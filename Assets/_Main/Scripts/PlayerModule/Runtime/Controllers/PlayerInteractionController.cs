using EffectModule;
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
        private readonly IItemUseHandler _itemUseHandler;
        private readonly IPrimaryBuffTargetService _primaryBuffTargetService;

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
            IDeathLootStorage deathLootStorage,
            IItemUseHandler itemUseHandler,
            IPrimaryBuffTargetService primaryBuffTargetService)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
            _buildingContainerService = buildingContainerService;
            _camera = camera;
            _config = config;
            _deathLootStorage = deathLootStorage;
            _itemUseHandler = itemUseHandler ?? throw new ArgumentNullException(nameof(itemUseHandler));
            _primaryBuffTargetService = primaryBuffTargetService ?? throw new ArgumentNullException(nameof(primaryBuffTargetService));
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

            if (itemData.PickupBehavior == ItemPickupBehavior.UseImmediately)
            {
                TryUseImmediately(loot, itemData);
                return;
            }

            if (TryStoreInInventory(loot, itemData))
                CompletePickup(loot);
        }

        private void TryUseImmediately(LootItemView loot, ItemDataSo itemData)
        {
            var effectTarget = _primaryBuffTargetService.PrimaryTarget;
            var effectData = itemData.EffectData;

            if (effectTarget == null || effectData == null)
                return;

            var item = loot.ItemTable ?? new ItemTable(itemData) { StackCount = loot.Amount };
            var availableAmount = Mathf.Max(1, loot.Amount);
            var usedAmount = 0;

            while (usedAmount < availableAmount && effectData.CanApply(effectTarget))
            {
                _itemUseHandler.UseItem(item, effectTarget);
                usedAmount++;
            }

            if (usedAmount == 0)
                return;

            var remainingAmount = availableAmount - usedAmount;

            if (remainingAmount > 0)
            {
                loot.SetAmount(remainingAmount);
                loot.PlayCollectEffects();
                return;
            }

            CompletePickup(loot);
        }

        private bool TryStoreInInventory(LootItemView loot, ItemDataSo itemData)
        {
            if (itemData.IsEquippable)
            {
                var isEquipped = loot.ItemTable != null
                    ? _inventoryManager.TryAutoEquipItem(loot.ItemTable)
                    : _inventoryManager.TryAutoEquipItem(new ItemTable(itemData) { StackCount = loot.Amount });

                if (isEquipped)
                    return true;
            }

            return loot.ItemTable != null
                ? _inventoryManager.AddExistingItemToInventory(loot.ItemTable)
                : _inventoryManager.AddItemToInventory(itemData, loot.Amount);
        }

        private void CompletePickup(LootItemView loot)
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
