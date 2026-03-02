using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace InventoryModule.ContextMenu
{
    public interface IContextActionService
    {
        IReadOnlyList<IContextAction> GetActions(ItemTable item);
        ContainerWindow ContainerWindowPrefab { get; }
        AbstractGrid GridPrefab { get; }
        Canvas Canvas { get; }
        bool IsInitialized { get; }
        void SetPrefabs(DiContainer sceneContainer, ContainerWindow windowPrefab, AbstractGrid gridPrefab, Canvas canvas);
    }
}
