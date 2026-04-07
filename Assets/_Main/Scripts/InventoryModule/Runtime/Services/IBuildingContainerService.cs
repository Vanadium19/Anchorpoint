namespace InventoryModule
{
    public interface IBuildingContainerService
    {
        bool IsContainerOpen(IContainerUI container);
        void OpenContainer(IContainerUI container);
        void CloseContainer(IContainerUI container);
        void CloseAllContainers();
    }
}
