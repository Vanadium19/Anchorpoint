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
            if (itemData == null || itemData.WorldPrefab == null)
                return;

            if (isStacked)
            {
                var lootItem = _container.InstantiatePrefab(itemData.WorldPrefab);

                if (lootItem != null)
                {
                    lootItem.transform.position = position;
                    var lootView = lootItem.GetComponent<LootItemView>();

                    if (lootView != null)
                    {
                        var itemTable = new ItemTable(itemData);
                        itemTable.AddAmount(count - 1);
                        lootView.SetItemTable(itemTable);
                    }
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    var lootItem = _container.InstantiatePrefab(itemData.WorldPrefab);

                    if (lootItem != null)
                        lootItem.transform.position = position;
                }
            }
        }
    }
}
