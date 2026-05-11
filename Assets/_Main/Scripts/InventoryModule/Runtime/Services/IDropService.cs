using UnityEngine;

namespace InventoryModule
{
    public interface IDropService
    {
        bool TryDropItem(ItemTable item);
        bool TryDropItem(ItemTable item, Vector3 worldPosition);
    }
}