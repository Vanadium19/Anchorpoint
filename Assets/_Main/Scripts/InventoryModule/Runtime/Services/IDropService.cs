namespace InventoryModule
{
    public interface IDropService
    {
        bool CanDrop(ItemTable item);
        bool TryDropItem(ItemTable item);
    }
}
