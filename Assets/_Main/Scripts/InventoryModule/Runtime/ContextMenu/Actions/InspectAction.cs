using UnityEngine;

namespace InventoryModule.ContextMenu.Actions
{
    public class InspectAction : ContextActionBase
    {
        public override string DisplayName => DisplayNameOverride ?? "Inspect";
        public override bool IsAvailable => true;

        public InspectAction(ItemTable item, string displayName) 
            : base(item, displayName)
        {
        }

        public override void Execute()
        {
        }
    }
}
