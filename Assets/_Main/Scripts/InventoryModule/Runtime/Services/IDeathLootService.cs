using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public interface IDeathLootService
    {
        void CreatePile(IReadOnlyList<ItemTable> items, Vector3 position);
        void RemoveCollectedItem(ItemTable item);
    }
}