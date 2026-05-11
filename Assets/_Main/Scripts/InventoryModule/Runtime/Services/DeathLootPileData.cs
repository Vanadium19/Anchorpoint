using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public class DeathLootPileData
    {
        private readonly List<ItemTable> _items = new();

        public string SceneName { get; }
        public Vector3 Position { get; }

        public IReadOnlyList<ItemTable> Items => _items;

        public bool HasItems => _items.Count > 0;

        public DeathLootPileData(string sceneName, Vector3 position, List<ItemTable> items)
        {
            SceneName = sceneName;
            Position = position;

            if (items == null)
                return;

            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];

                if (item == null || item.ItemDataSo == null || !item.ItemDataSo.IsDropable)
                    continue;

                _items.Add(item);
            }
        }

        public bool RemoveItem(ItemTable item)
        {
            if (item == null)
                return false;

            return _items.Remove(item);
        }
    }
}