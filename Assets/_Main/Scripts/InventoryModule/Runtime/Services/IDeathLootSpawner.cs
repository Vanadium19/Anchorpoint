namespace InventoryModule
{
    public interface IDeathLootSpawner
    {
        void SpawnStoredPiles();
        void SpawnPile(DeathLootPileData pile);
    }
}
