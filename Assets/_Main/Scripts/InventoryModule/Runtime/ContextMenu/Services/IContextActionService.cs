using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule.ContextMenu
{
    public interface IContextActionService
    {
        ContainerWindow ContainerWindowPrefab { get; }
        AbstractGrid GridPrefab { get; }
        Canvas Canvas { get; }
        bool IsInitialized { get; }

        IReadOnlyList<IContextAction> GetActions(ItemTable item);
    }
}