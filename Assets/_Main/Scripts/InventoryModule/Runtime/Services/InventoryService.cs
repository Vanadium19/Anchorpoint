using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public sealed class InventoryService : IInventoryService
    {
        private readonly InventoryModel _model;
        private readonly IGridService _gridService;
        private readonly IItemDatabase _itemDatabase;

        public event Action InventoryUpdated;
        public IReadOnlyList<InventoryItem> GetAllItems() => _model.Items;
        public int Width => _model.Width;
        public int Height => _model.Height;

        public InventoryService(InventoryModel model, IItemDatabase itemDatabase, IGridService gridService)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));
            _gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
            InitializeGridWithExistingItems();
        }

        private void InitializeGridWithExistingItems()
        {
            foreach (var item in _model.Items)
            {
                _gridService.PlaceItem(item, item.Position, item.IsRotated);
            }
        }

        public InventoryOperationResult AddItem(ItemDefinition itemDefinition, int amount)
        {
            if (itemDefinition == null)
                return InventoryOperationResult.CreateFailure(InventoryError.InvalidItem);

            if (amount <= 0)
                return InventoryOperationResult.CreateFailure(InventoryError.InvalidItem);

            try
            {
                int remainingAmount = amount;

                if (itemDefinition.MaxStack > 1)
                {
                    remainingAmount = TryStackExistingItems(itemDefinition, remainingAmount);
                    if (remainingAmount <= 0)
                    {
                        InventoryUpdated?.Invoke();
                        return InventoryOperationResult.CreateSuccess();
                    }
                }

                while (remainingAmount > 0)
                {
                    var placementResult = TryFindPlaceForItem(itemDefinition, out var position, out var isRotated);
                    if (!placementResult.Success)
                    {
                        return InventoryOperationResult.CreateFailure(InventoryError.NoSpace);
                    }

                    int stackAmount = Mathf.Min(remainingAmount, itemDefinition.MaxStack);
                    var newItem = CreateInventoryItem(itemDefinition, position, stackAmount, isRotated);

                    _gridService.PlaceItem(newItem, position, isRotated);
                    _model.AddItem(newItem);

                    remainingAmount -= stackAmount;
                }

                InventoryUpdated?.Invoke();
                return InventoryOperationResult.CreateSuccess();
            }
            catch (Exception)
            {
                return InventoryOperationResult.CreateFailure(InventoryError.InternalError);
            }
        }

        public InventoryOperationResult MoveItem(InventoryItem item, Vector2Int newPosition, bool isRotated)
        {
            if (item == null)
                return InventoryOperationResult.CreateFailure(InventoryError.InvalidItem);

            try
            {
                var targetItem = GetItemAtPosition(newPosition);

                if (targetItem != null && targetItem != item)
                {
                    if (targetItem.Id == item.Id && targetItem.Amount < targetItem.MaxStack)
                    {
                        int canTake = targetItem.MaxStack - targetItem.Amount;
                        int toMove = Mathf.Min(canTake, item.Amount);

                        targetItem.AddAmount(toMove);
                        item.AddAmount(-toMove);

                        if (item.Amount <= 0)
                        {
                            _gridService.ClearItem(item);
                            _model.RemoveItem(item);
                        }

                        InventoryUpdated?.Invoke();
                        return InventoryOperationResult.CreateSuccess(item);
                    }
                }

                if (!CanPlaceItemAt(item, newPosition, isRotated, item))
                {
                    return InventoryOperationResult.CreateFailure(InventoryError.PositionOccupied);
                }

                _gridService.ClearItem(item);
                _gridService.PlaceItem(item, newPosition, isRotated);

                item.SetPosition(newPosition);
                item.SetIsRotated(isRotated);

                InventoryUpdated?.Invoke();
                return InventoryOperationResult.CreateSuccess(item);
            }
            catch (Exception)
            {
                return InventoryOperationResult.CreateFailure(InventoryError.InternalError);
            }
        }

        public InventoryOperationResult SplitItem(InventoryItem item, Vector2Int splitPosition, bool isRotated)
        {
            if (item == null)
                return InventoryOperationResult.CreateFailure(InventoryError.InvalidItem);

            if (item.Amount < 2)
                return InventoryOperationResult.CreateFailure(InventoryError.CannotSplit);

            try
            {
                int moveAmount = item.Amount / 2;

                var targetItem = GetItemAtPosition(splitPosition);

                if (targetItem != null && targetItem != item && targetItem.Id == item.Id)
                {
                    if (targetItem.Amount < targetItem.MaxStack)
                    {
                        int canAccept = targetItem.MaxStack - targetItem.Amount;
                        int actualMove = Mathf.Min(moveAmount, canAccept);

                        targetItem.AddAmount(actualMove);
                        item.AddAmount(-actualMove);

                        InventoryUpdated?.Invoke();
                        return InventoryOperationResult.CreateSuccess(targetItem);
                    }
                }

                var splitSize = item.GetSize(isRotated);
                var originalSize = item.GetSize(item.IsRotated);

                if (_gridService.AreasOverlap(splitPosition, splitSize, item.Position, originalSize))
                {
                    return InventoryOperationResult.CreateFailure(InventoryError.CannotSplit);
                }

                if (!_gridService.IsAreaFree(splitPosition, splitSize))
                {
                    return InventoryOperationResult.CreateFailure(InventoryError.NoSpace);
                }

                item.AddAmount(-moveAmount);

                var newItem = CreateInventoryItem(
                    _itemDatabase.GetItem(item.Id),
                    splitPosition,
                    moveAmount,
                    isRotated);

                _gridService.PlaceItem(newItem, splitPosition, isRotated);
                _model.AddItem(newItem);

                InventoryUpdated?.Invoke();
                return InventoryOperationResult.CreateSuccess(newItem);
            }
            catch (Exception)
            {
                return InventoryOperationResult.CreateFailure(InventoryError.InternalError);
            }
        }

        public InventoryOperationResult RemoveItem(InventoryItem item)
        {
            if (item == null)
                return InventoryOperationResult.CreateFailure(InventoryError.InvalidItem);

            try
            {
                _gridService.ClearItem(item);
                _model.RemoveItem(item);

                InventoryUpdated?.Invoke();
                return InventoryOperationResult.CreateSuccess(item);
            }
            catch (Exception)
            {
                return InventoryOperationResult.CreateFailure(InventoryError.InternalError);
            }
        }

        public bool CanPlaceItem(ItemDefinition itemDefinition, Vector2Int position, bool isRotated)
        {
            if (itemDefinition == null)
                return false;

            var size = itemDefinition.CanRotate && isRotated
                ? new Vector2Int(itemDefinition.Height, itemDefinition.Width)
                : new Vector2Int(itemDefinition.Width, itemDefinition.Height);

            return _gridService.IsAreaFree(position, size);
        }

        public InventoryItem GetItemAtPosition(Vector2Int position)
        {
            foreach (var item in _model.Items)
            {
                var itemSize = item.GetSize(item.IsRotated);
                if (position.x >= item.Position.x && position.x < item.Position.x + itemSize.x &&
                    position.y >= item.Position.y && position.y < item.Position.y + itemSize.y)
                {
                    return item;
                }
            }

            return null;
        }

        public void UpdateInventory() => InventoryUpdated?.Invoke();

        private int TryStackExistingItems(ItemDefinition itemDefinition, int amount)
        {
            int remainingAmount = amount;

            foreach (var existingItem in _model.Items)
            {
                if (existingItem.Id == itemDefinition.Id && existingItem.Amount < existingItem.MaxStack)
                {
                    int space = existingItem.MaxStack - existingItem.Amount;
                    int toAdd = Mathf.Min(space, remainingAmount);

                    existingItem.AddAmount(toAdd);
                    remainingAmount -= toAdd;

                    if (remainingAmount <= 0)
                        break;
                }
            }

            return remainingAmount;
        }

        private InventoryOperationResult TryFindPlaceForItem(
            ItemDefinition itemDefinition,
            out Vector2Int position,
            out bool isRotated)
        {
            position = Vector2Int.zero;
            isRotated = false;

            var size = new Vector2Int(itemDefinition.Width, itemDefinition.Height);

            for (int y = 0; y <= _gridService.Height - size.y; y++)
            {
                for (int x = 0; x <= _gridService.Width - size.x; x++)
                {
                    var testPos = new Vector2Int(x, y);
                    if (_gridService.IsAreaFree(testPos, size))
                    {
                        position = testPos;
                        isRotated = false;
                        return InventoryOperationResult.CreateSuccess();
                    }
                }
            }

            if (itemDefinition.CanRotate)
            {
                var rotatedSize = new Vector2Int(size.y, size.x);
                for (int y = 0; y <= _gridService.Height - rotatedSize.y; y++)
                {
                    for (int x = 0; x <= _gridService.Width - rotatedSize.x; x++)
                    {
                        var testPos = new Vector2Int(x, y);
                        if (_gridService.IsAreaFree(testPos, rotatedSize))
                        {
                            position = testPos;
                            isRotated = true;
                            return InventoryOperationResult.CreateSuccess();
                        }
                    }
                }
            }

            return InventoryOperationResult.CreateFailure(InventoryError.NoSpace);
        }

        private bool CanPlaceItemAt(InventoryItem item, Vector2Int position, bool isRotated, InventoryItem ignoreItem)
        {
            var size = item.GetSize(isRotated);
            return _gridService.IsAreaFree(position, size, ignoreItem);
        }

        private InventoryItem CreateInventoryItem(
            ItemDefinition itemDefinition,
            Vector2Int position,
            int amount,
            bool isRotated)
        {
            var item = new InventoryItem(
                itemDefinition.Id,
                position,
                new Vector2Int(itemDefinition.Width, itemDefinition.Height),
                itemDefinition.MaxStack,
                amount);

            item.SetIsRotated(isRotated);

            return item;
        }

        public void ClearInventory()
        {
            _gridService.ClearAll();
            _model.Clear();
            InventoryUpdated?.Invoke();
        }
    }
}
