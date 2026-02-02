using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InventoryModule
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Inventory/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemDefinition> items = new();

        private Dictionary<string, ItemDefinition> _cache;

        public IReadOnlyList<ItemDefinition> AllItems => items;

        public ItemDefinition GetItem(string itemId)
        {
            if (_cache == null)
                BuildCache();

            if (_cache.TryGetValue(itemId, out var item))
                return item;

            return null;
        }

        public bool TryGetItem(string itemId, out ItemDefinition item)
        {
            item = GetItem(itemId);
            return item != null;
        }

        public List<ItemDefinition> GetItemsByCategory(ItemCategory category)
        {
            return items.Where(item => item.Category == category).ToList();
        }

        public bool ContainsItem(string itemId)
        {
            return items.Any(item => item.Id == itemId);
        }

        private void BuildCache()
        {
            _cache = new Dictionary<string, ItemDefinition>();

            foreach (var item in items)
            {
                if (item == null) continue;

                if (!_cache.ContainsKey(item.Id))
                {
                    _cache[item.Id] = item;
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _cache = null;

            var seenIds = new HashSet<string>();
            foreach (var item in items)
            {
                if (item == null) continue;

                if (seenIds.Contains(item.Id))
                {
                }
                else
                {
                    seenIds.Add(item.Id);
                }
            }
        }

        [ContextMenu("Sort Alphabetically")]
        private void SortAlphabetically()
        {
            items = items.OrderBy(x => x.ItemName).ToList();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}