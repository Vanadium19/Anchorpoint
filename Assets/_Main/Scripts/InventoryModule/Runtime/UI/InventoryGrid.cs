using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public class InventoryGrid : AbstractGrid
    {
        [Header("Item Prefab")]
        [SerializeField] private InventoryItem itemPrefab;

        protected override AbstractItem InstantiateItemPrefab()
        {
            return DiContainer.InstantiatePrefabForComponent<InventoryItem>(itemPrefab);
        }
    }
}
