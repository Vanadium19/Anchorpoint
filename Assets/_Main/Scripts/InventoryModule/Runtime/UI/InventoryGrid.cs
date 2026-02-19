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
            var container = ProjectContext.Instance.Container;
            return container.InstantiatePrefabForComponent<InventoryItem>(itemPrefab);
        }
    }
}
