using System.Collections.Generic;
using UnityEngine;
using InputModule;
using System;

namespace InventoryModule
{
    public interface IInventoryManager
    {
        GridTable MainGrid { get; }
        bool IsInventoryOpen { get; }

        event Action InventoryOpened;
        event Action InventoryClosed;

        void SetMainGrid(GridTable grid);
        void RegisterAdditionalGrid(GridTable grid);
        void UnregisterAdditionalGrid(GridTable grid);
        void SetInput(IInputMap inputMap, IInputService inputService);
        void SetInventoryUI(GameObject inventoryUI);
        void OpenInventory();
        void CloseInventory();

        bool AddItemToInventory(ItemDataSo itemData, int stackCount = 1);
        bool AddExistingItemToInventory(ItemTable existingItem);
        bool TryAutoEquipItem(ItemTable item);
        void SaveEquippedItem(EquipmentSlotType slotType, ItemTable item);
        ItemTable GetEquippedItem(EquipmentSlotType slotType);
        void RemoveEquippedItem(EquipmentSlotType slotType);
        int GetItemCount(ItemDataSo itemData);
        bool TryRemoveItems(ItemDataSo itemData, int count);

        List<ItemTable> ExtractAllRootItems();
        void ClearInventory();
    }
}