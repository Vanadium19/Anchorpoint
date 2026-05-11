namespace InventoryModule
{
    public interface IItemUseHandler
    {
        void UseItem(ItemTable item, object target);
    }
}
