using System;
using System.Collections.Generic;

namespace InventoryModule
{
    public sealed class InventoryModel
    {
        private readonly List<InventoryItem> _items = new();

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<InventoryItem> Items => _items;

        public InventoryModel(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Inventory dimensions must be positive");

            Width = width;
            Height = height;
        }

        public void AddItem(InventoryItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (_items.Contains(item))
                throw new InvalidOperationException("Item already exists in inventory");

            _items.Add(item);
        }

        public void RemoveItem(InventoryItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (!_items.Contains(item))
                throw new InvalidOperationException("Item not found in inventory");

            _items.Remove(item);
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}