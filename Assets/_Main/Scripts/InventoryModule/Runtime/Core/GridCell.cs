using UnityEngine;

namespace InventoryModule
{
    public class GridCell
    {
        public Vector2Int Position { get; }
        public InventoryItem OccupyingItem { get; private set; }
        public bool IsOccupied => OccupyingItem != null;

        public GridCell(Vector2Int position)
        {
            Position = position;
        }

        public void Occupy(InventoryItem item) => OccupyingItem = item;
        public void Vacate() => OccupyingItem = null;
    }
}