namespace InventoryModule
{
    public class LootPickupService
    {
        private readonly IInventoryManager _inventoryManager;
        private readonly IDeathLootStorage _deathLootStorage;

        public LootPickupService(IInventoryManager inventoryManager, IDeathLootStorage deathLootStorage)
        {
            _inventoryManager = inventoryManager;
            _deathLootStorage = deathLootStorage;
        }

        public bool Collect(LootItemView loot)
        {
            if (loot == null || loot.ItemData == null)
                return false;

            var success = loot.ItemData.IsEquippable
                ? CollectEquippable(loot)
                : CollectSimple(loot);

            if (success && loot.ItemTable != null)
                _deathLootStorage.RemoveItem(loot.ItemTable);

            return success;
        }

        private bool CollectEquippable(LootItemView loot)
        {
            if (loot.ItemTable != null)
                return _inventoryManager.TryAutoEquipItem(loot.ItemTable) ||
                       _inventoryManager.AddExistingItemToInventory(loot.ItemTable);

            var newItem = new ItemTable(loot.ItemData) { StackCount = loot.Amount };

            return _inventoryManager.TryAutoEquipItem(newItem) ||
                   _inventoryManager.AddItemToInventory(loot.ItemData, loot.Amount);
        }

        private bool CollectSimple(LootItemView loot)
        {
            return loot.ItemTable != null
                ? _inventoryManager.AddExistingItemToInventory(loot.ItemTable)
                : _inventoryManager.AddItemToInventory(loot.ItemData, loot.Amount);
        }
    }
}
