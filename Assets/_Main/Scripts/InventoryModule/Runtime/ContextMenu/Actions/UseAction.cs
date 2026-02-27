using UnityEngine;

namespace InventoryModule.ContextMenu.Actions
{
    public class UseAction : ContextActionBase
    {
        public override string DisplayName => DisplayNameOverride ?? "Use";
        public override bool IsAvailable => true;

        public UseAction(ItemTable item, string displayName) 
            : base(item, displayName)
        {
        }

        public override void Execute()
        {
        }
    }
}
