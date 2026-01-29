using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InventoryModule
{
    [CreateAssetMenu(fileName = "ItemCatalog", menuName = "Configs/Inventory/ItemCatalog")]
    public class ItemCatalog : ScriptableObject
    {
        [SerializeField] private List<ItemConfig> items;

        public bool TryGetConfig(ItemName id, out ItemConfig config)
        {
            config = items.FirstOrDefault(i => i.Id == id);
            return config != null;
        }
    }
}