using UnityEngine;

namespace InventoryModule
{
    public sealed class GridCell
    {
        private InventoryItem _occupyingItem;

        public Vector2Int Position { get; }

        public InventoryItem OccupyingItem => _occupyingItem;
        public bool IsOccupied => _occupyingItem != null;

        public GridCell(Vector2Int position)
        {
            Position = position;
        }

        public void Occupy(InventoryItem item)
        {
            _occupyingItem = item;
        }

        public void Vacate()
        {
            _occupyingItem = null;
        }
    }
}
