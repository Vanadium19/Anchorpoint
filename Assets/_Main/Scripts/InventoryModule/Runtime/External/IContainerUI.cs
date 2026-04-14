using UnityEngine;

namespace InventoryModule
{
    public interface IContainerUI : IInventoryContainer
    {
        GameObject UIPrefab { get; }
    }
}
