using UnityEngine;

namespace InventoryModule
{
    public sealed class ItemProvider : IItemProvider
    {
        private readonly ItemDatabase _itemDatabase;

        public ItemProvider(ItemDatabase itemDatabase)
        {
            _itemDatabase = itemDatabase;
        }

        public ItemDefinition GetItemDefinition(string itemId)
            => _itemDatabase.GetItem(itemId);

        public bool TryGetItemDefinition(string itemId, out ItemDefinition itemDefinition)
            => _itemDatabase.TryGetItem(itemId, out itemDefinition);

        public Sprite GetItemIcon(string itemId)
            => _itemDatabase.GetItem(itemId)?.Icon;
    }
}