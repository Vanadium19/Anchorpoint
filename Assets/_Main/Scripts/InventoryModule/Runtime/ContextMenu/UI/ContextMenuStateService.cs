using UnityEngine;

namespace InventoryModule.ContextMenu.UI
{
    public sealed class ContextMenuStateService : IContextMenuStateService
    {
        public ContextMenuView ActiveMenu { get; set; }
        public GameObject BlockerInstance { get; set; }
    }
}