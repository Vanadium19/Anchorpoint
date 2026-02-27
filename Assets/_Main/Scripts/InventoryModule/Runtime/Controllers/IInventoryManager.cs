namespace InventoryModule
{
    public interface IInventoryManager
    {
        GridTable MainGrid { get; }
        bool IsInventoryOpen { get; }
        void SetMainGrid(GridTable grid);
        void RegisterAdditionalGrid(GridTable grid);
        void UnregisterAdditionalGrid(GridTable grid);
        bool AddItemToInventory(ItemDataSo itemData, int stackCount = 1);
        bool AddExistingItemToInventory(ItemTable existingItem);
        bool TryAutoEquipItem(ItemTable item);
        void SaveEquippedItem(EquipmentSlotType slotType, ItemTable item);
        ItemTable GetEquippedItem(EquipmentSlotType slotType);
        void RemoveEquippedItem(EquipmentSlotType slotType);
        int GetItemCount(ItemDataSo itemData);
        bool TryRemoveItems(ItemDataSo itemData, int count);
        void ToggleInventory();
        void OpenInventory();
        void CloseInventory();
    }
}
