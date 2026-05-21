using InventoryModule;
using UnityEngine;

namespace SpawnModule
{
    public interface ILootFactory
    {
        void Create(Vector3 position, ItemDataSo itemData, int count, bool isStacked = false);
    }
}
