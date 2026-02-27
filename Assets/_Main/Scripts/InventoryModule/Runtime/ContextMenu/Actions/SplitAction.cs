using UnityEngine;

namespace InventoryModule.ContextMenu.Actions
{
    public class SplitAction : ContextActionBase
    {
        public override string DisplayName => DisplayNameOverride ?? "Split";
        public override bool IsAvailable => Item.IsStackable && Item.StackCount > 1;

        public SplitAction(ItemTable item, string displayName) 
            : base(item, displayName)
        {
        }

        public override void Execute()
        {
        }
    }
}
