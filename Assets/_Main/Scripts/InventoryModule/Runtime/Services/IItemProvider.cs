using UnityEngine;

namespace InventoryModule
{
    public interface IItemProvider
    {
        ItemDefinition GetItemDefinition(string itemId);
        bool TryGetItemDefinition(string itemId, out ItemDefinition itemDefinition);
        Sprite GetItemIcon(string itemId);
    }
}