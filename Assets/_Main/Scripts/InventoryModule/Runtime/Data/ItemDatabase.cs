using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InventoryModule
{
    public interface IItemDatabase
    {
        IReadOnlyList<ItemDefinition> AllItems { get; }
        ItemDefinition GetItem(string itemId);
        bool TryGetItem(string itemId, out ItemDefinition item);
        List<ItemDefinition> GetItemsByCategory(ItemCategory category);
        bool ContainsItem(string itemId);
    }

    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Inventory/Item Database")]
    public sealed class ItemDatabase : ScriptableObject, IItemDatabase
    {
        [SerializeField] private List<ItemDefinition> items = new();

        private Lazy<Dictionary<string, ItemDefinition>> _lazyCache;

        public IReadOnlyList<ItemDefinition> AllItems => items;

        private void Awake()
        {
            _lazyCache = new Lazy<Dictionary<string, ItemDefinition>>(() => new Dictionary<string, ItemDefinition>());
        }

        public ItemDefinition GetItem(string itemId)
        {
            EnsureCacheBuilt();

            if (_lazyCache.Value.TryGetValue(itemId, out var item))
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

        private void EnsureCacheBuilt()
        {
            if (_lazyCache.IsValueCreated)
                return;

            var cache = _lazyCache.Value;
            foreach (var item in items)
            {
                if (item == null)
                    continue;

                if (!cache.ContainsKey(item.Id))
                    cache[item.Id] = item;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _lazyCache = new Lazy<Dictionary<string, ItemDefinition>>(() => new Dictionary<string, ItemDefinition>());

            var seenIds = new HashSet<string>();
            foreach (var item in items)
            {
                if (item == null)
                    continue;

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