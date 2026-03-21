using UnityEngine;
using InputModule;

namespace InventoryModule
{
    public interface IInventoryManager
    {
        GridTable MainGrid { get; }

        void SetMainGrid(GridTable grid);
        void RegisterAdditionalGrid(GridTable grid);
        void UnregisterAdditionalGrid(GridTable grid);
        void SetInput(IInputMap inputMap, IInputService inputService);
        void SetInventoryUI(GameObject inventoryUI);

        bool AddItemToInventory(ItemDataSo itemData, int stackCount = 1);
        bool AddExistingItemToInventory(ItemTable existingItem);
        bool TryAutoEquipItem(ItemTable item);
        void SaveEquippedItem(EquipmentSlotType slotType, ItemTable item);
        ItemTable GetEquippedItem(EquipmentSlotType slotType);
        void RemoveEquippedItem(EquipmentSlotType slotType);
        int GetItemCount(ItemDataSo itemData);
        bool TryRemoveItems(ItemDataSo itemData, int count);
        void ClearInventory();
    }
}
