namespace InventoryModule
{
    public interface IContainerWindowService
    {
        bool IsContainerOpen(ItemTable item);
        void CloseAllWindowsForItem(ItemTable item);
        void RegisterWindow(ItemTable item, ContainerWindow window);
        void UnregisterWindow(ItemTable item, ContainerWindow window);
        void Clear();
    }
}
