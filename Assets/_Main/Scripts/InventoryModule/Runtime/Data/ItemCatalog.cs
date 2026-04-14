using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InventoryModule
{
    [CreateAssetMenu(fileName = "ItemCatalog", menuName = "Game/Configs/Inventory/ItemCatalog")]
    public class ItemCatalog : ScriptableObject
    {
        [SerializeField] private List<ItemDataSo> items;

        private Dictionary<string, ItemDataSo> _itemCache;

        private void OnValidate()
        {
            if (items == null || items.Count == 0)
                return;

            var validItems = items.Where(i => i != null && !string.IsNullOrEmpty(i.name)).ToList();
            var duplicates = validItems.GroupBy(i => i.name).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

            if (duplicates.Count > 0)
                throw new System.InvalidOperationException("Duplicate items found: " + string.Join(", ", duplicates));
        }

        public ItemDataSo GetByName(string name)
        {
            BuildCache();
            return _itemCache.TryGetValue(name, out var item) ? item : null;
        }

        public IReadOnlyList<ItemDataSo> GetAll() => items;

        private void BuildCache()
        {
            if (_itemCache != null)
                return;

            _itemCache = new Dictionary<string, ItemDataSo>();

            foreach (var item in items)
            {
                if (item != null && !_itemCache.ContainsKey(item.name))
                    _itemCache[item.name] = item;
            }
        }
    }
}