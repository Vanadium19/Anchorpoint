using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public class InventoryModel
    {
        private readonly InventoryGrid _grid;
        private readonly List<InventoryItem> _items = new List<InventoryItem>();

        public event Action Updated;

        public InventoryModel(int width, int height)
        {
            _grid = new InventoryGrid(width, height);
        }

        public IEnumerable<InventoryItem> Items => _items;

        public bool TryAddItem(ItemConfig config, int amount)
        {
            if (config.MaxStack > 1)
            {
                foreach (var item in _items)
                {
                    if (item.Id == config.Id && item.Amount < item.MaxStack)
                    {
                        item.TryAddAmount(amount, out int remainder);
                        int added = amount - remainder;

                        if (added > 0)
                        {
                            amount = remainder;
                            Updated?.Invoke();

                            if (amount <= 0)
                                return true;
                        }
                    }
                }
            }

            while (amount > 0)
            {
                if (!TryFindPlaceForItem(config, out Vector2Int position, out bool isRotated))
                {
                    Updated?.Invoke();
                    return false;
                }

                int toAdd = Mathf.Min(amount, config.MaxStack);
                var newItem = new InventoryItem(
                    config.Id,
                    position,
                    new Vector2Int(config.Width, config.Height),
                    config.MaxStack,
                    toAdd
                );
                newItem.IsRotated = isRotated;

                _grid.PlaceItem(newItem, position, isRotated);
                _items.Add(newItem);
                amount -= toAdd;

                Updated?.Invoke();
            }

            return true;
        }

        public bool TryMoveItem(InventoryItem item, Vector2Int newPosition, bool isRotated, Vector2Int? mouseGridPos = null)
        {
            Vector2Int size = item.GetSize(isRotated);

            if (mouseGridPos.HasValue && item.MaxStack > 1)
            {
                var targetAtCursor = GetItemAt(mouseGridPos.Value.x, mouseGridPos.Value.y, ignoreItem: item);

                if (targetAtCursor != null && targetAtCursor.Id == item.Id)
                {
                    int spaceAvailable = targetAtCursor.MaxStack - targetAtCursor.Amount;

                    if (spaceAvailable > 0)
                    {
                        int amountToAdd = Mathf.Min(item.Amount, spaceAvailable);
                        targetAtCursor.TryAddAmount(amountToAdd, out int _);
                        item.Amount -= amountToAdd;

                        if (item.Amount <= 0)
                        {
                            _items.Remove(item);
                            _grid.ClearItem(item);
                            Updated?.Invoke();
                            return true;
                        }
                        else
                        {
                            Updated?.Invoke();
                            return true;
                        }
                    }
                }
            }
            if (_grid.IsAreaFree(newPosition, size, item))
            {
                _grid.PlaceItem(item, newPosition, isRotated);
                Updated?.Invoke();
                return true;
            }
            var overlappingItems = _grid.GetItemsAtArea(newPosition, size);
            overlappingItems.Remove(item);

            if (overlappingItems.Count == 1)
            {
                var obstacle = overlappingItems[0];
                Vector2Int obstacleSize = obstacle.GetSize(obstacle.IsRotated);

                if (_grid.IsAreaFree(item.Position, obstacleSize, obstacle))
                {
                    bool swapped = _grid.TrySwapItems(
                        item, newPosition, isRotated,
                        obstacle, item.Position, obstacle.IsRotated
                    );

                    if (swapped)
                    {
                        Updated?.Invoke();
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TrySplitItem(InventoryItem originalItem, Vector2Int targetPos, bool isRotated)
        {
            if (originalItem.Amount < 2)
                return false;

            Vector2Int splitSize = originalItem.GetSize(isRotated);
            Vector2Int originalSize = originalItem.GetSize(originalItem.IsRotated);
            if (_grid.AreasOverlap(targetPos, splitSize, originalItem.Position, originalSize))
                return false;

            int moveAmount = originalItem.Amount / 2;
            int keepAmount = originalItem.Amount - moveAmount;
            if (_grid.IsAreaFree(targetPos, splitSize))
            {
                originalItem.Amount = keepAmount;

                var newItem = new InventoryItem(
                    originalItem.Id,
                    targetPos,
                    originalItem.BaseSize,
                    originalItem.MaxStack,
                    moveAmount
                );
                newItem.IsRotated = isRotated;

                _grid.PlaceItem(newItem, targetPos, isRotated);
                _items.Add(newItem);
                Updated?.Invoke();
                return true;
            }
            var overlappingItems = _grid.GetItemsAtArea(targetPos, splitSize);
            InventoryItem targetItem = null;

            foreach (var overlap in overlappingItems)
            {
                if (overlap != originalItem && overlap.Id == originalItem.Id && overlap.MaxStack > 1)
                {
                    targetItem = overlap;
                    break;
                }
            }

            if (targetItem != null)
            {
                int spaceAvailable = targetItem.MaxStack - targetItem.Amount;
                if (spaceAvailable > 0)
                {
                    int toAdd = Mathf.Min(moveAmount, spaceAvailable);
                    targetItem.TryAddAmount(toAdd, out int _);
                    originalItem.Amount -= toAdd;

                    Updated?.Invoke();
                    return toAdd > 0;
                }
            }

            return false;
        }

        private bool TryFindPlaceForItem(ItemConfig config, out Vector2Int position, out bool isRotated)
        {
            Vector2Int size = new Vector2Int(config.Width, config.Height);
            for (int y = 0; y <= _grid.Height - size.y; y++)
            {
                for (int x = 0; x <= _grid.Width - size.x; x++)
                {
                    var testPos = new Vector2Int(x, y);
                    if (_grid.IsAreaFree(testPos, size))
                    {
                        position = testPos;
                        isRotated = false;
                        return true;
                    }
                }
            }
            Vector2Int rotatedSize = new Vector2Int(size.y, size.x);
            for (int y = 0; y <= _grid.Height - rotatedSize.y; y++)
            {
                for (int x = 0; x <= _grid.Width - rotatedSize.x; x++)
                {
                    var testPos = new Vector2Int(x, y);
                    if (_grid.IsAreaFree(testPos, rotatedSize))
                    {
                        position = testPos;
                        isRotated = true;
                        return true;
                    }
                }
            }

            position = Vector2Int.zero;
            isRotated = false;
            return false;
        }

        private InventoryItem GetItemAt(int x, int y, InventoryItem ignoreItem = null)
        {
            foreach (var item in _items)
            {
                if (item == ignoreItem) continue;

                Vector2Int itemSize = item.GetSize(item.IsRotated);
                int w = itemSize.x;
                int h = itemSize.y;

                if (x >= item.Position.x && x < item.Position.x + w &&
                    y >= item.Position.y && y < item.Position.y + h)
                {
                    return item;
                }
            }
            return null;
        }

        public void Clear()
        {
            foreach (var item in _items)
            {
                _grid.ClearItem(item);
            }
            _items.Clear();
            Updated?.Invoke();
        }
    }
}