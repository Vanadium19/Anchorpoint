using System.Collections.Generic;
using UnityEngine;
using Zenject;
using InputModule;

namespace InventoryModule
{
    public class InventoryManager : IInventoryManager, ITickable
    {
        private readonly List<GridTable> _additionalGrids = new();
        private readonly Dictionary<EquipmentSlotType, ItemTable> _equippedItems = new();

        private readonly IEquipmentSlotService _slotService;

        private GridTable _mainGrid;

        private IInputMap _inputMap;
        private IInputService _inputService;

        private GameObject _inventoryUI;

        private bool _isInventoryOpen;

        public GridTable MainGrid => _mainGrid;
        public bool IsInventoryOpen => _isInventoryOpen;

        public InventoryManager(IEquipmentSlotService slotService)
        {
            _slotService = slotService;
        }

        public void SetInput(IInputMap inputMap, IInputService inputService)
        {
            _inputMap = inputMap;
            _inputService = inputService;
        }

        public void SetMainGrid(GridTable grid) => _mainGrid = grid;

        public void RegisterAdditionalGrid(GridTable grid)
        {
            if (grid != null && !_additionalGrids.Contains(grid))
                _additionalGrids.Add(grid);
        }

        public void UnregisterAdditionalGrid(GridTable grid)
        {
            if (grid != null)
                _additionalGrids.Remove(grid);
        }

        public void SetInventoryUI(GameObject inventoryUI)
        {
            _inventoryUI = inventoryUI;

            if (_inventoryUI != null)
                _inventoryUI.SetActive(false);
        }

        public void Tick()
        {
            if (_inputMap is { IsInventoryPressed: true })
                ToggleInventory();
        }

        public void ToggleInventory()
        {
            if (_isInventoryOpen)
                CloseInventory();
            else
                OpenInventory();
        }

        public void OpenInventory()
        {
            _isInventoryOpen = true;
            _inventoryUI?.SetActive(true);
            _inputService?.SetUIMode(true);
        }

        public void CloseInventory()
        {
            _isInventoryOpen = false;
            _inventoryUI?.SetActive(false);
            _inputService?.SetUIMode(false);
        }

        public void SaveEquippedItem(EquipmentSlotType slotType, ItemTable item)
        {
            if (item == null)
                _equippedItems.Remove(slotType);
            else
                _equippedItems[slotType] = item;
        }

        public ItemTable GetEquippedItem(EquipmentSlotType slotType)
        {
            _equippedItems.TryGetValue(slotType, out var item);
            return item;
        }

        public void RemoveEquippedItem(EquipmentSlotType slotType) => _equippedItems.Remove(slotType);

        public int GetItemCount(ItemDataSo itemData)
        {
            if (itemData == null)
                return 0;

            var count = 0;
            var processedGrids = new HashSet<GridTable>();

            if (_mainGrid != null)
                count += CountItemsRecursive(_mainGrid, itemData, processedGrids);

            foreach (var additionalGrid in _additionalGrids)
            {
                if (!processedGrids.Contains(additionalGrid))
                    count += CountItemsRecursive(additionalGrid, itemData, processedGrids);
            }

            if (_slotService != null)
            {
                foreach (var slot in _slotService.GetAllSlots())
                {
                    if (slot.IsEquipped && slot.EquippedItem != null)
                        count += CountInEquippedItem(slot.EquippedItem, itemData, processedGrids);
                }
            }

            return count;
        }

        public bool TryRemoveItems(ItemDataSo itemData, int count)
        {
            if (itemData == null)
                return false;

            var available = GetItemCount(itemData);

            if (available < count)
                return false;

            var remaining = count;
            var processedGrids = new HashSet<GridTable>();

            if (_mainGrid != null)
                RemoveItemsRecursive(_mainGrid, itemData, ref remaining, processedGrids);

            foreach (var additionalGrid in _additionalGrids)
            {
                if (remaining > 0 && !processedGrids.Contains(additionalGrid))
                    RemoveItemsRecursive(additionalGrid, itemData, ref remaining, processedGrids);
            }

            if (remaining > 0 && _slotService != null)
            {
                foreach (var slot in _slotService.GetAllSlots())
                {
                    if (remaining <= 0)
                        break;

                    if (slot.IsEquipped && slot.EquippedItem != null)
                        RemoveFromEquippedItem(slot.EquippedItem, itemData, ref remaining, processedGrids);
                }
            }

            if (remaining <= 0)
                return remaining == 0;

            return remaining == 0;
        }

        public void ClearInventory()
        {
            if (_mainGrid != null)
            {
                var items = _mainGrid.GetAllItems();
                for (var i = 0; i < items.Length; i++)
                    items[i].RemoveItselfFromLocation();
            }

            foreach (var grid in _additionalGrids)
            {
                var items = grid.GetAllItems();
                for (var i = 0; i < items.Length; i++)
                    items[i].RemoveItselfFromLocation();
            }

            if (_slotService != null)
            {
                foreach (var slot in _slotService.GetAllSlots())
                {
                    if (slot.IsEquipped)
                        slot.Unequip();
                }
            }

            _equippedItems.Clear();
        }

