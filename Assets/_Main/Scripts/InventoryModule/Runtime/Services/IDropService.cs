namespace InventoryModule
{
    public interface IDropService
    {
        bool TryDropItem(ItemTable item);
    }
}
