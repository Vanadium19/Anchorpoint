using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public interface IGridService
    {
        int Width { get; }
        int Height { get; }

        bool IsCellOccupied(int x, int y);
        bool IsAreaFree(Vector2Int position, Vector2Int size, InventoryItem ignoreItem = null);
        bool CanPlaceItem(InventoryItem item, Vector2Int position, bool isRotated);
        void PlaceItem(InventoryItem item, Vector2Int position, bool isRotated);
        void ClearItem(InventoryItem item);
        void ClearAll();
        bool TrySwapItems(InventoryItem item1, Vector2Int pos1, bool rot1,
                          InventoryItem item2, Vector2Int pos2, bool rot2);
        bool AreasOverlap(Vector2Int pos1, Vector2Int size1, Vector2Int pos2, Vector2Int size2);
        void GetItemsAtArea(Vector2Int position, Vector2Int size, List<InventoryItem> resultBuffer);
        InventoryItem GetItemAtPosition(Vector2Int position);
    }
}
