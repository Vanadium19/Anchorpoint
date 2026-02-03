using System;
using UnityEngine;
using System.Collections.Generic;

namespace InventoryModule
{
    public interface IInventoryService
    {
        event Action InventoryUpdated;
        int Width { get; }
        int Height { get; }
        InventoryOperationResult AddItem(ItemDefinition itemDefinition, int amount);
        InventoryOperationResult MoveItem(InventoryItem item, Vector2Int newPosition, bool isRotated);
        InventoryOperationResult SplitItem(InventoryItem item, Vector2Int splitPosition, bool isRotated);
        InventoryOperationResult RemoveItem(InventoryItem item);
        void UpdateInventory();
        bool CanPlaceItem(ItemDefinition itemDefinition, Vector2Int position, bool isRotated);
        InventoryItem GetItemAtPosition(Vector2Int position);
        IReadOnlyList<InventoryItem> GetAllItems();
        void ClearInventory();
    }
}