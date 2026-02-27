namespace InventoryModule.ContextMenu
{
    public abstract class ContextActionBase : IContextAction
    {
        protected readonly ItemTable Item;
        protected readonly string DisplayNameOverride;

        public abstract string DisplayName { get; }
        public abstract bool IsAvailable { get; }

        protected ContextActionBase(ItemTable item, string displayNameOverride = null)
        {
            Item = item;
            DisplayNameOverride = displayNameOverride;
        }

        public abstract void Execute();
    }
}
