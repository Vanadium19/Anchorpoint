using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public interface ILootItemFactory
    {
        LootItemView Create(ItemDefinition definition, int amount, Vector3 position, Quaternion rotation);
    }

    public sealed class LootItemFactory : IFactory<ItemDefinition, int, Vector3, Quaternion, LootItemView>, ILootItemFactory
    {
        private readonly DiContainer _container;

        public LootItemFactory(DiContainer container)
        {
            _container = container;
        }

        public LootItemView Create(ItemDefinition definition, int amount, Vector3 position, Quaternion rotation)
        {
            if (definition?.WorldPrefab == null)
                return null;

            var lootItem = _container.InstantiatePrefabForComponent<LootItemView>(
                definition.WorldPrefab,
                position,
                rotation,
                null);

            lootItem.Initialize(definition, amount);

            return lootItem;
        }
    }
}
