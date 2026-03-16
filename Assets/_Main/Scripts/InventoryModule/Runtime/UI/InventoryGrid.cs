using UnityEngine;

namespace InventoryModule
{
    public class InventoryGrid : AbstractGrid
    {
        [Header("Item Prefab")]
        [SerializeField] private InventoryItem itemPrefab;

        protected override AbstractItem InstantiateItemPrefab() => DiContainer.InstantiatePrefabForComponent<InventoryItem>(itemPrefab);
    }
}
