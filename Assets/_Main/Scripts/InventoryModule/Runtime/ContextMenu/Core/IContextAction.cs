namespace InventoryModule.ContextMenu
{
    public interface IContextAction
    {
        string DisplayName { get; }
        bool IsAvailable { get; }

        void Execute();
    }
}