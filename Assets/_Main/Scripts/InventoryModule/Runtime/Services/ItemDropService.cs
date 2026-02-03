using ComponentsModule;
using UnityEngine;

namespace InventoryModule
{
    public interface IItemDropService
    {
        void SpawnItemInWorld(ItemDefinition itemDefinition, int amount, IPlayerPositionProvider playerPosition, InventoryConfig config);
    }

    public sealed class ItemDropService : IItemDropService
    {
        private readonly ILootItemFactory _lootItemFactory;

        public ItemDropService(ILootItemFactory lootItemFactory)
        {
            _lootItemFactory = lootItemFactory;
        }

        public void SpawnItemInWorld(ItemDefinition itemDefinition, int amount, IPlayerPositionProvider playerPosition, InventoryConfig config)
        {
            if (itemDefinition?.WorldPrefab == null)
                return;

            Vector3 spawnPosition = playerPosition.Position + playerPosition.Forward * config.DiscardOffset + Vector3.up * config.DiscardUpOffset;

            LootItemView worldItem = _lootItemFactory.Create(itemDefinition, amount, spawnPosition, playerPosition.Rotation);

            if (worldItem == null)
                return;

            if (worldItem.TryGetComponent<Rigidbody>(out var rigidbody))
            {
                Vector3 throwDirection = playerPosition.Rotation * Vector3.forward;
                rigidbody.AddForce(throwDirection * config.DiscardThrowForce, ForceMode.Impulse);
            }
        }
    }
}
