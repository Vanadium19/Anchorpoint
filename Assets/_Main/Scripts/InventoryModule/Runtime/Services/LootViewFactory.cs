using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public class LootViewFactory
    {
        private readonly DiContainer _container;

        public LootViewFactory(DiContainer container)
        {
            _container = container;
        }

        public LootItemView Create(LootItemView prefab, Vector3 position)
        {
            return _container.InstantiatePrefabForComponent<LootItemView>(prefab, position, Quaternion.identity, null);
        }
    }
}
