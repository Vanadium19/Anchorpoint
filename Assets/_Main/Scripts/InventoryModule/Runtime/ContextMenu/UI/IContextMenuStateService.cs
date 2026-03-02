using System;
using UnityEngine;

namespace InventoryModule.ContextMenu.UI
{
    public interface IContextMenuStateService
    {
        ContextMenuView ActiveMenu { get; set; }
        GameObject BlockerInstance { get; set; }
        bool HasActiveMenu { get; }
        void HideActiveMenu();
    }
}
