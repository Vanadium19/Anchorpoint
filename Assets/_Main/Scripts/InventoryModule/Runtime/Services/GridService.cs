using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public sealed class GridService
    {
        private readonly GridCell[,] _cells;
        private readonly int _width;
        private readonly int _height;

        public int Width => _width;
        public int Height => _height;

        public GridService(int width, int height)
        {
            _width = width;
            _height = height;
            _cells = new GridCell[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _cells[x, y] = new GridCell(new Vector2Int(x, y));
                }
            }
        }

        public bool IsCellOccupied(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return true;

            return _cells[x, y].IsOccupied;
        }

        public GridCell GetCell(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return null;

            return _cells[x, y];
        }

        public bool IsAreaFree(Vector2Int position, Vector2Int size, InventoryItem ignoreItem = null)
        {
            if (position.x < 0 || position.y < 0 ||
                position.x + size.x > _width || position.y + size.y > _height)
                return false;

            for (int x = position.x; x < position.x + size.x; x++)
            {
                for (int y = position.y; y < position.y + size.y; y++)
                {
                    var cell = GetCell(x, y);
                    if (cell != null && cell.IsOccupied && cell.OccupyingItem != ignoreItem)
                        return false;
                }
            }

            return true;
        }

        public bool CanPlaceItem(InventoryItem item, Vector2Int position, bool isRotated)
        {
            if (item == null)
                return false;

            Vector2Int size = item.GetSize(isRotated);
            return IsAreaFree(position, size, item);
        }

        public void PlaceItem(InventoryItem item, Vector2Int position, bool isRotated)
        {
            Vector2Int size = item.GetSize(isRotated);
            if (!IsAreaFree(position, size, item))
            {
                Debug.LogWarning($"Cannot place item at position {position} with size {size}");
                return;
            }

            ClearItem(item);

            for (int x = position.x; x < position.x + size.x; x++)
            {
                for (int y = position.y; y < position.y + size.y; y++)
                {
                    _cells[x, y].Occupy(item);
                }
            }

            item.Position = position;
            item.IsRotated = isRotated;
        }

        public void ClearItem(InventoryItem item)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_cells[x, y].OccupyingItem == item)
                    {
                        _cells[x, y].Vacate();
                    }
                }
            }
        }

        public bool TrySwapItems(InventoryItem item1, Vector2Int pos1, bool rot1,
                                InventoryItem item2, Vector2Int pos2, bool rot2)
        {
            Vector2Int originalPos1 = item1.Position;
            Vector2Int originalPos2 = item2.Position;
            bool originalRot1 = item1.IsRotated;
            bool originalRot2 = item2.IsRotated;

            ClearItem(item1);
            ClearItem(item2);

            Vector2Int size1 = item1.GetSize(rot1);
            Vector2Int size2 = item2.GetSize(rot2);
            if (!IsAreaFree(pos1, size1) || !IsAreaFree(pos2, size2))
            {
                PlaceItem(item1, originalPos1, originalRot1);
                PlaceItem(item2, originalPos2, originalRot2);
                return false;
            }

            PlaceItem(item1, pos1, rot1);
            PlaceItem(item2, pos2, rot2);
            return true;
        }

        public bool AreasOverlap(Vector2Int pos1, Vector2Int size1, Vector2Int pos2, Vector2Int size2)
        {
            bool overlapX = pos1.x < pos2.x + size2.x && pos1.x + size1.x > pos2.x;
            bool overlapY = pos1.y < pos2.y + size2.y && pos1.y + size1.y > pos2.y;
            return overlapX && overlapY;
        }

        public List<InventoryItem> GetItemsAtArea(Vector2Int position, Vector2Int size)
        {
            var items = new List<InventoryItem>();

            for (int x = position.x; x < position.x + size.x; x++)
            {
                for (int y = position.y; y < position.y + size.y; y++)
                {
                    var cell = GetCell(x, y);
                    if (cell != null && cell.IsOccupied)
                    {
                        if (!items.Contains(cell.OccupyingItem))
                        {
                            items.Add(cell.OccupyingItem);
                        }
                    }
                }
            }

            return items;
        }

        public InventoryItem GetItemAtPosition(Vector2Int position)
        {
            foreach (var cell in _cells)
            {
                if (cell.IsOccupied && cell.OccupyingItem.Position == position)
                {
                    return cell.OccupyingItem;
                }
            }

            return null;
        }

        public void ClearAll()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _cells[x, y].Vacate();
                }
            }
        }
    }
}