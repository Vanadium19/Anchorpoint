using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace InventoryModule
{
    public sealed class InventoryService : IInventoryService
    {
        private readonly InventoryModel _model;
        private readonly GridService _gridService;
        private readonly ItemDatabase _itemDatabase;

        public event Action InventoryUpdated;
        public IReadOnlyList<InventoryItem> GetAllItems() => _model.Items;
        public int Width => _model.Width;
        public int Height => _model.Height;
        public InventoryService(InventoryModel model, ItemDatabase itemDatabase)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));
            _gridService = new GridService(model.Width, model.Height);
            InitializeGridWithExistingItems();
        }

        private void InitializeGridWithExistingItems()
        {
            foreach (var item in _model.Items)
            {
                _gridService.PlaceItem(item, item.Position, item.IsRotated);
            }
        }
        public UniTask<InventoryOperationResult> AddItemAsync(
            ItemDefinition itemDefinition,
            int amount,
            CancellationToken token)
        {
            if (itemDefinition == null)
                return UniTask.FromResult(InventoryOperationResult.CreateFailure("Item definition is null"));

            if (amount <= 0)
                return UniTask.FromResult(InventoryOperationResult.CreateFailure("Amount must be positive"));

            token.ThrowIfCancellationRequested();

            try
            {
                int remainingAmount = amount;

                if (itemDefinition.MaxStack > 1)
                {
                    remainingAmount = TryStackExistingItems(itemDefinition, remainingAmount);
                    if (remainingAmount <= 0)
                    {
                        InventoryUpdated?.Invoke();
                        return UniTask.FromResult(InventoryOperationResult.CreateSuccess());
                    }
                }

                while (remainingAmount > 0)
                {
                    var placementResult = TryFindPlaceForItem(itemDefinition, out var position, out var isRotated);
                    if (!placementResult.Success)
                    {
                        return UniTask.FromResult(InventoryOperationResult.CreateFailure(
                            $"No space for item: {itemDefinition.ItemName}"));
                    }

                    int stackAmount = Mathf.Min(remainingAmount, itemDefinition.MaxStack);
                    var newItem = CreateInventoryItem(itemDefinition, position, stackAmount, isRotated);

                    _gridService.PlaceItem(newItem, position, isRotated);
                    _model.AddItem(newItem);

                    remainingAmount -= stackAmount;
                }

                InventoryUpdated?.Invoke();
                return UniTask.FromResult(InventoryOperationResult.CreateSuccess());
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to add item: {exception}");
                return UniTask.FromResult(InventoryOperationResult.CreateFailure($"Internal error: {exception.Message}"));
            }
        }

        public UniTask<InventoryOperationResult> MoveItemAsync(
            InventoryItem item,
            Vector2Int newPosition,
            bool isRotated,
            CancellationToken token)
        {
            if (item == null)
                return UniTask.FromResult(InventoryOperationResult.CreateFailure("Item is null"));

            token.ThrowIfCancellationRequested();

            try
            {
                var targetItem = GetItemAtPosition(newPosition);

                if (targetItem != null && targetItem != item)
                {
                    if (targetItem.Id == item.Id && targetItem.Amount < targetItem.MaxStack)
                    {
                        int canTake = targetItem.MaxStack - targetItem.Amount;
                        int toMove = Mathf.Min(canTake, item.Amount);

                        targetItem.Amount += toMove;
                        item.Amount -= toMove;

                        if (item.Amount <= 0)
                        {

                            _gridService.ClearItem(item);
                            _model.RemoveItem(item);
                        }

                        InventoryUpdated?.Invoke();
                        return UniTask.FromResult(InventoryOperationResult.CreateSuccess(item));
                    }
                }

                if (!CanPlaceItemAt(item, newPosition, isRotated, item))
                {
                    return UniTask.FromResult(InventoryOperationResult.CreateFailure("Position occupied"));
                }

                _gridService.ClearItem(item);
                _gridService.PlaceItem(item, newPosition, isRotated);

                item.Position = newPosition;
                item.IsRotated = isRotated;

                InventoryUpdated?.Invoke();
                return UniTask.FromResult(InventoryOperationResult.CreateSuccess(item));
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to move item: {exception}");
                return UniTask.FromResult(InventoryOperationResult.CreateFailure(exception.Message));
            }
        }

        public UniTask<InventoryOperationResult> SplitItemAsync(
            InventoryItem item,
            Vector2Int splitPosition,
            bool isRotated,
            CancellationToken token)
        {
            if (item == null)
                return UniTask.FromResult(InventoryOperationResult.CreateFailure("Item is null"));

            if (item.Amount < 2)
                return UniTask.FromResult(InventoryOperationResult.CreateFailure("Cannot split: item amount less than 2"));

            token.ThrowIfCancellationRequested();

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

                        targetItem.Amount += actualMove;
                        item.Amount -= actualMove;

                        InventoryUpdated?.Invoke();
                        return UniTask.FromResult(InventoryOperationResult.CreateSuccess(targetItem));
                    }
                }

                var splitSize = item.GetSize(isRotated);
                var originalSize = item.GetSize(item.IsRotated);

                if (_gridService.AreasOverlap(splitPosition, splitSize, item.Position, originalSize))
                {
                    return UniTask.FromResult(InventoryOperationResult.CreateFailure(
                        "Cannot split: overlap with original item"));
                }

                if (!_gridService.IsAreaFree(splitPosition, splitSize))
                {
                    return UniTask.FromResult(InventoryOperationResult.CreateFailure(
                        "Cannot split: target position is occupied"));
                }

                item.Amount -= moveAmount;

                var newItem = CreateInventoryItem(
                    _itemDatabase.GetItem(item.Id),
                    splitPosition,
                    moveAmount,
                    isRotated);

                _gridService.PlaceItem(newItem, splitPosition, isRotated);
                _model.AddItem(newItem);

                InventoryUpdated?.Invoke();
                return UniTask.FromResult(InventoryOperationResult.CreateSuccess(newItem));
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to split item: {exception}");
                return UniTask.FromResult(InventoryOperationResult.CreateFailure($"Internal error: {exception.Message}"));
            }
        }

        public UniTask<InventoryOperationResult> RemoveItemAsync(
            InventoryItem item,
            CancellationToken token)
        {
            if (item == null)
                return UniTask.FromResult(InventoryOperationResult.CreateFailure("Item is null"));

            token.ThrowIfCancellationRequested();

            try
            {
                _gridService.ClearItem(item);
                _model.RemoveItem(item);

                InventoryUpdated?.Invoke();
                return UniTask.FromResult(InventoryOperationResult.CreateSuccess(item));
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to remove item: {exception}");
                return UniTask.FromResult(InventoryOperationResult.CreateFailure($"Internal error: {exception.Message}"));
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

                    existingItem.Amount += toAdd;
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

            return InventoryOperationResult.CreateFailure("No free space found");
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
            return new InventoryItem(
                itemDefinition.Id,
                position,
                new Vector2Int(itemDefinition.Width, itemDefinition.Height),
                itemDefinition.MaxStack,
                amount)
            {
                IsRotated = isRotated
            };
        }
        public void ClearInventory()
        {
            _gridService.ClearAll();
            _model.Clear();
            InventoryUpdated?.Invoke();
        }
    }
}