        public bool AddItemToInventory(ItemDataSo itemData, int stackCount = 1)
        {
            if (itemData == null)
                return false;

            if (_mainGrid != null)
            {
                if (itemData.IsStackable)
                {
                    var items = _mainGrid.GetAllItems();

                    foreach (var item in items)
                    {
                        if (item.ItemDataSo != itemData || item.StackCount >= item.MaxStack)
                            continue;

                        var remaining = item.TryAddToStack(stackCount);

                        if (remaining == 0)
                            return true;

                        stackCount = remaining;
                    }
                }

                while (stackCount > 0)
                {
                    var itemTable = new ItemTable(itemData);
                    int toAdd = Mathf.Min(stackCount, itemData.MaxStackSize);
                    itemTable.StackCount = toAdd;

                    var pos = _mainGrid.FindSpaceForObjectAnyDirection(itemTable);

                    if (pos == null)
                        break;

                    _mainGrid.PlaceItem(itemTable, pos.Value.x, pos.Value.y);
                    stackCount -= toAdd;
                }
            }

            if (stackCount > 0)
                stackCount = TryAddToContainers(itemData, stackCount);

            return stackCount == 0;
        }

        public bool AddExistingItemToInventory(ItemTable existingItem)
        {
            if (existingItem == null)
                return false;

            existingItem.RemoveItselfFromLocation();

            var position = _mainGrid?.FindSpaceForObjectAnyDirection(existingItem);

            if (position == null)
                return TryAddExistingToContainers(existingItem);

            _mainGrid.PlaceItem(existingItem, position.Value.x, position.Value.y);
            return true;
        }

        public bool TryAutoEquipItem(ItemTable item)
        {
            if (item == null)
                return false;

            if (!item.ItemDataSo.IsEquippable)
                return false;

            if (_slotService == null)
                return false;

            foreach (var slot in _slotService.GetAllSlots())
            {
                if (slot.IsEquipped || !slot.CanEquip(item))
                    continue;

                if (slot.TryEquip(item))
                    return true;
            }

            return false;
        }

        private int CountInEquippedItem(ItemTable equippedItem, ItemDataSo itemData, HashSet<GridTable> processedGrids)
        {
            var count = 0;

            if (equippedItem.ItemDataSo == itemData)
                count += equippedItem.StackCount;

            if (!equippedItem.IsContainer)
                return count;

            var metadata = equippedItem.GetMetadata<ContainerMetadata>();

            if (metadata?.Inventories == null)
                return count;

            foreach (var containerGrid in metadata.Inventories)
                count += CountItemsRecursive(containerGrid, itemData, processedGrids);

            return count;
        }

        private void RemoveFromEquippedItem(ItemTable equippedItem, ItemDataSo itemData, ref int remaining, HashSet<GridTable> processedGrids)
        {
            if (equippedItem.ItemDataSo != itemData)
                return;

            if (!equippedItem.IsContainer)
            {
                if (equippedItem.StackCount <= remaining)
                {
                    remaining -= equippedItem.StackCount;
                    equippedItem.StackCount = 0;

                    if (_slotService != null)
                    {
                        var slot = _slotService.GetSlotForItem(equippedItem);
                        slot?.Unequip();
                    }
                }
                else
                {
                    equippedItem.StackCount -= remaining;
                    remaining = 0;
                }

                return;
            }

            var metadata = equippedItem.GetMetadata<ContainerMetadata>();

            if (metadata?.Inventories == null)
                return;

            foreach (var containerGrid in metadata.Inventories)
            {
                if (remaining > 0)
                {
                    RemoveItemsRecursive(containerGrid, itemData, ref remaining, processedGrids);
                }
            }
        }

        private int CountItemsRecursive(GridTable grid, ItemDataSo itemData, HashSet<GridTable> processedGrids)
        {
            if (grid == null || processedGrids.Contains(grid))
                return 0;

            processedGrids.Add(grid);

            var count = 0;
            var items = grid.GetAllItems();

            foreach (var item in items)
            {
                if (item.ItemDataSo == itemData)
                    count += item.StackCount;

                if (!item.IsContainer)
                    continue;

                var metadata = item.GetMetadata<ContainerMetadata>();

                if (metadata?.Inventories == null)
                    continue;

                foreach (var containerGrid in metadata.Inventories)
                    count += CountItemsRecursive(containerGrid, itemData, processedGrids);
            }

            return count;
        }

