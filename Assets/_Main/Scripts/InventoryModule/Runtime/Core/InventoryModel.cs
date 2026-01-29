using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public class InventoryModel
    {
        private readonly int _width;
        private readonly int _height;
        private readonly List<InventoryItem> _items = new();

        public event Action Updated;

        public InventoryModel(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public IEnumerable<InventoryItem> Items => _items;
        public int Width => _width;
        public int Height => _height;

        public bool TryAddItem(ItemConfig config, int amount)
        {
            foreach (var item in _items)
            {
                if (item.Id != config.Id)
                    continue;

                if (item.Amount >= item.MaxStack)
                    continue;

                item.TryAddAmount(amount, out int remainder);
                amount = remainder;

                if (amount <= 0)
                {
                    Updated?.Invoke();
                    return true;
                }
            }

            while (amount > 0)
            {
                if (!FindSpaceFor(config.Width, config.Height, out Vector2Int position))
                    return false;

                int addAmount = Mathf.Min(amount, config.MaxStack);
                var newItem = new InventoryItem(config.Id, position, new(config.Width, config.Height), config.MaxStack, addAmount);

                _items.Add(newItem);
                amount -= addAmount;
            }

            Updated?.Invoke();
            return true;
        }

        public void Clear()
        {
            _items.Clear();
            Updated?.Invoke();
        }

        private bool FindSpaceFor(int itemW, int itemH, out Vector2Int position)
        {
            position = new Vector2Int(-1, -1);

            for (int y = 0; y <= _height - itemH; y++)
            {
                for (int x = 0; x <= _width - itemW; x++)
                {
                    if (IsAreaFree(x, y, itemW, itemH))
                    {
                        position = new Vector2Int(x, y);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsAreaFree(int startX, int startY, int width, int height)
        {
            foreach (var item in _items)
            {
                if (startX < item.Position.x + item.Size.x &&
                    startX + width > item.Position.x &&
                    startY < item.Position.y + item.Size.y &&
                    startY + height > item.Position.y)
                {
                    return false;
                }
            }
            return true;
        }
    }
}