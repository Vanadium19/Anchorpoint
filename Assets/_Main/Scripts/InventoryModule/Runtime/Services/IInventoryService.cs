using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

namespace InventoryModule
{
    public interface IInventoryService
    {
        event Action InventoryUpdated;
        int Width { get; }
        int Height { get; }
        UniTask<InventoryOperationResult> AddItemAsync(
            ItemDefinition itemDefinition,
            int amount,
            CancellationToken token);

        UniTask<InventoryOperationResult> MoveItemAsync(
            InventoryItem item,
            Vector2Int newPosition,
            bool isRotated,
            CancellationToken token);

        UniTask<InventoryOperationResult> SplitItemAsync(
            InventoryItem item,
            Vector2Int splitPosition,
            bool isRotated,
            CancellationToken token);

        UniTask<InventoryOperationResult> RemoveItemAsync(
            InventoryItem item,
            CancellationToken token);

        bool CanPlaceItem(ItemDefinition itemDefinition, Vector2Int position, bool isRotated);
        InventoryItem GetItemAtPosition(Vector2Int position);
        IReadOnlyList<InventoryItem> GetAllItems();
    }
}