        private void RemoveItemsRecursive(GridTable grid, ItemDataSo itemData, ref int remainingCount, HashSet<GridTable> processedGrids)
        {
            if (grid == null || remainingCount <= 0 || !processedGrids.Add(grid))
                return;

            var items = grid.GetAllItems();
            var itemsToRemove = new List<ItemTable>();

            foreach (var item in items)
            {
                if (remainingCount <= 0)
                    break;

                if (item.ItemDataSo != itemData)
                    continue;

                if (item.StackCount <= remainingCount)
                {
                    remainingCount -= item.StackCount;
                    itemsToRemove.Add(item);
                }
                else
                {
                    item.AddAmount(-remainingCount);
                    remainingCount = 0;
                }
            }

            foreach (var item in itemsToRemove)
                grid.RemoveItem(item);

            if (remainingCount <= 0)
                return;

            foreach (var item in items)
            {
                if (remainingCount <= 0)
                    break;

                if (!item.IsContainer)
                    continue;

                var metadata = item.GetMetadata<ContainerMetadata>();

                if (metadata?.Inventories == null)
                    continue;

                foreach (var containerGrid in metadata.Inventories)
                {
                    if (remainingCount > 0)
                        RemoveItemsRecursive(containerGrid, itemData, ref remainingCount, processedGrids);
                }
            }
        }

        private int TryAddToContainers(ItemDataSo itemData, int stackCount)
        {
            if (_mainGrid != null)
                stackCount = TryAddToContainersInGrid(_mainGrid, itemData, stackCount);

            if (stackCount <= 0 || _slotService == null)
                return stackCount;

            foreach (var slot in _slotService.GetAllSlots())
            {
                if (stackCount <= 0)
                    break;

                if (slot.IsEquipped && slot.EquippedItem != null && slot.EquippedItem.IsContainer)
                    stackCount = TryAddToContainerItem(slot.EquippedItem, itemData, stackCount);
            }

            return stackCount;
        }

        private int TryAddToContainersInGrid(GridTable grid, ItemDataSo itemData, int stackCount)
        {
            var items = grid.GetAllItems();

            foreach (var item in items)
            {
                if (stackCount <= 0)
                    break;

                if (item.IsContainer)
                    stackCount = TryAddToContainerItem(item, itemData, stackCount);
            }

            return stackCount;
        }

        private int TryAddToContainerItem(ItemTable containerItem, ItemDataSo itemData, int stackCount)
        {
            var metadata = containerItem.GetMetadata<ContainerMetadata>();

            if (metadata?.Inventories == null)
                return stackCount;

            foreach (var containerGrid in metadata.Inventories)
            {
                if (stackCount <= 0)
                    break;

                if (itemData.IsStackable)
                {
                    var items = containerGrid.GetAllItems();

                    foreach (var item in items)
                    {
                        if (item.ItemDataSo != itemData || item.StackCount >= item.MaxStack)
                            continue;

                        var remaining = item.TryAddToStack(stackCount);
                        stackCount = remaining;

                        if (stackCount == 0)
                            break;
                    }
                }

                while (stackCount > 0)
                {
                    var itemTable = new ItemTable(itemData);
                    var toAdd = Mathf.Min(stackCount, itemData.MaxStackSize);
                    itemTable.StackCount = toAdd;

                    var pos = containerGrid.FindSpaceForObjectAnyDirection(itemTable);

                    if (pos == null)
                        break;

                    containerGrid.PlaceItem(itemTable, pos.Value.x, pos.Value.y);
                    stackCount -= toAdd;
                }
            }

            return stackCount;
        }

        private bool TryAddExistingToContainers(ItemTable existingItem)
        {
            if (_mainGrid != null)
                if (TryAddExistingToContainersInGrid(_mainGrid, existingItem))
                    return true;

            if (_slotService == null)
                return false;

            foreach (var slot in _slotService.GetAllSlots())
            {
                if (!slot.IsEquipped || slot.EquippedItem is not { IsContainer: true })
                    continue;

                var metadata = slot.EquippedItem.GetMetadata<ContainerMetadata>();

                if (metadata?.Inventories == null)
                    continue;

                foreach (var containerGrid in metadata.Inventories)
                {
                    var pos = containerGrid.FindSpaceForObjectAnyDirection(existingItem);

                    if (pos == null)
                        continue;

                    containerGrid.PlaceItem(existingItem, pos.Value.x, pos.Value.y);
                    return true;
                }
            }

            return false;
        }

        private bool TryAddExistingToContainersInGrid(GridTable grid, ItemTable existingItem)
        {
            var items = grid.GetAllItems();

            foreach (var item in items)
            {
                if (!item.IsContainer)
                    continue;

                var metadata = item.GetMetadata<ContainerMetadata>();

                if (metadata?.Inventories == null)
                    continue;

                foreach (var containerGrid in metadata.Inventories)
                {
                    var position = containerGrid.FindSpaceForObjectAnyDirection(existingItem);

                    if (position == null)
                        continue;

                    containerGrid.PlaceItem(existingItem, position.Value.x, position.Value.y);
                    return true;
                }
            }

            return false;
        }
    }
}