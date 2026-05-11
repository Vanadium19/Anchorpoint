using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public interface IDeathLootStorage
    {
        DeathLootPileData CreatePile(string sceneName, Vector3 position, List<ItemTable> items);
        IReadOnlyList<DeathLootPileData> GetPiles(string sceneName);
        void RemoveItem(ItemTable item);
    }
}