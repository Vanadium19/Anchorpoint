namespace InventoryModule
{
    public interface ILootable
    {
        ItemConfig Config { get; }
        int Amount { get; }
        void Collect();
    }
}