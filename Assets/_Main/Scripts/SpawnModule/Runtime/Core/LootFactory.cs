using InventoryModule;
using UnityEngine;
using Zenject;

namespace SpawnModule
{
    public class LootFactory : ILootFactory
    {
        private readonly DiContainer _container;

        public LootFactory(DiContainer container)
        {
            _container = container;
        }

        public void Create(Vector3 position, ItemDataSo itemData, int count, bool isStacked = false)
        {
            if (itemData == null || itemData.WorldPrefab == null || count <= 0)
                return;

            var prefabCount = isStacked ? 1 : count;

            for (var itemIndex = 0; itemIndex < prefabCount; itemIndex++)
            {
                var lootObject = _container.InstantiatePrefab(itemData.WorldPrefab);

                if (lootObject == null)
                    continue;

                lootObject.transform.position = position;

                if (!lootObject.TryGetComponent<LootItemView>(out var lootView))
                    continue;

                var itemTable = new ItemTable(itemData);

                if (isStacked)
                    itemTable.AddAmount(count - 1);

                lootView.SetItemTable(itemTable);
            }
        }
    }
}
